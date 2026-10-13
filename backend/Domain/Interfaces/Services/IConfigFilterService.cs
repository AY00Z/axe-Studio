using System.Text.Json;

namespace backend.Domain.Interfaces.Services
{
    public interface IConfigFilterService
    {
     
        JsonElement FilterConfig(JsonElement config, List<string> visibleKeys);
    }
}



