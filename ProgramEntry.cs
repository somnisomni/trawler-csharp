using Trawler.Utility.Logging;

namespace Trawler {
  public class ProgramEntry {
    private static readonly ILogger logger = new ConsoleLogger(nameof(ProgramEntry));

    public static void Main(string[] args) {
      logger.Log("Program started");
    }

    private static void Initialize() {

    }
  }
}
