using System.Text.Json.Serialization;

namespace backend.Domain.Entities.Project
{
    public class ProjectConfig
    {
        [JsonPropertyName("acronym")]
        public string Acronym { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;


        [JsonPropertyName("keyMappings")]
        public List<ConfigKeyMapping> KeyMappings { get; set; } = new List<ConfigKeyMapping>();
    }

}
