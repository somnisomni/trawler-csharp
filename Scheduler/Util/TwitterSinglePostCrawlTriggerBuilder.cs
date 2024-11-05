using System.Collections.Immutable;
using Quartz;
using Trawler.Crawler;
using Trawler.Database.Model;

namespace Trawler.Scheduler.Util {
  public static class TwitterSinglePostCrawlTriggerBuilder {
    private static readonly ImmutableArray<TimeSpan> CrawlTimeOffsets = [
      TimeSpan.FromMinutes(1),
      TimeSpan.FromMinutes(5),
      TimeSpan.FromMinutes(10),
      TimeSpan.FromMinutes(15),
      TimeSpan.FromMinutes(20),
      TimeSpan.FromMinutes(25),
      TimeSpan.FromMinutes(30),
      TimeSpan.FromMinutes(40),
      TimeSpan.FromMinutes(50),
      TimeSpan.FromHours(1),
      TimeSpan.FromHours(1) + TimeSpan.FromMinutes(30),
      TimeSpan.FromHours(2),
      TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30),
      TimeSpan.FromHours(3),
      TimeSpan.FromHours(4),
      TimeSpan.FromHours(6),
      TimeSpan.FromHours(8),
      TimeSpan.FromHours(10),
      TimeSpan.FromHours(12),
      TimeSpan.FromHours(18),
      TimeSpan.FromDays(1),
      TimeSpan.FromDays(2),
      TimeSpan.FromDays(4),
      TimeSpan.FromDays(7),
      TimeSpan.FromDays(14),
      TimeSpan.FromDays(30),
    ];

    private static ImmutableArray<ITrigger> Build(DateTime postDateTime) {
      DateTime copy = postDateTime;
      return CrawlTimeOffsets.Select(offset => TriggerBuilder.Create()
        .StartAt(copy.Add(offset))
        .Build()).ToImmutableArray();
    }
    
    public static ImmutableArray<ITrigger> Build(TwitterPostData postData) {
      return Build(postData.CreatedAt);
    }

    public static ImmutableArray<ITrigger> Build(CrawlTarget postTarget) {
      if(postTarget.PostCreatedAtUtc == null) throw new ArgumentException("PostCreatedAtUtc is not set in specified CrawlTarget.");
      
      return Build(postTarget.PostCreatedAtUtc.Value);
    }
  }
}