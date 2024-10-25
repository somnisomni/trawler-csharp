using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using Trawler.Common;
using Trawler.Config;

namespace Trawler.Utility {
  public static class WebDriverUtil {
    public static ChromiumDriver CreateChromiumDriver(bool msEdge = false) {
      ChromiumDriverService service = msEdge
        ? EdgeDriverService.CreateDefaultService()
        : ChromeDriverService.CreateDefaultService();
      service.HideCommandPromptWindow = true;
      service.SuppressInitialDiagnosticInformation = true;
      service.InitializationTimeout = TimeSpan.FromSeconds(Constants.WebDriverInitTimeout);
      
      ChromiumOptions options = msEdge
        ? new EdgeOptions()
        : new ChromeOptions();
      options.AddArguments(Constants.ChromiumWebDriverArguments);
      options.AddArgument($"--user-agent={Configuration.Instance.Config.WebDriver.CustomUserAgent ?? "Chrome/130.0.0.0"}");
      options.AddArguments(Configuration.Instance.Config.WebDriver.AdditionalArguments);

      ChromiumDriver driver = msEdge
        ? new EdgeDriver((EdgeDriverService)service, (EdgeOptions)options)
        : new ChromeDriver((ChromeDriverService)service, (ChromeOptions)options);
      driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', { get: () => undefined })");
      
      return driver;
    }
    
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