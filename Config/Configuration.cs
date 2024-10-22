using System.Text;
using Trawler.Common;
using Trawler.Utility.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Trawler.Config {
  public sealed class Configuration {
    private static readonly LoggerBase logger = new ConsoleLogger(nameof(Configuration));
    
    private static Configuration? instance = null;
    public static Configuration Instance => instance ??= new Configuration();

    public ConfigRoot? Config { get; private set; }
    
    private Configuration() { }

    public async Task Load() {
      logger.Log($"Start loading configuration...");

      ConfigRoot config = null;
      try {
        var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();
        string configRaw = await ReadConfigFileAsync();
        
        logger.Log("Deserializing configuration...");
        config = deserializer.Deserialize<ConfigRoot>(configRaw);
      } catch(Exception e) {
        logger.LogError("Error while deserializing configuration.", e);
        throw;
      }
      
      if(config == null) {
        logger.LogError("Deserialized configuration is null.");
        throw new ApplicationException();
      }

      logger.Log("Configuration loaded successfully.");
      Config = config;
    }

    private async Task<string> ReadConfigFileAsync() {
      string configFilePath = Constants.ConfigFileAbsolutePath;

      logger.Log($"Reading configuration file: {configFilePath}");

      if(!File.Exists(configFilePath)) {
        logger.LogError("Configuration file is likely not exist.");
        throw new FileNotFoundException();
      }

      string configRaw = null;

      try {
        configRaw = await File.ReadAllTextAsync(
          path: configFilePath,
          encoding: Encoding.UTF8);
      } catch(Exception e) {
        logger.LogError("Error while reading configuration file.", e);
        throw;
      }

      if(configRaw == null || configRaw.Length <= 0) {
        logger.LogError("Configuration file is likely empty.");
        throw new ApplicationException();
      }

      logger.Log("Configuration file read successfully.");
      return configRaw;
    }
  }

  public sealed class ConfigRoot {
    [YamlMember(Alias = "mysql")]
    public MySqlDatabaseConfig? MySql { get; set; }
    
    [YamlMember(Alias = "webdriver")]
    public WebDriverConfig? WebDriver { get; set; }
  }
  
  public sealed record MySqlDatabaseConfig {
    public string? Host { get; init; } = "localhost";
    public ushort? Port { get; init; } = 3306;
    public required string User { get; init; } = "user";
    public required string Password { get; init; } = "password";
    public string? Database { get; init; } = "trawler";
  }

  public sealed record WebDriverConfig {
    public string? CustomDriverPath { get; init; } = null;
    public string? AdditionalArguments { get; init; } = null;
  }
}
