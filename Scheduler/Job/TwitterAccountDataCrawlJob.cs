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
  public class TwitterAccountDataCrawlJob : CrawlJobBase {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterAccountDataCrawlJob));

    protected override async Task<ImmutableArray<CrawlTarget>> AcquireTarget() {
      await using var db = new DatabaseContext();
      
      return (await db.CrawlTargets
        .Where(x => x.CrawlType == CrawlTargetType.Account || x.CrawlType == CrawlTargetType.AccountWorkaround)
        .ToArrayAsync()).ToImmutableArray();
    }

    protected override async Task<CrawlResultBase?> DoCrawl(IWebDriver driver, CrawlTarget target) {
      TwitterAccountData data;
      
      // #1. Actual crawling with target data validation
      if(target.CrawlType == CrawlTargetType.AccountWorkaround) {
        // Workaround crawl strategy
        
        if(target.WorkaroundPostId == null) {
          logger.LogError($"Target #{target.Id} is marked to use workaround strategy for crawling account data, but WorkaroundPostId is not properly set. Skip this target.");  
          return null;
        }

        if(target.TargetScreenName == null) {
          logger.LogError($"Screen name of target #{target.Id} is not properly set. Skip this target.");
          return null;
        }
        
        data = await new TwitterAccountWorkaroundCrawler(driver, target.TargetScreenName, target.WorkaroundPostId.Value).DoCrawlAsync();
      } else {
        // Normal crawl strategy
        
        if(target.TargetId == null && target.TargetScreenName == null) {
          logger.LogError($"Both account ID and screen name of target #{target.Id} are not properly set. Skip this target.");
          return null;
        }
        
        data = await new TwitterAccountCrawler(driver, target.TargetScreenName, target.TargetId).DoCrawlAsync();
      }
      
      // #2. Construct the crawl result and return
      return new TwitterAccountCrawlResult {
        CrawlTargetId = target.Id,
        CrawlDoneAt = DateTime.Now,
        DisplayName = data.DisplayName,
        FollowerCount = data.FollowerCount,
        FollowingCount = data.FollowingCount,
        PostCount = data.PostCount,
      };
    }
  }
}