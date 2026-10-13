using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using backend.ApplicationCore.Project.Requests;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UrlsController> _logger;

        private const string AppUrlsJsonPath = "C:\\Users\\aya20\\source\\repos\\backend\\backend\\WebApi\\Data\\urls.json"; 
        private const string ProjectsUrlsJsonPath = "C:\\Users\\aya20\\source\\repos\\backend\\backend\\WebApi\\Data\\ProjectsUrls.json"; 

        public UrlsController(IWebHostEnvironment env, ILogger<UrlsController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUrls()
        {
            try
            {
                var fullPath = Path.Combine(_env.ContentRootPath, AppUrlsJsonPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogError("AppUrls JSON file not found at: {Path}", fullPath);
                    return NotFound($"Configuration file not found at: {AppUrlsJsonPath}");
                }

                var jsonContent = System.IO.File.ReadAllText(fullPath);
                var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (!jsonData.TryGetValue("AppUrls", out var appUrls))
                {
                    _logger.LogError("'AppUrls' section not found in JSON file");
                    return BadRequest("Invalid configuration format - missing 'AppUrls' section");
                }

                return Ok(appUrls);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Invalid JSON format in configuration file");
                return BadRequest("Invalid JSON format in configuration file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading configuration");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("[action]")]
        public IActionResult AddProjectUrl([FromBody] ProjectUrlRequest request)
        {
            try
            {
                var fullPath = Path.Combine(_env.ContentRootPath, ProjectsUrlsJsonPath);

                // Read existing data
                var jsonData = new Dictionary<string, Dictionary<string, string>>();
                if (System.IO.File.Exists(fullPath))
                {
                    var existingContent = System.IO.File.ReadAllText(fullPath);
                    jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(existingContent)
                              ?? new Dictionary<string, Dictionary<string, string>>();
                }

                // Initialize ProjectsUrls if it doesn't exist
                if (!jsonData.ContainsKey("ProjectsUrls"))
                {
                    jsonData["ProjectsUrls"] = new Dictionary<string, string>();
                }

                // Find the next available app number
                int appNumber = 1;
                while (jsonData["ProjectsUrls"].ContainsKey($"app{appNumber}"))
                {
                    appNumber++;
                }

                // Add the new URL
                jsonData["ProjectsUrls"][$"app{appNumber}"] = request.Url;

                // Save back to file
                var updatedContent = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(fullPath, updatedContent);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project URL");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetProjectUrls()
        {
            try
            {
                var fullPath = Path.Combine(_env.ContentRootPath, ProjectsUrlsJsonPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    return Ok(new Dictionary<string, string>());
                }

                var jsonContent = System.IO.File.ReadAllText(fullPath);
                var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (!jsonData.TryGetValue("ProjectsUrls", out var projectUrls))
                {
                    return Ok(new Dictionary<string, string>());
                }

                // Return only the URL values without the app keys
                var urlValues = projectUrls.Values.ToList();
                return Ok(urlValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project URLs");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpDelete("[action]")]
        public IActionResult DeleteProjectByUrl([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return BadRequest("URL parameter is required");
                }

                var fullPath = Path.Combine(_env.ContentRootPath, ProjectsUrlsJsonPath);

                // Read the existing content
                var jsonContent = System.IO.File.ReadAllText(fullPath);
                var projectsData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (projectsData == null || !projectsData.ContainsKey("ProjectsUrls"))
                {
                    return NotFound("No projects found");
                }

                var projectsUrls = projectsData["ProjectsUrls"];
                var projectToRemove = projectsUrls.FirstOrDefault(x => x.Value.Equals(url, StringComparison.OrdinalIgnoreCase)).Key;

                if (projectToRemove != null)
                {
                    // Remove the project
                    projectsUrls.Remove(projectToRemove);

                    // Write the updated content back to the file
                    var updatedContent = JsonSerializer.Serialize(projectsData, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(fullPath, updatedContent);

                    return Ok();
                }

                return NotFound($"Project with URL '{url}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project with URL {url}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpDelete("[action]")]
        public IActionResult ClearProjectUrls()
        {
            try
            {
                var fullPath = Path.Combine(_env.ContentRootPath, ProjectsUrlsJsonPath);    

                // Create or overwrite the file with empty ProjectsUrls object
                var emptyData = new Dictionary<string, Dictionary<string, string>>
                {
                    ["ProjectsUrls"] = new Dictionary<string, string>()
                };

                var updatedContent = JsonSerializer.Serialize(emptyData, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(fullPath, updatedContent);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing project URLs");
                return StatusCode(500, "Internal server error");
            }
        }
    
    }
}