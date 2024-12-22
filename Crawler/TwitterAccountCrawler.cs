using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using OpenQA.Selenium;
using Trawler.Common.Utility;
using Trawler.Common.Utility.Extension;
using Trawler.Common.Utility.Logging;

namespace Trawler.Crawler {
  public readonly struct TwitterAccountData {
    public ulong Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string ScreenName { get; init; }
    public string DisplayName { get; init; }
    public string Bio { get; init; }
    public string Location { get; init; }
    public string Website { get; init; }
    public uint FollowerCount { get; init; }
    public uint FollowingCount { get; init; }
    public uint PostCount { get; init; }

    public void DebugPrint() {
      Console.WriteLine();
      Console.WriteLine($"Id: {Id}");
      Console.WriteLine($"CreatedAt: {CreatedAt}");
      Console.WriteLine($"ScreenName: {ScreenName}");
      Console.WriteLine($"DisplayName: {DisplayName}");
      Console.WriteLine($"Bio: {Bio}");
      Console.WriteLine($"Location: {Location}");
      Console.WriteLine($"Website: {Website}");
      Console.WriteLine($"FollowerCount: {FollowerCount}");
      Console.WriteLine($"FollowingCount: {FollowingCount}");
      Console.WriteLine($"PostCount: {PostCount}");
      Console.WriteLine();
    }
  }
  
  public class TwitterAccountCrawler(
    IWebDriver driver,
    string? handle = null,
    ulong? accountId = null) : CrawlerBase<TwitterAccountData>(driver) {
    private readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterAccountCrawler));
    private string BaseUrl {
      get {
        if(handle is { Length: > 0 }) {
          return $"https://twitter.com/{handle}";
        }

        if(accountId is > 0) {
          return $"https://twitter.com/i/user/{accountId}";
        }
        
        logger.LogError("Either handle or accountId must be provided.");
        throw new ArgumentException("Either handle or accountId must be provided.");
      }
    }

    protected override async Task NavigateToTargetAsync() {
      logger.Log("Navigating to user profile page...");

      await driver.Navigate().GoToUrlAsync(BaseUrl);
    }
    
    public override async Task<TwitterAccountData> DoCrawlAsync() {
      logger.Log($"*** Start crawling Twitter account data. Target user: {handle ?? accountId.ToString()}");
      
      // #1. Navigate to target
      try {
        await NavigateToTargetAsync();
      } catch(Exception e) {
        logger.LogError("Failed to navigate to the base URL.", e);
        throw;
      }
      
      // #2. Wait for profile page to be loaded (almost) completely
      try {
        logger.Log("Waiting for profile page to be loaded completely...");

        WebDriverUtil.WaitForElementsByCssSelectors(driver, [
          "[data-testid='UserDescription']",
          "[data-testid='UserName']"
        ]);
      } catch(Exception e) {
        logger.LogError("Seems like the profile page is not loaded correctly.", e);
        throw;
      }
      
      // #3. Find user profile data schema element
      string schemaData;
      try {
        logger.Log("Finding user profile data schema...");
        
        // #3-1. Find all script elements
        ReadOnlyCollection<IWebElement>? scripts = driver.FindElements(By.TagName("script"));
        if(scripts is not { Count: > 0 }) {
          throw new ApplicationException("No script element found.");
        }

        // #3-2. Find UserProfileSchema script element among all script elements
        IWebElement? targetSchemaElement = scripts.FirstOrDefault(
          element => element.GetDomAttribute("type") == "application/ld+json"
                     && element.GetDomAttribute("data-testid") == "UserProfileSchema-test");
        if(targetSchemaElement == null) {
          throw new ApplicationException("UserProfileSchema script element not found.");
        }

        // #3-3. Get content of UserProfileSchema script element
        schemaData = targetSchemaElement.GetAttribute("innerHTML");
        if(schemaData is not { Length: > 0 } || !schemaData.StartsWith('{')) {
          throw new ApplicationException("Can't get content of UserProfileSchema script element, or it has no/invalid content.");
        }
        
        logger.Log("User profile data schema found and (seems) valid.");
      } catch(Exception e) {
        logger.LogError("Failed to find user profile data schema.", e);
        throw;
      }
      
      // #4. Parse user profile data
      TwitterAccountData resultData;
      try {
        logger.Log("Start parsing user profile data...");
        
        // #4-1. Parse JSON
        JsonElement data = (await JsonDocument.ParseAsync(new MemoryStream(Encoding.UTF8.GetBytes(schemaData)))).RootElement;

        // #4-2. Extract actual data
        resultData = ParseTwitterAccountSchema(data);
      } catch(Exception e) {
        logger.LogError("Failed to parse user profile data schema.", e);
        throw;
      }
      
      // #5. Done
      logger.Log($"*** User profile data parsed successfully. Done crawling. (User: @{resultData.ScreenName} (#{resultData.Id}))");
      return resultData;
    }
    
    private static TwitterAccountData ParseTwitterAccountSchema(JsonElement json) {
      JsonElement mainEntity = json.GetProperty("mainEntity");
    
      // Extract
      DateTime createdAt = json.GetProperty("dateCreated").GetDateTime();
      string screenName = mainEntity.GetProperty("additionalName").SafeGetString();
      string nickname = mainEntity.GetProperty("givenName").SafeGetString();
      string bio = mainEntity.GetProperty("description").SafeGetString();
      string location = mainEntity.GetProperty("homeLocation").GetProperty("name").SafeGetString();
      ulong id = ulong.Parse(mainEntity.GetProperty("identifier").SafeGetString());
      uint followerCount = 0, followingCount = 0, postCount = 0;
      foreach(JsonElement stat in mainEntity.GetProperty("interactionStatistic").EnumerateArray()) {
        switch(stat.GetProperty("name").SafeGetString().ToLower()) {
          case "follows":
            followerCount = stat.GetProperty("userInteractionCount").GetUInt32();
            break;
          case "friends":
            followingCount = stat.GetProperty("userInteractionCount").GetUInt32();
            break;
          case "tweets":
            postCount = stat.GetProperty("userInteractionCount").GetUInt32();
            break;
        }
      }
      string homepage = mainEntity.GetProperty("url").SafeGetString();

      // Construct
      return new TwitterAccountData {
        CreatedAt = createdAt,
        ScreenName = screenName,
        DisplayName = nickname,
        Bio = bio,
        Location = location,
        Id = id,
        FollowerCount = followerCount,
        FollowingCount = followingCount,
        PostCount = postCount,
        Website = homepage
      };
    }
  }
}