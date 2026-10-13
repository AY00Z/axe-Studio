using MediatR;

namespace backend.ApplicationCore.Project.Commands
{
    public class DeleteProjectCommand : IRequest<bool>
    {
        public string ProjectPath { get; set; }
    }
}
