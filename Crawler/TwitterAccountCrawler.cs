using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Trawler.Config;
using Trawler.Utility.Logging;

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
  
  public class TwitterAccountCrawler(IWebDriver driver, string? handle = null, ulong? accountId = null) : CrawlerBase<TwitterAccountData>(driver) {
    private readonly LoggerBase logger = new ConsoleLogger(nameof(TwitterAccountCrawler));
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

    public override async Task NavigateToTargetAsync() {
      await driver.Navigate().GoToUrlAsync(BaseUrl);
    }
    
    public override async Task<TwitterAccountData> DoCrawlAsync() {
      logger.Log($"*** Start crawling Twitter account data. Target user: {handle ?? accountId.ToString()}");
      
      // #1. Navigate to target
      try {
        logger.Log("Navigating to user profile page...");
        
        await NavigateToTargetAsync();
      } catch(Exception e) {
        logger.LogError("Failed to navigate to the base URL.", e);
        throw;
      }
      
      // #2. Wait for profile page to be loaded (almost) completely
      try {
        logger.Log("Waiting for profile page to be loaded completely...");
        
        WebDriverWait wait = new WebDriverWait(driver,
          TimeSpan.FromSeconds(Configuration.Instance.Config.WebDriver.WaitTimeout));
        wait.PollingInterval = TimeSpan.FromMilliseconds(200);
        IWebElement[] finds = [
          wait.Until(drv => drv.FindElement(By.CssSelector("[data-testid='primaryColumn']"))),
          wait.Until(drv => drv.FindElement(By.CssSelector("[data-testid='UserName']")))
          // Since script elements can't be found by CSS selector, we will find them later
        ];
        
        if(finds.Any(e => e == null)) {
          throw new ApplicationException("Some of wait target elements are not found.");
        }
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
        if(schemaData is not { Length: > 0 } || !schemaData.StartsWith("{")) {
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
        JsonElement authorElement = data.GetProperty("author");
        
        DateTime createdAt = data.GetProperty("dateCreated").GetDateTime();
        string screenName = authorElement.GetProperty("additionalName").GetString();
        string nickname = authorElement.GetProperty("givenName").GetString();
        string bio = authorElement.GetProperty("description").GetString();
        string location = authorElement.GetProperty("homeLocation").GetProperty("name").GetString();
        ulong id = ulong.Parse(authorElement.GetProperty("identifier").GetString());
        uint followerCount = 0, followingCount = 0, postCount = 0;
        foreach(JsonElement stat in authorElement.GetProperty("interactionStatistic").EnumerateArray()) {
          switch(stat.GetProperty("name").GetString().ToLower()) {
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
        string homepage = authorElement.GetProperty("url").GetString();

        // #4-3. Create TwitterAccountData object with extracted data
        resultData = new TwitterAccountData {
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
      } catch(Exception e) {
        logger.LogError("Failed to parse user profile data schema.", e);
        throw;
      }
      
      logger.Log($"*** User profile data parsed successfully. Done crawling. (User: @{resultData.ScreenName} (#{resultData.Id}))");
      return resultData;
    }
  }
}