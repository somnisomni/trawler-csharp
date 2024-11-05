namespace Trawler.Common.Utility.Logging {
  public class ConsoleLogger(string subject) : LoggerBase {
    public override void Log(string message) {
      Console.Out.WriteLine($"({DateTimeNow}) [{subject}] {message}");
    }

    public override void LogWarning(string message) {
      Console.Out.WriteLine($"({DateTimeNow}) [{subject}] WARNING: {message}");
    }

    public override void LogWarning(string message, Exception e) {
      Console.Out.WriteLine($"({DateTimeNow}) [{subject}] WARNING: {message}");
      Console.Out.WriteLine($"  \u2514 Exception: {e}");
    }

    public override void LogError(string message) {
      Console.Error.WriteLine($"({DateTimeNow}) [{subject}] ERROR: {message}");
    }

    public override void LogError(string message, Exception e) {
      Console.Error.WriteLine($"({DateTimeNow}) [{subject}] ERROR: {message}");
      Console.Error.WriteLine($"  \u2514 Exception: {e}");
    }
  }
}