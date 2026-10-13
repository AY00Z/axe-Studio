
using backend.Domain.Entities.Project;
using MediatR;

namespace backend.ApplicationCore.Project.Queries
{
    public class GetProjectByPathQuery : IRequest<ProjectConfig>
    {
        public string ProjectPath { get; set; } 
    }
}