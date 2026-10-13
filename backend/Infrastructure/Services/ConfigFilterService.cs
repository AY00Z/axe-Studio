using backend.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Infrastructure.Services
{
    public class ConfigFilterService : IConfigFilterService
    {
        private readonly ILogger<ConfigFilterService> _logger;

        public ConfigFilterService(ILogger<ConfigFilterService> logger)
        {
            _logger = logger;
        }

        public JsonElement FilterConfig(JsonElement config, List<string> visibleKeys)
        {
            var filteredObject = new JsonObject();

            foreach (var keyPattern in visibleKeys)
            {
                try
                {
                    if (keyPattern.Contains("*"))
                    {
                        ProcessWildcard(config, keyPattern, filteredObject);
                    }
                    else
                    {
                        var value = GetNestedValue(config, keyPattern);
                        if (value != null)
                        {
                            SetNestedValue(filteredObject, keyPattern, value.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to process key pattern: {keyPattern}");
                }
            }

            return JsonSerializer.SerializeToElement(filteredObject);
        }

        private void ProcessWildcard(JsonElement config, string wildcardPattern, JsonObject result)
        {
            var segments = wildcardPattern.Split('.');
            var queue = new Queue<(JsonElement element, string[] remainingSegments, JsonObject parent)>();
            queue.Enqueue((config, segments, result));

            while (queue.Count > 0)
            {
                var (element, remainingSegments, parent) = queue.Dequeue();

                if (remainingSegments.Length == 0) continue;

                var currentSegment = remainingSegments[0];
                var isLastSegment = remainingSegments.Length == 1;

                if (currentSegment == "*")
                {
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in element.EnumerateObject())
                        {
                            if (isLastSegment)
                            {
                                parent[property.Name] = JsonSerializer.SerializeToNode(property.Value);
                            }
                            else
                            {
                                var newParent = new JsonObject();
                                parent[property.Name] = newParent;
                                queue.Enqueue((property.Value, remainingSegments[1..], newParent));
                            }
                        }
                    }
                }
                else if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(currentSegment, out var nextElement))
                {
                    if (isLastSegment)
                    {
                        parent[currentSegment] = JsonSerializer.SerializeToNode(nextElement);
                    }
                    else
                    {
                        var newParent = new JsonObject();
                        parent[currentSegment] = newParent;
                        queue.Enqueue((nextElement, remainingSegments[1..], newParent));
                    }
                }
            }
        }

        private JsonElement? GetNestedValue(JsonElement element, string path)
        {
            var current = element;
            foreach (var segment in path.Split('.'))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                {
                    return null;
                }
            }
            return current;
        }

        private void SetNestedValue(JsonObject obj, string path, JsonElement value)
        {
            var segments = path.Split('.');
            var current = obj;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (!current.TryGetPropertyValue(segments[i], out var nextNode))
                {
                    nextNode = new JsonObject();
                    current[segments[i]] = nextNode;
                }
                current = nextNode as JsonObject ?? new JsonObject();
            }

            current[segments.Last()] = JsonSerializer.SerializeToNode(value);
        }
    }
}