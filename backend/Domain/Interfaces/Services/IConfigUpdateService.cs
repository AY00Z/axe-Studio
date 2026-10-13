using System.Text.Json;

namespace backend.Domain.Interfaces.Services
{
    public interface IConfigUpdateService
    {
        Task<JsonElement> UpdateFilteredConfig(
            string projectName,
            string configName,
            JsonElement updatedFilteredConfig,
            List<string> visibleKeys); // Added visibleKeys parameter
    }
}
