namespace Trawler.Utility.Logging {
  public interface ILogger {
    public void Log(string message);
    public void LogError(string message);
    public void LogError(string message, Exception e);
  }
}
