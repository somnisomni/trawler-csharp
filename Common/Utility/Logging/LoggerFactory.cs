namespace Trawler.Common.Utility.Logging {
  public static class LoggerFactory {
    public enum LoggerType {
      Console
    }

    public static LoggerBase CreateLogger(LoggerType type = LoggerType.Console, string? subject = null) {
      return type switch {
        LoggerType.Console => new ConsoleLogger(subject ?? "Default"),
        _ => throw new ArgumentException("Invalid logger type.")
      };
    }
  }
}