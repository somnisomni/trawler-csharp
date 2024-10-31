using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trawler.Database.Model {
  public class CrawlResultBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : CrawlResultBase {
    public virtual void Configure(EntityTypeBuilder<TEntity> builder) {
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
      builder.Property(x => x.CrawlTargetId).IsRequired();
      builder.Property(x => x.CrawlDoneAt).IsRequired().ValueGeneratedOnAdd();
    
      // === Relationships
      builder.HasOne(x => x.CrawlTarget)
        .WithMany(x => x.CrawlResults as IEnumerable<TEntity>)
        .HasForeignKey(x => x.CrawlTargetId)
        .IsRequired();
      // ===
    }
  }
  
  public abstract record CrawlResultBase {
    public uint Id { get; }
    public uint CrawlTargetId { get; set; }
    public DateTime CrawlDoneAt { get; set; } = DateTime.UtcNow;
  
    // === Relationships
    public CrawlTarget CrawlTarget { get; }
    // ===
  }
}