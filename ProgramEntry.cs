using Trawler.Config;
using Trawler.Database;
using Trawler.Utility.Logging;

namespace Trawler {
  public class ProgramEntry {
    private static readonly ILogger logger = new ConsoleLogger(nameof(ProgramEntry));

    public static async Task Main(string[] args) {
      logger.Log("Program started");

      await Initialize();
    }

    private static async Task Initialize() {
      logger.Log("Start initialization...");
      
      // Configuration
      await Configuration.Instance.Load();
      
      // Database connection test
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
