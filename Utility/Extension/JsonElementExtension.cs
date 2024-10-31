using System.Text.Json;

namespace Trawler.Utility.Extension {
  public static class JsonElementExtension {
    public static string SafeGetString(this JsonElement json) {
      if(json.GetString() is { } value) {
        return value;
      }
      
      throw new JsonException("Can't get string value from JSON element.");
    }
  }
}