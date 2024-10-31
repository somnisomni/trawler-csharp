using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;
using Trawler.Crawler;
using Trawler.Database;
using Trawler.Database.Model;
using Trawler.Utility;
using Trawler.Utility.Logging;

namespace Trawler.Processor {
  public static class CrawlProcessor {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(CrawlProcessor));
    
    public static async Task Process() {
      logger.Log("Start crawling process.");
      
      // #1. Create chromium driver
      using ChromiumDriver driver = WebDriverUtil.CreateChromiumDriver();
      
      // #2. Acquire crawl targets
      logger.Log("Acquiring crawl targets from the database...");
      
      List<CrawlTarget> targets = [];

      try {
        await using var db = new DatabaseContext();
        targets = await db.CrawlTargets.ToListAsync();

        logger.Log($"Total {targets.Count} target(s) acquired.");
      } catch(Exception e) {
        logger.LogError("Failed to acquire crawl targets from the database.", e);
        return;
      }

      if(targets.Count <= 0) {
        logger.Log("No targets to crawl. Stop crawling process.");
        return;
      }
      
      // #3. Do crawl
      List<CrawlResultBase> results = [];
      
      foreach(CrawlTarget target in targets) {
        logger.Log($"***** Processing target #{target.Id}...");

        try {
          switch(target.CrawlType) {
            case CrawlTargetType.Account:
            case CrawlTargetType.AccountWorkaround: {
              TwitterAccountCrawlResult? result = await DoCrawlAccountAsync(driver, target, target.CrawlType == CrawlTargetType.AccountWorkaround);
              
              if(result == null) {
                logger.LogError("Error occurred while crawling account data. No result data will be stored.");
                continue;
              }
              
              results.Add(result);
              break;
            }

            case CrawlTargetType.SinglePost: {
              TwitterPostCrawlResult? result = await DoCrawlSinglePostAsync(driver, target);
              
              if(result == null) {
                logger.LogError("Error occurred while crawling single post data. No result data will be stored.");
                continue;
              }
              
              results.Add(result);
              continue;
            }

            default: {
              logger.LogError("Unknown crawl target type. Skip this target.");
              continue;
            }
          }
          
          logger.Log($"***** Target #{target.Id} crawled successfully.");
        } catch(Exception e) {
          logger.LogError($"Failed to crawl target #{target.Id}.", e);
          logger.Log("Anyway skip this target and continue to the next target.");
        }
      }
      
      // #4. Save crawl results
      logger.Log("Saving crawl results to the database...");
      
      try {
        await using var db = new DatabaseContext();
        await db.AddRangeAsync(results);
        await db.SaveChangesAsync();
        
        logger.Log($"Crawl results saved successfully. Total {results.Count} (out of {targets.Count}) result(s) saved.");
      } catch(Exception e) {
        logger.LogError("Failed to save crawl results to the database.", e);
        return;
      }
      
      // Done
      logger.Log("Crawling process done.");
    }

    private static async Task<TwitterAccountCrawlResult?> DoCrawlAccountAsync(IWebDriver driver, CrawlTarget target, bool useWorkaround = false) {
      TwitterAccountData data;
      
      // #1. Actual crawling with target data validation
      if(useWorkaround) {
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
    
    private static async Task<TwitterPostCrawlResult?> DoCrawlSinglePostAsync(IWebDriver driver, CrawlTarget target) {
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
  }
}