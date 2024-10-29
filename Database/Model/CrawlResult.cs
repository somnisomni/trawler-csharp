using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trawler.Database.Model {
  public sealed class CrawlResultConfiguration : IEntityTypeConfiguration<CrawlResult> {
    public void Configure(EntityTypeBuilder<CrawlResult> builder) {
      builder.ToTable("crawl_results");

      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
      builder.Property(x => x.CrawlTargetId).IsRequired();
      builder.Property(x => x.CrawlDoneAt).IsRequired().ValueGeneratedOnAdd();
      builder.Property(x => x.DisplayName).IsRequired();
      builder.Property(x => x.FollowerCount).IsRequired();
      builder.Property(x => x.FollowingCount).IsRequired();
      builder.Property(x => x.PostCount).IsRequired();
      
      // === Relationships
      builder.HasOne(x => x.CrawlTarget)
        .WithMany(x => x.CrawlResults)
        .HasForeignKey(x => x.CrawlTargetId)
        .IsRequired();
      // ===
    }
  }
  
  [EntityTypeConfiguration(typeof(CrawlResultConfiguration))]
  public record CrawlResult {
    public uint Id { get; }
    public uint CrawlTargetId { get; set; }
    public DateTime CrawlDoneAt { get; set; } = DateTime.UtcNow;
    
    // === Twitter User Data
    public string DisplayName { get; set; }
    public uint FollowerCount { get; set; }
    public uint FollowingCount { get; set; }
    public uint PostCount { get; set; }
    // ===
    
    // === Relationships
    public CrawlTarget CrawlTarget { get; }
    // ===
  }
}