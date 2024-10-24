using OpenQA.Selenium;

namespace Trawler.Crawler {
  public abstract class CrawlerBase<T>(IWebDriver driver) {
    public abstract Task NavigateToTargetAsync();
    public abstract Task<T> DoCrawlAsync();
  }
}