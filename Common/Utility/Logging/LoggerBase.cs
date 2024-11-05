namespace Trawler.Common.Utility.Logging {
  public abstract class LoggerBase {
    public abstract void Log(string message);
    public abstract void LogError(string message);
    public abstract void LogError(string message, Exception e);
  
    protected static string DateTimeNow => DateTime.Now.ToString("s") + "Z";
  }
}