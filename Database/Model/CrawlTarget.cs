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
      
      // === Specific to CrawlTargetType.SinglePost
      builder.Property(x => x.PostCreatedAtUtc).IsRequired(false).HasConversion(
        v => v != null ? v.Value.ToUniversalTime() : (DateTime?)null,
        v => v != null ? v.Value.ToUniversalTime() : null);
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
    
    // === Specific to CrawlTargetType.SinglePost
    public DateTime? PostCreatedAtUtc { get; set; }
    // ===
    
    // === Relationships
    public ICollection<CrawlResultBase> CrawlResults { get; } = new List<CrawlResultBase>();
    // ===
  }
}