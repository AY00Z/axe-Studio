using System.Text.Json.Serialization;

namespace backend.Domain.Entities.Project
{
   public class ProjectsConfig
    {
        [JsonPropertyName("projects")]
        public List<ProjectConfig> Projects { get; set; } = new List<ProjectConfig>();  
    }     
}