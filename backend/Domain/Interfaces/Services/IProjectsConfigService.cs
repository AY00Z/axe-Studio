using backend.Domain.Entities.Project;
using System.Text.Json;

namespace backend.Domain.Interfaces.Services
{
    public interface IProjectsConfigService
    {
        ProjectsConfig GetProjectsConfig();
        ProjectConfig? GetProjectByPath(string path);
        List<string> GetProjectConfigsList(string projectName);
        Task<JsonElement> GetSingleProjectConfig(string projectName, string configName);
        Task UpdateSingleProjectConfig(string projectName, string configName, JsonElement updatedConfig);
        void UpdateAllProjectsConfig(ProjectsConfig updatedConfig);
        Task<JsonElement> GetFilteredProjectConfig(string projectName, string configName);
        void UpdateKeyMappings(string projectName, List<ConfigKeyMapping> keyMappings);
        ProjectConfig GetProjectByAcronym(string acronym);
    }
}
