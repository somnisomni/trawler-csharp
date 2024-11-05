using System.Text;
using Trawler.Common;
using Trawler.Common.Utility.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Trawler.Config {
  public sealed class Configuration {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(Configuration));
    
    private static Configuration? instance = null;
    public static Configuration Instance => instance ??= new Configuration();

    public ConfigRoot Config { get; private set; } = new();
    public bool IsLoaded { get; private set; } = false;
    
    private Configuration() { }

    public async Task Load() {
      logger.Log($"Start loading configuration...");

      ConfigRoot config;
      try {
        var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();
        string configRaw = await ReadConfigFileAsync();
        
        logger.Log("Parsing configuration...");
        config = deserializer.Deserialize<ConfigRoot>(configRaw);
      } catch(Exception e) {
        logger.LogError("Error while parsing configuration.", e);
        throw;
      }

      if(config == null) {
        logger.LogError("Parsed configuration is null.");
        throw new ApplicationException();
      }

      logger.Log("Configuration loaded successfully.");
      Config = config;
      IsLoaded = true;
    }

    public ConfigRoot LoadForEfMigration() {
      var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
      string configRaw = File.ReadAllText(Constants.ConfigFileAbsolutePath, Encoding.UTF8);

      return deserializer.Deserialize<ConfigRoot>(configRaw);
    }

    private async Task<string> ReadConfigFileAsync() {
      string configFilePath = Constants.ConfigFileAbsolutePath;

      logger.Log($"Reading configuration file: {configFilePath}");

      if(!File.Exists(configFilePath)) {
        logger.LogError("Configuration file is likely not exist.");
        throw new FileNotFoundException();
      }

      string configRaw;
      try {
        configRaw = await File.ReadAllTextAsync(
          path: configFilePath,
          encoding: Encoding.UTF8);
      } catch(Exception e) {
        logger.LogError("Error while reading configuration file.", e);
        throw;
      }

      if(configRaw is not { Length: > 0 }) {
        logger.LogError("Configuration file is likely empty.");
        throw new ApplicationException();
      }

      logger.Log("Configuration file read successfully.");
      return configRaw;
    }
  }

  public sealed class ConfigRoot {
    [YamlMember(Alias = "mysql")]
    public MySqlDatabaseConfig MySql { get; set; } = new();

    [YamlMember(Alias = "webdriver")]
    public WebDriverConfig WebDriver { get; set; } = new();
    
    [YamlMember(Alias = "scheduler")]
    public SchedulerConfig Scheduler { get; set; } = new();
  }

  public sealed record MySqlDatabaseConfig {
    public string Host { get; init; } = "localhost";
    public ushort Port { get; init; } = 3306;
    public string User { get; init; } = "user";
    public string Password { get; init; } = "password";
    public string Database { get; init; } = "trawler";
    
    [YamlIgnore]
    public string ConnectionString => $"Server={Host}; Port={Port}; User={User}; Password={Password}; Database={Database}";
  }

  public sealed record WebDriverConfig {
    public uint WaitTimeout { get; init; } = 10;
    public string? CustomUserAgent { get; init; } = null;
    public string[] AdditionalArguments { get; init; } = [];
  }

  public sealed record SchedulerConfig {
    public string DefaultTimezone { get; init; } = "Etc/UTC";
  }
}
