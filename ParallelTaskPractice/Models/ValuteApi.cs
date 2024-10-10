using Newtonsoft.Json;

namespace ParallelTaskPractice.Models;

// сгенерировано сервисом https://app.quicktype.io/#l=cs&r=json2csharp
public class ValuteApi
{
    [JsonProperty("Date")]
    public DateTime Date { get; set; }

    [JsonProperty("PreviousDate")]
    public DateTime PreviousDate { get; set; }

    [JsonProperty("PreviousURL")]
    public string PreviousUrl { get; set; } = string.Empty;

    [JsonProperty("Timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("Valute")] public Dictionary<string, Valute> Valutes { get; set; } = new();

    public static ValuteApi FromJson(string json) =>
        JsonConvert.DeserializeObject<ValuteApi>(json)!;

} // class ValuteApi
