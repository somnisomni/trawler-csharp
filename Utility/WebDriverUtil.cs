using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Trawler.Config;

namespace Trawler.Utility {
  public static class WebDriverUtil {
    public static WebDriverWait CreateWait(IWebDriver driver) {
      return new WebDriverWait(driver,
        TimeSpan.FromSeconds(Configuration.Instance.Config.WebDriver.WaitTimeout)) {
        PollingInterval = TimeSpan.FromMilliseconds(200)
      };
    }
    
    public static IWebElement[] WaitForElements(IWebDriver driver, By[] conditions) {
      WebDriverWait wait = CreateWait(driver);
      IWebElement[] finds = new IWebElement[conditions.Length];
      
      for(int i = 0; i < conditions.Length; i++) {
        int idx = i;
        finds[i] = wait.Until(drv => drv.FindElement(conditions[idx]));
      }

      return finds;
    }
  }
}