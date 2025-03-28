﻿using Microsoft.EntityFrameworkCore;
using Trawler.Common.Utility.Logging;
using Trawler.Config;
using Trawler.Database.Model;

namespace Trawler.Database {
  public partial class DatabaseContext : DbContext {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(DatabaseContext));
    
    public DbSet<CrawlTarget> CrawlTargets { get; init; }
    public DbSet<TwitterAccountCrawlResult> TwitterAccountCrawlResults { get; init; }
    public DbSet<TwitterPostCrawlResult> TwitterPostCrawlResults { get; init; }
    
    public DatabaseContext() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public static async Task<bool> TestConnection() {
      bool result = false;
      
      try {
        await using var db = new DatabaseContext();
        result = await db.Database.CanConnectAsync();
      
        logger.Log("Database connection test successful.");
      } catch(Exception e) {
        logger.LogError("Database connection test failed.", e);
      }

      return result;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      if(Configuration.Instance.IsLoaded == false) {
        // EF tooling environment
        logger.Log("NOTE: Configuration is not loaded, assuming EF tooling environment.");

        var config = Configuration.Instance.LoadForEfMigration();
        optionsBuilder.UseMySql(config.MySql.ConnectionString,
          ServerVersion.AutoDetect(config.MySql.ConnectionString));
        return;
      }

      // Normal environment
      optionsBuilder.UseMySql(Configuration.Instance.Config.MySql.ConnectionString,
        ServerVersion.AutoDetect(Configuration.Instance.Config.MySql.ConnectionString));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      
      modelBuilder.UseCollation("utf8mb4_general_ci").HasCharSet("utf8mb4");
      
      // Base entities
      modelBuilder.Ignore<CrawlResultBase>();

      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}