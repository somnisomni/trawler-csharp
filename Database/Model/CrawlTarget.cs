using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trawler.Database.Model {
  public enum CrawlTargetType {
    Account,
    AccountWorkaround,
    SinglePost
  }
  
  public sealed class CrawlTargetConfiguration : IEntityTypeConfiguration<CrawlTarget> {
    public void Configure(EntityTypeBuilder<CrawlTarget> builder) {
      builder.ToTable("crawl_targets");
      
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
      builder.Property(x => x.CrawlType).IsRequired().HasConversion(t => t.ToString(), t => Enum.Parse<CrawlTargetType>(t));
      builder.Property(x => x.TargetId).IsRequired();
      
      // === Specific to CrawlTargetType.AccountWorkaround
      builder.Property(x => x.WorkaroundPostId).IsRequired(false);
      // ===
    }
  }
  
  [EntityTypeConfiguration(typeof(CrawlTargetConfiguration))]
  public record CrawlTarget {
    public uint Id { get; }
    public CrawlTargetType CrawlType { get; set; }
    public string TargetId { get; set; }
    
    // === Specific to CrawlTargetType.AccountWorkaround
    public string? WorkaroundPostId { get; set; }
    // ===
  }
}