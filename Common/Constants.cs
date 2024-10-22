namespace Trawler.Common {
  public static class Constants {
    public const string ConfigFilePath = "config.yaml";
    public static string ConfigFileAbsolutePath => Path.GetFullPath(ConfigFilePath);

    public static readonly string[] WebDriverArguments = [
      "--headless",
      "--disable-gpu",
      "--no-sandbox",
      "--disable-dev-shm-usage"
    ];
  }
}