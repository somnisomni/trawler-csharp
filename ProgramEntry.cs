using Trawler.Common.Utility.Logging;
using Trawler.Config;
using Trawler.Database;
using Trawler.Processor;

namespace Trawler {
  public static class ProgramEntry {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(ProgramEntry));

    public static async Task Main(string[] args) {
      logger.Log("Program started.");

      await Initialize();
      
      // test
      await CrawlProcessor.Process();
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