using Microsoft.EntityFrameworkCore;
using Trawler.Config;
using Trawler.Database.Model;
using Trawler.Utility.Logging;

namespace Trawler.Database {
  public partial class DatabaseContext : DbContext {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(DatabaseContext));
    
    public DbSet<CrawlTarget> CrawlTargets { get; set; }

    public DatabaseContext() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      if(Configuration.Instance.Config == null) {
        // EF tooling environment
        logger.Log("NOTE: Configuration is not loaded, assuming EF tooling environment.");

        var config = Configuration.Instance.LoadForEfMigration();
        optionsBuilder.UseMySql(config.MySql.ConnectionString,
          ServerVersion.AutoDetect(config.MySql.ConnectionString));
        return;
      }

      // Normal environment
      optionsBuilder.UseMySql(Configuration.Instance.Config!.MySql!.ConnectionString,
        ServerVersion.AutoDetect(Configuration.Instance.Config!.MySql!.ConnectionString));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.UseCollation("utf8mb4_general_ci").HasCharSet("utf8mb4");

      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}