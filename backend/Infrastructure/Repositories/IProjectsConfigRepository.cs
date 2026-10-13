using backend.Domain.Entities.Project;

namespace backend.Infrastructure.Repositories
{
    public interface IProjectsConfigRepository
    {
        Task<ProjectsConfig> GetProjectsConfigAsync();
        Task SaveProjectsConfigAsync(ProjectsConfig config);
    }
}
