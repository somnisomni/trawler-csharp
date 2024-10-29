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
      builder.Property(x => x.CreatedAt).IsRequired().ValueGeneratedOnAdd();
      builder.Property(x => x.CrawlType).IsRequired().HasConversion(t => t.ToString(), t => Enum.Parse<CrawlTargetType>(t));
      builder.Property(x => x.TargetScreenName).IsRequired(false);
      builder.Property(x => x.TargetId).IsRequired(false);
      
      // === Specific to CrawlTargetType.AccountWorkaround
      builder.Property(x => x.WorkaroundPostId).IsRequired(false);
      // ===
      
      // === Relationships
      builder.HasMany(x => x.CrawlResults)
        .WithOne(x => x.CrawlTarget)
        .HasForeignKey(x => x.CrawlTargetId)
        .IsRequired();
      // ===
    }
  }
  
  [EntityTypeConfiguration(typeof(CrawlTargetConfiguration))]
  public record CrawlTarget {
    public uint Id { get; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CrawlTargetType CrawlType { get; set; }
    public string? TargetScreenName { get; set; }
    public ulong? TargetId { get; set; }
    
    // === Specific to CrawlTargetType.AccountWorkaround
    public ulong? WorkaroundPostId { get; set; }
    // ===
    
    // === Relationships
    public ICollection<CrawlResult> CrawlResults { get; } = new List<CrawlResult>();
    // ===
  }
}