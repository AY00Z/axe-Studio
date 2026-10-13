
using backend.Domain.Entities.Project;
using backend.Infrastructure.Repositories;
using MediatR;

namespace backend.ApplicationCore.Project.Queries
{
    public class GetAllProjectsQueryHandler
        : IRequestHandler<GetAllProjectsQuery, ProjectsConfig>
    {
        private readonly IProjectsConfigRepository _repository;
        private readonly ILogger<GetAllProjectsQueryHandler> _logger;

        public GetAllProjectsQueryHandler(
            IProjectsConfigRepository repository,
            ILogger<GetAllProjectsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProjectsConfig> Handle(
            GetAllProjectsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _repository.GetProjectsConfigAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all projects");
                throw; // Re-throw for global error handling
            }
        }
    }
}