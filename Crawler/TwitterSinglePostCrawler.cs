using System.Text;
using System.Text.Json;
using OpenQA.Selenium;
using Trawler.Utility;
using Trawler.Utility.Extension;
using Trawler.Utility.Logging;

namespace Trawler.Crawler {
  public readonly struct TwitterPostData {
    public ulong Id { get; init; }
    public ulong AuthorId { get; init; }
    // public DateTime CreatedAt { get; init; }  <-- Non-standard human-readable format in API response
    public string TextContent { get; init; }
    public ulong ViewCount { get; init; }
    public ulong BookmarkCount { get; init; }
    public ulong LikesCount { get; init; }
    public ulong RetweetsCount { get; init; }
    public ulong QuotesCount { get; init; }
    public ulong RepliesCount { get; init; }
    public string[] Hashtags { get; init; }
    
    public void DebugPrint() {
      Console.WriteLine();
      Console.WriteLine($"Id: {Id}");
      Console.WriteLine($"AuthorId: {AuthorId}");
      Console.WriteLine($"TextContent: {TextContent}");
      Console.WriteLine($"ViewCount: {ViewCount}");
      Console.WriteLine($"BookmarkCount: {BookmarkCount}");
      Console.WriteLine($"LikesCount: {LikesCount}");
      Console.WriteLine($"RetweetsCount: {RetweetsCount}");
      Console.WriteLine($"QuotesCount: {QuotesCount}");
      Console.WriteLine($"RepliesCount: {RepliesCount}");
      Console.WriteLine($"Hashtags: {string.Join(", ", Hashtags)}");
      Console.WriteLine();
    }
  }
  
  public class TwitterSinglePostCrawler(
    IWebDriver driver,
    string handle,
    ulong postId) : CrawlerBase<TwitterPostData>(driver) {
    private readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterSinglePostCrawler));
    private string BaseUrl {
      get {
        if(handle is { Length: > 0 } && postId > 0) {
          return $"https://twitter.com/{handle}/status/{postId}";
        }
        
        logger.LogError("Handle and Post ID must be valid.");
        throw new ArgumentException("Handle and Post ID must be valid.");
      }
    }
    
    protected override async Task NavigateToTargetAsync() {
      logger.Log("Navigating to post(status) page...");

      await driver.Navigate().GoToUrlAsync(BaseUrl);
    }

    public override async Task<TwitterPostData> DoCrawlAsync() {
      Task<string?> responseWatcher = Task.Run(StartWatchTweetResultResponse);
      
      await NavigateToTargetAsync();

      string? tweetData = await responseWatcher.WaitAsync(Timeout.InfiniteTimeSpan);
      logger.Log($"tweet data: {tweetData}");
      
      TwitterPostData resultData;
      try {
        logger.Log("Start parsing post data...");
        
        // #4-1. Parse JSON
        JsonElement data = (await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(tweetData))))
          .RootElement
          .GetProperty("data")
          .GetProperty("tweetResult")
          .GetProperty("result");

        // #4-2. Extract actual data
        resultData = ParseTwitterPostData(data);
      } catch(Exception e) {
        logger.LogError("Failed to parse post data.", e);
        throw;
      }
      
      resultData.DebugPrint();

      return default;
    }
    
    private async Task<string?> StartWatchTweetResultResponse() {
      // NOTE: `TweetResultByRestId` response is only available when not logged in to Twitter.
      
      logger.Log("Start watching for `TweetResultByRestId` response...");
      
      var network = new NetworkManager(driver);
      string responseBody = null!;
      
      network.NetworkResponseReceived += (_, response) => {
        if(!response.ResponseUrl.Contains("TweetResultByRestId")) return;
        
        logger.Log("Found `TweetResultByRestId` response.");

        if(response.ResponseBody is { Length: <= 0 }) {
          logger.Log("Has empty response body, skipping.");
          return;
        }
        
        logger.Log("Response body seems available. Using this one.");
        responseBody = response.ResponseBody;
        
        network.ClearResponseHandlers();
        network.StopMonitoring();
        network = null;
      };
      await network.StartMonitoring();

      await TaskUtil.WaitForValueAsync(() => responseBody);
      return responseBody;
    }

    private static TwitterPostData ParseTwitterPostData(JsonElement json) {
      JsonElement legacy = json.GetProperty("legacy");
      
      return new TwitterPostData {
        Id = ulong.Parse(legacy.GetProperty("id_str").SafeGetString()),
        AuthorId = ulong.Parse(legacy.GetProperty("user_id_str").SafeGetString()),
        TextContent = legacy.GetProperty("full_text").SafeGetString(),
        ViewCount = ulong.Parse(json.GetProperty("views").GetProperty("count").SafeGetString()),
        BookmarkCount = legacy.GetProperty("bookmark_count").GetUInt64(),
        LikesCount = legacy.GetProperty("favorite_count").GetUInt64(),
        RetweetsCount = legacy.GetProperty("retweet_count").GetUInt64(),
        QuotesCount = legacy.GetProperty("quote_count").GetUInt64(),
        RepliesCount = legacy.GetProperty("reply_count").GetUInt64(),
        Hashtags = legacy.GetProperty("entities").GetProperty("hashtags").EnumerateArray()
          .Select(x => x.GetProperty("text").SafeGetString()).ToArray()
      };
    }
  }
}