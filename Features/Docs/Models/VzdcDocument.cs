using System.Text.Json.Serialization;

namespace ZdcReference.Features.Docs.Models;

public record VzdcDocument
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}
