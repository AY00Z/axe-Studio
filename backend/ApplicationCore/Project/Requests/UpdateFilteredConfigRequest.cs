using System.Text.Json;

public class UpdateFilteredConfigRequest
{
    public string ProjectName { get; set; }
    public string ConfigName { get; set; }
    public JsonElement UpdatedConfig { get; set; }
}