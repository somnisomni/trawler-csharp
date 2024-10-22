namespace Trawler.Utility.Logging;

public abstract class LoggerBase : ILogger {
  public abstract void Log(string message);
  public abstract void LogError(string message);
  public abstract void LogError(string message, Exception e);

  protected string DateTimeNow => DateTime.Now.ToString("s") + "Z";
}