using backend.Domain.Entities.Project;
using MediatR;

namespace backend.ApplicationCore.Project.Commands
{
    public class UpdateProjectCommand : IRequest<ProjectConfig>
    {
        public string ProjectPath { get; set; }
        public ProjectConfig UpdatedProject { get; set; }
    }
}
