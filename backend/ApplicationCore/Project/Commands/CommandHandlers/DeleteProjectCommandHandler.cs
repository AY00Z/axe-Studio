using backend.Infrastructure.Repositories;
using MediatR;

namespace backend.ApplicationCore.Project.Commands
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
    {
        private readonly IProjectsConfigRepository _repository;
        private readonly ILogger<DeleteProjectCommandHandler> _logger;

        public DeleteProjectCommandHandler(
            IProjectsConfigRepository repository,
            ILogger<DeleteProjectCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var projectsConfig = await _repository.GetProjectsConfigAsync();
                var projectToRemove = projectsConfig.Projects.FirstOrDefault(p =>
                    p.Path.Equals(request.ProjectPath, StringComparison.OrdinalIgnoreCase));

                if (projectToRemove == null)
                {
                    throw new FileNotFoundException($"Project with path '{request.ProjectPath}' not found");
                }

                projectsConfig.Projects.Remove(projectToRemove);
                await _repository.SaveProjectsConfigAsync(projectsConfig);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project '{request.ProjectPath}'");
                throw;
            }
        }
    }
}