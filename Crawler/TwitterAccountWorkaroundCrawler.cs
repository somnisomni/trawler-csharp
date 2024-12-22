using OpenQA.Selenium;
using Trawler.Common.Utility;
using Trawler.Common.Utility.Logging;

namespace Trawler.Crawler {
  public class TwitterAccountWorkaroundCrawler(
    IWebDriver driver,
    string handle,
    ulong postId) : TwitterAccountCrawler(driver, handle) {
    private readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(TwitterAccountWorkaroundCrawler));
  
    protected override async Task NavigateToTargetAsync() {
      logger.Log("*** Workaround (post-in-the-middle) mode");
    
      // #1. Navigate to the post page
      try {
        logger.Log($"Navigating to the specified post page... (#{postId})");
        await driver.Navigate().GoToUrlAsync($"https://twitter.com/{handle}/status/{postId}");
      } catch(Exception e) {
        logger.LogError("Failed to navigate to the post page.", e);
        throw;
      }
    
      // #2. Wait for the post page to be loaded
      try {
        logger.Log("Waiting for the post page to be loaded completely...");

        WebDriverUtil.WaitForElementsByCssSelectors(driver, [
          "[data-testid='UserDescription']",
          "[data-testid='User-Name']",
        ]);
      } catch(Exception e) {
        logger.LogError("Seems like the post page is not loaded correctly.", e);
        throw;
      }
    
      // #3. Navigate to profile page by clicking username anchor
      try {
        logger.Log("Navigating to the user profile page...");
      
        IWebElement userNameAnchor = driver
          .FindElement(By.CssSelector("[data-testid='User-Name']"))
          .FindElement(By.TagName("a"));
        userNameAnchor.Click();
      } catch(Exception e) {
        logger.LogError("Failed to navigate to the user profile page.", e);
        throw;
      }
    }
  }
}