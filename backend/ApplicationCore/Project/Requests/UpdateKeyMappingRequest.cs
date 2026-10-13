using backend.ApplicationCore.Project.Requests;

namespace backend.ApplicationCore.Project.Requests
{
    public class UpdateKeyMappingsRequest : ProjectRequest
    {
        public List<ConfigKeyMapping> KeyMappings { get; set; }
    }
}



