using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using Quartz;
using Trawler.Common.Utility.Logging;
using Trawler.Crawler;
using Trawler.Database;
using Trawler.Database.Model;

namespace Trawler.Scheduler.Job {
  [DisallowConcurrentExecution]
  public class TwitterSinglePostCrawlJob : CrawlJobBase {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterSinglePostCrawlJob));

    protected override async Task<ImmutableArray<CrawlTarget>> AcquireTarget() {
      await using var db = new DatabaseContext();
      
      return (await db.CrawlTargets
        .Where(x => x.CrawlType == CrawlTargetType.SinglePost)
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
        PostCreatedAtUtc = data.CreatedAt,
        ViewCount = data.ViewCount,
        BookmarkCount = data.BookmarkCount,
        LikesCount = data.LikesCount,
        RetweetsCount = data.RetweetsCount,
        QuotesCount = data.QuotesCount,
        RepliesCount = data.RepliesCount
      };
    }
  }
}