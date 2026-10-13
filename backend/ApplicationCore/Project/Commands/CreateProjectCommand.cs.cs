using backend.Domain.Entities.Project;
using MediatR;

namespace backend.ApplicationCore.Project.Commands
{
    public class CreateProjectCommand : IRequest<ProjectConfig>
    {
        public ProjectConfig Project { get; set; }
    }
}
