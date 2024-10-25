using OpenQA.Selenium.Chromium;
using Trawler.Config;
using Trawler.Crawler;
using Trawler.Database;
using Trawler.Utility;
using Trawler.Utility.Logging;

namespace Trawler {
  public static class ProgramEntry {
    private static readonly ILogger logger = new ConsoleLogger(nameof(ProgramEntry));

    public static async Task Main(string[] args) {
      logger.Log("Program started.");

      await Initialize();
      
      // test
      {
        using ChromiumDriver drv = WebDriverUtil.CreateChromiumDriver();
        
        (await new TwitterAccountCrawler(drv, "nasa").DoCrawlAsync()).DebugPrint();
        (await new TwitterAccountWorkaroundCrawler(drv, "nasa", 1849140528631173326).DoCrawlAsync()).DebugPrint();
      }
    }

    private static async Task Initialize() {
      logger.Log("Start initialization...");
      
      // *** Configuration *** //
      await Configuration.Instance.Load();
      
      // *** Database connection test *** //
      await using(var db = new DatabaseContext()) {
        try {
          if(await db.Database.CanConnectAsync()) {
            logger.Log("Database connection test successful.");
          }
        } catch(Exception e) {
          logger.LogError("Database connection test failed.", e);
        }
      }
      
      logger.Log("Initialization completed.");
    }
  }
}
