namespace Trawler.Common.Utility {
  public static class TaskUtil {
    public static async Task WaitForValueAsync<T>(Func<T?> valueGetter, Func<T?, bool> predicate, ulong pollInterval = 100UL) {
      while(!predicate(valueGetter())) await Task.Delay(TimeSpan.FromMilliseconds(pollInterval));
    }
  
    public static async Task WaitForValueAsync<T>(Func<T?> valueGetter, ulong pollInterval = 100UL) {
      await WaitForValueAsync(valueGetter, (value) => value != null, pollInterval);
    }
  
    public static async Task WaitForValueAsync<T>(Func<T?> valueGetter) {
      await WaitForValueAsync(valueGetter, (ulong)TimeSpan.MaxValue.Milliseconds);
    }
  }
}