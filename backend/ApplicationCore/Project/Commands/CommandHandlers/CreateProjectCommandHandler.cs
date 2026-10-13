
using backend.Domain.Entities.Project;
using backend.Infrastructure.Repositories;
using MediatR;

namespace backend.ApplicationCore.Project.Commands.CommandHandlers;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectConfig>
{
    private readonly IProjectsConfigRepository _repository;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    public CreateProjectCommandHandler(
        IProjectsConfigRepository repository,
        ILogger<CreateProjectCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProjectConfig> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var projectsConfig = await _repository.GetProjectsConfigAsync();

            // Check if project with same path already exists
            if (projectsConfig.Projects.Any(p => p.Path.Equals(request.Project.Path, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Project with path '{request.Project.Path}' already exists");
            }

            projectsConfig.Projects.Add(request.Project);
            await _repository.SaveProjectsConfigAsync(projectsConfig);

            return request.Project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            throw;
        }
    }
}


