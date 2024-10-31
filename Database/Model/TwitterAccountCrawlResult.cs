using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trawler.Database.Model {
  public sealed class TwitterAccountCrawlResultConfiguration : CrawlResultBaseConfiguration<TwitterAccountCrawlResult> {
    public override void Configure(EntityTypeBuilder<TwitterAccountCrawlResult> builder) {
      builder.ToTable("twitter_account_crawl_results");

      base.Configure(builder);
      builder.Property(x => x.DisplayName).IsRequired();
      builder.Property(x => x.FollowerCount).IsRequired();
      builder.Property(x => x.FollowingCount).IsRequired();
      builder.Property(x => x.PostCount).IsRequired();
    }
  }
  
  [EntityTypeConfiguration(typeof(TwitterAccountCrawlResultConfiguration))]
  public record TwitterAccountCrawlResult : CrawlResultBase {
    // === Twitter User Data
    public string DisplayName { get; set; }
    public uint FollowerCount { get; set; }
    public uint FollowingCount { get; set; }
    public uint PostCount { get; set; }
    // ===
  }
}