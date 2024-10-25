namespace Trawler.Common {
  public static class Constants {
    public const string ConfigFilePath = "config/config.yaml";
    public static string ConfigFileAbsolutePath => Path.GetFullPath(ConfigFilePath);
    public const uint WebDriverInitTimeout = 10;
    public static readonly string[] ChromiumWebDriverArguments = [ "--headless",
                                                                   "--disable-gpu",
                                                                   "--no-sandbox",
                                                                   "--disable-dev-shm-usage" ];
  }
}
