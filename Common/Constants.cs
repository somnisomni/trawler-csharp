namespace Trawler.Common {
  public static class Constants {
    public const string CONFIG_FILE_PATH = "config/config.yaml";
    public static readonly string[] WEBDRIVER_ARGUMENTS = [ "--headless",
                                                            "--disable-gpu",
                                                            "--no-sandbox",
                                                            "--disable-dev-shm-usage" ];
  }
}
