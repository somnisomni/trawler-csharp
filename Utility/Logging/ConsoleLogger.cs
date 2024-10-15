namespace Trawler.Utility.Logging {
  public class ConsoleLogger(string subject) : ILogger {
    public void Log(string message) {
      string date = DateTime.Now.ToString("s") + "Z";

      Console.Out.WriteLine($"({date}) [{subject}] {message}");
    }

    public void LogError(string message) {
      string date = DateTime.Now.ToString("s") + "Z";

      Console.Error.WriteLine($"({date}) [{subject}] ERROR: {message}");
    }
  }
}
