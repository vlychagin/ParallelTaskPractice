using Newtonsoft.Json;

namespace ParallelTaskPractice.Models;

// представление валюты в файле JSON от ЦРБ
// сгенерировано сервисом https://app.quicktype.io/#l=cs&r=json2csharp
public class Valute
{
    [JsonProperty("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("NumCode")]
    public string NumCode { get; set; } = string.Empty;

    [JsonProperty("CharCode")]
    public string CharCode { get; set; } = string.Empty;

    [JsonProperty("Nominal")]
    public long Nominal { get; set; }

    [JsonProperty("Name")] public string Name { get; set; } = string.Empty;

    [JsonProperty("Value")]
    public double Value { get; set; }

    [JsonProperty("Previous")]
    public double Previous { get; set; }
} // class Valute 
