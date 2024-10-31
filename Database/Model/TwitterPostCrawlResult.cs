using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trawler.Database.Model {
  public sealed class TwitterPostCrawlResultConfiguration : CrawlResultBaseConfiguration<TwitterPostCrawlResult> {
    public override void Configure(EntityTypeBuilder<TwitterPostCrawlResult> builder) {
      builder.ToTable("twitter_post_crawl_results");
      
      base.Configure(builder);
    }
  }
  
  [EntityTypeConfiguration(typeof(TwitterPostCrawlResultConfiguration))]
  public record TwitterPostCrawlResult : CrawlResultBase {
    public ulong ViewCount { get; set; }
    public uint BookmarkCount { get; set; }
    public uint LikesCount { get; set; }
    public uint RetweetsCount { get; set; }
    public uint QuotesCount { get; set; }
    public uint RepliesCount { get; set; }
  }
}