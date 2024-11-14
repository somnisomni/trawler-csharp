using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Quartz;
using Trawler.Common.Utility;
using Trawler.Common.Utility.Logging;
using Trawler.Crawler;
using Trawler.Database;
using Trawler.Database.Model;

namespace Trawler.Scheduler.Job {
  [DisallowConcurrentExecution]
  public class TwitterSinglePostCrawlJob : CrawlJobBase {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterSinglePostCrawlJob));

    private uint targetDbId = uint.MaxValue;

    public override Task Execute(IJobExecutionContext context) {
      targetDbId = (uint)context.MergedJobDataMap.GetLongValue("targetDbId");
      logger.Log($"Job CrawlTarget DB ID: {targetDbId}");
      
      return base.Execute(context);
    }

    protected override async Task<ImmutableArray<CrawlTarget>> AcquireTarget() {
      await using var db = new DatabaseContext();
      
      return (await db.CrawlTargets
        .Where(x => x.Id == targetDbId && x.CrawlType == CrawlTargetType.SinglePost)
        .ToArrayAsync()).ToImmutableArray();
    }

    protected override async Task<CrawlResultBase?> DoCrawl(IWebDriver driver, CrawlTarget target) {
      // #1. Target data validation
      if(target.TargetId == null || target.TargetScreenName == null) {
        logger.LogError($"Both target post ID and screen name of target #{target.Id} are not properly set. Skip this target.");
        return null;
      }

      // #2. Actual crawling
      TwitterPostData data = await new TwitterSinglePostCrawler(driver, target.TargetScreenName, target.TargetId.Value).DoCrawlAsync();
      
      // #3. Construct the crawl result and return
      return new TwitterPostCrawlResult {
        CrawlTargetId = target.Id,
        CrawlDoneAt = DateTime.Now,
        ViewCount = data.ViewCount,
        BookmarkCount = data.BookmarkCount,
        LikesCount = data.LikesCount,
        RetweetsCount = data.RetweetsCount,
        QuotesCount = data.QuotesCount,
        RepliesCount = data.RepliesCount
      };
    }

    public static async Task AcquirePostCreationDateForTargets() {
      logger.Log("Acquiring post creation date for targets if needed...");

      // #1. Seek for targets to update
      CrawlTarget[] updateTargets;
      try {
        await using var db = new DatabaseContext();
        updateTargets = await db.CrawlTargets
          .Where(x => x.CrawlType == CrawlTargetType.SinglePost)
          .Where(x => x.PostCreatedAtUtc == null)
          .ToArrayAsync();
        
        logger.Log($"Total {updateTargets.Length} target(s) need update post creation date.");
      } catch(Exception e) {
        logger.LogError("Failed to acquire targets to update.", e);
        return;
      }

      if(updateTargets.Length <= 0) {
        logger.Log("No targets should be updated. Done.");
        return;
      }

      // #2. Create WebDriver
      IWebDriver driver = WebDriverUtil.CreateChromiumDriver();

      // #3. Update post creation date
      foreach(CrawlTarget target in updateTargets) {
        // #3-1. Target data validation
        if(target.TargetId == null || target.TargetScreenName == null) {
          logger.LogError($"Both target post ID and screen name of target #{target.Id} are not properly set. Skip this target.");
          continue;
        }
        
        // #3-2. Do crawl
        TwitterPostData data = await new TwitterSinglePostCrawler(driver, target.TargetScreenName, target.TargetId.Value).DoCrawlAsync();
        
        // #3-3. Update target column with post creation date
        target.PostCreatedAtUtc = data.CreatedAt.ToUniversalTime();
        await using var db = new DatabaseContext();
        db.CrawlTargets.Update(target);
        await db.SaveChangesAsync();
      }
    }
  }
}