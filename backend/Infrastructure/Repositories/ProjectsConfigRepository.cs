using backend.Domain.Entities.Project;
using System.Text.Json;

namespace backend.Infrastructure.Repositories
{
    public class ProjectsConfigRepository : IProjectsConfigRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProjectsConfigRepository> _logger;
        private const string ConfigFileName = "C:\\Users\\aya20\\source\\repos\\backend\\backend\\WebApi\\Data\\project-config.json";

        public ProjectsConfigRepository(
            IWebHostEnvironment env,
            ILogger<ProjectsConfigRepository> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<ProjectsConfig> GetProjectsConfigAsync()
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Data", ConfigFileName);
                if (!File.Exists(filePath))
                {
                    return new ProjectsConfig { Projects = new List<ProjectConfig>() };
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<ProjectsConfig>(json) ?? new ProjectsConfig { Projects = new List<ProjectConfig>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading projects config");
                throw;
            }
        }

        public async Task SaveProjectsConfigAsync(ProjectsConfig config)
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Data", ConfigFileName);
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(config, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving projects config");
                throw;
            }
        }
    }
}