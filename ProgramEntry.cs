using OpenQA.Selenium.Chromium;
using Trawler.Config;
using Trawler.Crawler;
using Trawler.Database;
using Trawler.Utility;
using Trawler.Utility.Logging;

namespace Trawler {
  public static class ProgramEntry {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(ProgramEntry));

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
      try {
        await Configuration.Instance.Load();
      } catch {
        logger.LogError("Configuration loading failed.");
        throw;
      }
      
      // *** Database connection test *** //
      if(!await DatabaseContext.TestConnection()) {
        logger.LogError("Database connection should be configured and available to use this program.");
        throw new ApplicationException("Database connection should be configured and available to use this program.");
      }

      logger.Log("Initialization completed.");
    }
  }
}