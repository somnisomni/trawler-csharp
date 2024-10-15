namespace Trawler.Config {
  public sealed class Configuration {
    private static Configuration? instance = null;
    public static Configuration Instance => instance ??= new Configuration();

    private Configuration() { }

    public void Load() {
      // TODO: load config file
    }
  }
}
