using backend.Domain.Entities.Project;
using backend.Infrastructure.Repositories;
using MediatR;

namespace backend.ApplicationCore.Project.Queries
{
    public class GetProjectByPathQueryHandler : IRequestHandler<GetProjectByPathQuery, ProjectConfig>
    {
        private readonly IProjectsConfigRepository _repository;
        private readonly ILogger<GetProjectByPathQueryHandler> _logger;

        public GetProjectByPathQueryHandler(
            IProjectsConfigRepository repository,
            ILogger<GetProjectByPathQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProjectConfig> Handle(
            GetProjectByPathQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var config = await _repository.GetProjectsConfigAsync();
                var project = config.Projects.FirstOrDefault(p =>
                    p.Path.Equals(request.ProjectPath, StringComparison.OrdinalIgnoreCase));

                return project ?? throw new FileNotFoundException(
                    $"Project with path '{request.ProjectPath}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching project {request.ProjectPath}");
                throw;
            }
        }
    }
}