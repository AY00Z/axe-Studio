using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using backend.Domain.Interfaces.Services;
using backend.Domain.Entities.Project;

namespace backend.Infrastructure.Services
{
    public class ProjectsConfigService : IProjectsConfigService
    {
        private const string ProjectsConfigPath = "WebApi/Data/project-config.json";
        private const string ConfigBasePath = "WebApi/Data/";
        private readonly ILogger<ProjectsConfigService> _logger;
        private readonly IConfigFilterService _configFilterService;

        public ProjectsConfigService(
            ILogger<ProjectsConfigService> logger,
            IConfigFilterService configFilterService)
        {
            _logger = logger;
            _configFilterService = configFilterService;
        }

        public ProjectsConfig GetProjectsConfig()
        {
            if (!File.Exists(ProjectsConfigPath))
            {
                return new ProjectsConfig { Projects = new List<ProjectConfig>() };
            }
            var jsonContent = File.ReadAllText(ProjectsConfigPath);
            return JsonSerializer.Deserialize<ProjectsConfig>(jsonContent) ?? new ProjectsConfig();
        }

        public ProjectConfig? GetProjectByPath(string path)
        {
            var config = GetProjectsConfig();
            return config.Projects.FirstOrDefault(p =>
                p.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetProjectConfigsList(string projectName)
        {
            var project = GetProjectByPath(projectName);
            if (project == null) throw new FileNotFoundException($"Project '{projectName}' not found");

            return project.KeyMappings?
                .Select(m => m.ConfigFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();
        }

        public async Task<JsonElement> GetSingleProjectConfig(string projectName, string configName)
        {
            var project = GetProjectByPath(projectName);
            if (project == null) throw new FileNotFoundException($"Project '{projectName}' not found");

            var configPath = Path.Combine(ConfigBasePath, configName);
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Config file '{configName}' not found in data directory");
            }

            var jsonContent = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<JsonElement>(jsonContent);
        }

        public async Task UpdateSingleProjectConfig(string projectName, string configName, JsonElement updatedConfig)
        {
            var configPath = Path.Combine(ConfigBasePath, configName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(updatedConfig, options));
        }

        public void UpdateAllProjectsConfig(ProjectsConfig updatedConfig)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonContent = JsonSerializer.Serialize(updatedConfig, options);
            File.WriteAllText(ProjectsConfigPath, jsonContent);
        }


        public ProjectConfig GetProjectByAcronym(string acronym)
        {
            var allProjects = GetProjectsConfig().Projects;
            return allProjects.FirstOrDefault(p =>
                p.Acronym.Equals(acronym, StringComparison.OrdinalIgnoreCase));
        }


        public async Task<JsonElement> GetFilteredProjectConfig(string projectName, string configName)
        {
            var project = GetProjectByPath(projectName);
            if (project == null) throw new FileNotFoundException($"Project '{projectName}' not found");

            var configMapping = project.KeyMappings?.FirstOrDefault(m =>
                m.ConfigFileName.Equals(configName, StringComparison.OrdinalIgnoreCase));

            if (configMapping?.VisibleKeys == null || !configMapping.VisibleKeys.Any())
            {
                return await GetSingleProjectConfig(projectName, configName);
            }

            var config = await GetSingleProjectConfig(projectName, configName);
            return _configFilterService.FilterConfig(config, configMapping.VisibleKeys);
        }

        public void UpdateKeyMappings(string projectName, List<ConfigKeyMapping> keyMappings)
        {
            var config = GetProjectsConfig();
            var project = config.Projects.FirstOrDefault(p =>
                p.Path.Equals(projectName, StringComparison.OrdinalIgnoreCase));

            if (project == null) throw new FileNotFoundException($"Project '{projectName}' not found");

            project.KeyMappings = keyMappings;
            UpdateAllProjectsConfig(config);
        }
    }


}