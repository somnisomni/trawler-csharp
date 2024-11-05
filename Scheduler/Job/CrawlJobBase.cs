using System.Collections.Immutable;
using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;
using Quartz;
using Trawler.Common.Utility;
using Trawler.Common.Utility.Logging;
using Trawler.Database;
using Trawler.Database.Model;

namespace Trawler.Scheduler.Job {
  public abstract class CrawlJobBase : IJob {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(CrawlJobBase));

    public virtual async Task Execute(IJobExecutionContext context) {
      logger.Log("Crawling job started.");
      
      // #1. Create chromium driver
      using ChromiumDriver driver = WebDriverUtil.CreateChromiumDriver();
      
      // #2. Acquire crawl targets
      ImmutableArray<CrawlTarget> targets;
      {
        logger.Log("Acquiring crawl targets...");
      
        try {
          targets = await AcquireTarget();

          logger.Log($"Total {targets.Length} target(s) acquired.");
        } catch(Exception e) {
          logger.LogError("Failed to acquire crawl targets.", e);
          return;
        }

        if(targets.Length <= 0) {
          logger.Log("No targets to crawl. Stop crawling process.");
          return;
        }
      }
      
      // #3. Do crawl
      List<CrawlResultBase> results = [];
      {
        foreach(CrawlTarget target in targets) {
          logger.Log($"***** Processing target #{target.Id}...");

          try {
            CrawlResultBase? result = await DoCrawl(driver, target);

            if(result == null) {
              logger.LogError("Error occurred while crawling data (result is null). No result data will be stored.");
              continue;
            }
            
            results.Add(result);
            logger.Log($"***** Target #{target.Id} crawled successfully.");
          } catch(Exception e) {
            logger.LogError($"Failed to crawl target #{target.Id}.", e);
            logger.Log("Anyway skip this target and continue to the next target.");
          }
        }
      }
      
      
      // #4. Save crawl results
      logger.Log("Saving crawl result(s) to the database...");
      
      try {
        await SaveResultToDatabase(results.ToImmutableArray());
        
        logger.Log($"Crawl result(s) saved successfully. Total {results.Count} (out of {targets.Length}) result(s) saved.");
      } catch(Exception e) {
        logger.LogError("Failed to save crawl result(s) to the database.", e);
        return;
      }
      
      // Done
      logger.Log("Crawling job done.");
    }

    protected abstract Task<ImmutableArray<CrawlTarget>> AcquireTarget();
    protected abstract Task<CrawlResultBase?> DoCrawl(IWebDriver driver, CrawlTarget target);
    protected virtual async Task SaveResultToDatabase(ImmutableArray<CrawlResultBase> results) {
      await using var db = new DatabaseContext();
      await db.AddRangeAsync(results);
      await db.SaveChangesAsync();
    }
  }
}