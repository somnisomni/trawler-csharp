using Microsoft.EntityFrameworkCore;
using Trawler.Config;
using Trawler.Utility.Logging;

namespace Trawler.Database {
  public partial class DatabaseContext : DbContext {
    private static readonly LoggerBase logger = new ConsoleLogger(nameof(DatabaseContext));
    
    public DatabaseContext() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      => optionsBuilder.UseMySql(Configuration.Instance.Config.MySql.ConnectionString,
        ServerVersion.AutoDetect(Configuration.Instance.Config.MySql.ConnectionString));

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.UseCollation("utf8mb4_general_ci").HasCharSet("utf8mb4");

      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
  }
}