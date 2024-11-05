using Quartz;
using Quartz.Impl;
using Trawler.Common.Utility.Logging;
using Trawler.Config;
using Trawler.Database;
using Trawler.Scheduler.Job;

namespace Trawler {
  public static class ProgramEntry {
    private static readonly LoggerBase logger = LoggerFactory.CreateLogger(subject: nameof(ProgramEntry));
    private const string WaitHandleName = "04D477C6-6CB7-4375-AAA8-BC6C490F5889";

    public static async Task Main(string[] args) {
      logger.Log("Program started.");
      
      await Initialize();
      await StartScheduler();
      await KeepRunning();
    }

    private static async Task Initialize() {
      logger.Log("Start initialization...");
      
      // *** Configuration *** //
      try {
        await Configuration.Instance.Load();
      } catch {
        logger.LogError("Configuration loading failed.");
        throw;
      }
      
      // *** Database connection test *** //
      if(!await DatabaseContext.TestConnection()) {
        logger.LogError("Database connection should be configured and available to use this program.");
        throw new ApplicationException("Database connection should be configured and available to use this program.");
      }

      logger.Log("Initialization completed.");
    }

    private static async Task StartScheduler() {
      logger.Log("Starting scheduler...");
      
      TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(Configuration.Instance.Config.Scheduler.DefaultTimezone);
      StdSchedulerFactory schedulerFactory = new StdSchedulerFactory();
      IScheduler scheduler = await schedulerFactory.GetScheduler();
      await scheduler.Start();
      
      ITrigger dailyTrigger = TriggerBuilder.Create()
        .WithIdentity("DailyTrigger", "Crawler")
        .StartNow()
        .WithCronSchedule("0 50 23 * * ?", x => x.InTimeZone(timezone))
        .Build();
      
      ITrigger immediateTrigger = TriggerBuilder.Create()
        .WithIdentity("ImmediateTrigger", "Crawler")
        .StartNow()
        .Build();

      // Twitter account data crawl job - daily
      {
        IJobDetail accountDataJobDetail = JobBuilder.Create<TwitterAccountDataCrawlJob>()
          .WithIdentity("TwitterAccountData", "Crawler")
          .Build();
        await scheduler.ScheduleJob(accountDataJobDetail, dailyTrigger);

        if(await scheduler.CheckExists(accountDataJobDetail.Key)) {
          logger.Log("Daily Twitter account data crawl job scheduled.");

          if(dailyTrigger.GetNextFireTimeUtc() is { UtcDateTime: { } nextFireTimeUtcDateTime }) {
            logger.Log($"  \u2514 Next run at UTC: {nextFireTimeUtcDateTime}");
            logger.Log($"  \u2514 Next run at {timezone.Id}: {TimeZoneInfo.ConvertTimeFromUtc(nextFireTimeUtcDateTime, timezone)}");
          }
        }
      }

      // Twitter single post crawl job - ???
      {
        IJobDetail singlePostJobDetail = JobBuilder.Create<TwitterSinglePostCrawlJob>()
          .WithIdentity("TwitterSinglePost", "Crawler")
          .Build();
        await scheduler.ScheduleJob(singlePostJobDetail, immediateTrigger);
        // TODO
        
        if(await scheduler.CheckExists(singlePostJobDetail.Key)) {
          logger.Log("Twitter single post crawl job scheduled.");
        }
      }
    }

    private static async Task KeepRunning() {
      bool signaled = false;
      bool createdNew = false;
      
      // Setup EventWaitHandle
      EventWaitHandle waitHandle;
      try {
        waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, WaitHandleName,
          out createdNew);
      } catch(PlatformNotSupportedException) {
        logger.LogWarning("Platform does not support named EventWaitHandle. Fallback to unnamed EventWaitHandle.");
        waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null, out createdNew);
      } catch(Exception e) {
        logger.LogError("Failed to create EventWaitHandle. Program will exit.", e);
        return;
      }

      // Check if another instance is already running
      if(!createdNew) {
        logger.LogError("Another instance of this program is already running. This instance will exit.");
        waitHandle.Set();
        return;
      }

      // Wait forever until exit signal is received
      logger.Log("Program is now keeping running.");
      await using var timer = new Timer(_ => { }, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
      do {
        signaled = waitHandle.WaitOne(TimeSpan.FromSeconds(10));
      } while(!signaled);
      
      // Program will exit after signal
      logger.Log("Got signal to exit. Program will exit.");
    }
  }
}