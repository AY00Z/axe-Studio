using System.Text.Json.Serialization;

public class ConfigKeyMapping
{
    [JsonPropertyName("configFileName")]
    public string ConfigFileName { get; set; } = string.Empty;

    [JsonPropertyName("visibleKeys")]
    public List<string> VisibleKeys { get; set; } = new List<string>();
}