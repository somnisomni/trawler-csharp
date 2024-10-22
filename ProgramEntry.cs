using Trawler.Config;
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
    }
  }
}
