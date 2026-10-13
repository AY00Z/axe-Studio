using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Domain.Interfaces.Services;

namespace backend.Infrastructure.Services
{
    public class ConfigUpdateService : IConfigUpdateService
    {
        private readonly ILogger<ConfigUpdateService> _logger;
        private readonly IProjectsConfigService _projectsConfigService;
        private readonly IConfigFilterService _configFilterService;

        public ConfigUpdateService(
              ILogger<ConfigUpdateService> logger,
            IProjectsConfigService projectsConfigService,
            IConfigFilterService configFilterService)
        {
            _logger = logger;
            _projectsConfigService = projectsConfigService;
            _configFilterService = configFilterService;
        }

        public async Task<JsonElement> UpdateFilteredConfig(
            string projectName,
            string configName,
            JsonElement updatedFilteredConfig,
            List<string> visibleKeys)
        {
            // 1. Get the complete current config
            var completeConfig = await _projectsConfigService.GetSingleProjectConfig(projectName, configName);

            // 2. Merge the filtered updates into the complete config
            var mergedConfig = MergeConfigUpdates(completeConfig, updatedFilteredConfig, visibleKeys);

            // 3. Save the complete updated config
            await _projectsConfigService.UpdateSingleProjectConfig(projectName, configName, mergedConfig);

            // 4. Return the filtered view of the updated config
            return _configFilterService.FilterConfig(mergedConfig, visibleKeys);
        }

        public JsonElement MergeConfigUpdates(
            JsonElement completeConfig,
            JsonElement filteredUpdates,
            List<string> visibleKeys)
        {
            try
            {
                var completeNode = JsonSerializer.SerializeToNode(completeConfig)!.DeepClone();
                var updatesNode = JsonSerializer.SerializeToNode(filteredUpdates)!;

                foreach (var keyPattern in visibleKeys)
                {
                    if (keyPattern.Contains("*"))
                    {
                        MergeWildcardUpdates(completeNode, updatesNode, keyPattern);
                    }
                    else
                    {
                        MergeSpecificKey(completeNode, updatesNode, keyPattern);
                    }
                }

                return JsonSerializer.SerializeToElement(completeNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging config updates");
                throw;
            }
        }

        private void MergeSpecificKey(JsonNode completeNode, JsonNode updatesNode, string path)
        {
            var segments = path.Split('.');
            var currentUpdateNode = updatesNode;
            var currentCompleteNode = completeNode;

            // Navigate to the value in updates
            foreach (var segment in segments)
            {
                if (currentUpdateNode?[segment] == null) return;
                currentUpdateNode = currentUpdateNode[segment];
            }

            // Navigate to the location in complete config and update
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (currentCompleteNode?[segments[i]] == null)
                {
                    currentCompleteNode![segments[i]] = new JsonObject();
                }
                currentCompleteNode = currentCompleteNode[segments[i]];
            }

            if (currentCompleteNode != null && currentUpdateNode != null)
            {
                currentCompleteNode[segments.Last()] = currentUpdateNode.DeepClone();
            }
        }

        private void MergeWildcardUpdates(JsonNode completeNode, JsonNode updatesNode, string wildcardPattern)
        {
            var segments = wildcardPattern.Split('.');
            var updateQueue = new Queue<(JsonNode updateParent, string[] remainingSegments)>();
            var completeQueue = new Queue<(JsonNode completeParent, string[] remainingSegments)>();

            updateQueue.Enqueue((updatesNode, segments));
            completeQueue.Enqueue((completeNode, segments));

            while (updateQueue.Count > 0 && completeQueue.Count > 0)
            {
                var (updateParent, remainingUpdateSegments) = updateQueue.Dequeue();
                var (completeParent, remainingCompleteSegments) = completeQueue.Dequeue();

                if (remainingUpdateSegments.Length == 0 || remainingCompleteSegments.Length == 0) continue;

                var currentSegment = remainingUpdateSegments[0];
                var isLastSegment = remainingUpdateSegments.Length == 1;

                if (currentSegment == "*")
                {
                    if (updateParent is JsonObject updateObj && completeParent is JsonObject completeObj)
                    {
                        foreach (var property in updateObj)
                        {
                            if (completeObj.ContainsKey(property.Key))
                            {
                                if (isLastSegment)
                                {
                                    completeObj[property.Key] = property.Value?.DeepClone();
                                }
                                else
                                {
                                    updateQueue.Enqueue((property.Value, remainingUpdateSegments[1..]));
                                    completeQueue.Enqueue((completeObj[property.Key], remainingCompleteSegments[1..]));
                                }
                            }
                        }
                    }
                }
                else if (updateParent[currentSegment] != null && completeParent[currentSegment] != null)
                {
                    if (isLastSegment)
                    {
                        completeParent[currentSegment] = updateParent[currentSegment]?.DeepClone();
                    }
                    else
                    {
                        updateQueue.Enqueue((updateParent[currentSegment], remainingUpdateSegments[1..]));
                        completeQueue.Enqueue((completeParent[currentSegment], remainingCompleteSegments[1..]));
                    }
                }
            }
        }
    }
}