using backend.ApplicationCore.Project.Requests;
using backend.Domain.Entities.Project;
using backend.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.WebApi.Controllers

{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsConfigController : ControllerBase
    {
        private readonly IProjectsConfigService _projectsConfigService;
        private readonly IConfigUpdateService _configUpdateService;
        private readonly ILogger<ProjectsConfigController> _logger;

        public ProjectsConfigController(
            IProjectsConfigService projectsConfigService,
            IConfigUpdateService configUpdateService,
            ILogger<ProjectsConfigController> logger)
        {
            _projectsConfigService = projectsConfigService;
            _configUpdateService = configUpdateService;
            _logger = logger;
        }

        // GET: api/projects 
        [HttpGet]
        public IActionResult GetAllProjects()
        {
            try
            {
                var config = _projectsConfigService.GetProjectsConfig();
                return Ok(config.Projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all projects");
                return Problem($"Error loading projects: {ex.Message}");
            }
        }

        [HttpPost("[action]")]
        public IActionResult GetProjectConfigsList([FromBody] ProjectRequest request)
        {
            try
            {
                var configFiles = _projectsConfigService.GetProjectConfigsList(request.ProjectName);
                return Ok(configFiles);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Project not found: {request.ProjectName}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"No key mappings found for project: {request.ProjectName}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing configs for {request.ProjectName}");
                return Problem($"Error listing configs: {ex.Message}");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetSingleProjectConfig([FromBody] ConfigRequest request)
        {
            try
            {
                var config = await _projectsConfigService.GetSingleProjectConfig(request.ProjectName, request.ConfigName);
                return Ok(config);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Config not found: {request.ProjectName}/{request.ConfigName}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading config {request.ConfigName} for {request.ProjectName}");
                return Problem($"Error loading config: {ex.Message}");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetFilteredProjectConfig([FromBody] ConfigRequest request)
        {
            try
            {
                var config = await _projectsConfigService.GetFilteredProjectConfig(request.ProjectName, request.ConfigName);
                return Ok(config);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Config not found: {request.ProjectName}/{request.ConfigName}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading filtered config {request.ConfigName} for {request.ProjectName}");
                return Problem($"Error loading filtered config: {ex.Message}");
            }
        }

        [HttpPut("[action]")]
        public IActionResult UpdateKeyMappings([FromBody] UpdateKeyMappingsRequest request)
        {
            try
            {
                _projectsConfigService.UpdateKeyMappings(request.ProjectName, request.KeyMappings);
                return Ok();
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Project not found: {request.ProjectName}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating key mappings for {request.ProjectName}");
                return Problem($"Error updating key mappings: {ex.Message}");
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetProjectKeyMappings([FromQuery] string acronym)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(acronym))
                {
                    return BadRequest("Project acronym is required");
                }

                var project = _projectsConfigService.GetProjectByAcronym(acronym);

                if (project == null)
                {
                    return NotFound($"Project with acronym '{acronym}' not found");
                }

                return Ok(project.KeyMappings ?? new List<ConfigKeyMapping>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting key mappings for project {acronym}");
                return Problem($"Error retrieving key mappings: {ex.Message}");
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateFilteredProjectConfig([FromBody] UpdateFilteredConfigRequest request)
        {
            try
            {
                // Get visible keys for this config
                var project = _projectsConfigService.GetProjectByPath(request.ProjectName);
                var configMapping = project?.KeyMappings?.FirstOrDefault(m =>
                    m.ConfigFileName.Equals(request.ConfigName, StringComparison.OrdinalIgnoreCase));

                if (configMapping?.VisibleKeys == null)
                {
                    return BadRequest("No visible keys defined for this configuration");
                }

                var filteredResult = await _configUpdateService.UpdateFilteredConfig(
                    request.ProjectName,
                    request.ConfigName,
                    request.UpdatedConfig,
                    configMapping.VisibleKeys);

                return Ok(filteredResult);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Config not found: {request.ProjectName}/{request.ConfigName}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating filtered config {request.ConfigName} for {request.ProjectName}");
                return Problem($"Error updating config: {ex.Message}");
            }
        }
    }
}