
using backend.Domain.Entities.Project;
using backend.Infrastructure.Repositories;
using MediatR;

namespace backend.ApplicationCore.Project.Commands
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectConfig>
    {
        private readonly IProjectsConfigRepository _repository;
        private readonly ILogger<UpdateProjectCommandHandler> _logger;

        public UpdateProjectCommandHandler(
            IProjectsConfigRepository repository,
            ILogger<UpdateProjectCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProjectConfig> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            try 
            {
                var projectsConfig = await _repository.GetProjectsConfigAsync();
                var existingProject = projectsConfig.Projects.FirstOrDefault(p =>
                    p.Path.Equals(request.ProjectPath, StringComparison.OrdinalIgnoreCase));

                if (existingProject == null)
                {
                    throw new FileNotFoundException($"Project with path '{request.ProjectPath}' not found");
                }

                // Update properties
                existingProject.Acronym = request.UpdatedProject.Acronym;
                existingProject.Title = request.UpdatedProject.Title;
                existingProject.Description = request.UpdatedProject.Description;
                existingProject.KeyMappings = request.UpdatedProject.KeyMappings;

                await _repository.SaveProjectsConfigAsync(projectsConfig);

                return existingProject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project '{request.ProjectPath}'");
                throw;
            }
        }
    }
}
