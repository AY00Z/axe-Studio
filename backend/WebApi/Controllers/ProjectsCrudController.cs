using backend.ApplicationCore.Project.Commands;
using backend.ApplicationCore.Project.Queries;
using backend.Domain.Entities.Project;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsCrudController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsCrudController(IMediator mediator)
        {
            _mediator = mediator;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectConfig>>> GetAllProjects()
        {
            var projectsConfig = await _mediator.Send(new GetAllProjectsQuery());
            return Ok(projectsConfig.Projects);
        }

       
        [HttpGet("{path}")]
        public async Task<ActionResult<ProjectConfig>> GetProjectByPath(string path)
        {
            var project = await _mediator.Send(new GetProjectByPathQuery { ProjectPath = path });
            return Ok(project);
        }

      
        [HttpPost]
        public async Task<ActionResult<ProjectConfig>> CreateProject([FromBody] ProjectConfig project)
        {
            var createdProject = await _mediator.Send(new CreateProjectCommand { Project = project });
            return CreatedAtAction(nameof(GetProjectByPath), new { path = createdProject.Path }, createdProject);
        }


        [HttpPut("{path}")]
        public async Task<ActionResult<ProjectConfig>> UpdateProject(string path, [FromBody] ProjectConfig updatedProject)
        {
            var result = await _mediator.Send(new UpdateProjectCommand
            {
                ProjectPath = path,
                UpdatedProject = updatedProject
            });
            return Ok(result);                         
        }

       
        [HttpDelete("{path}")]
        public async Task<IActionResult> DeleteProject(string path)
        {
            await _mediator.Send(new DeleteProjectCommand { ProjectPath = path });
            return NoContent();
        }
    }
}