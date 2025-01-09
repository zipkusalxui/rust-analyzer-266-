using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RustAnalyzer.Models
{
    public static class PluginHooksJson
    {
        private const string JsonContent = @"[
            {
                ""HookName"": ""OnPlayerTeleported"",
                ""HookParameters"": [""BasePlayer"", ""Vector3""],
                ""PluginName"": ""NTeleportation""
            }
        ]";

        public static List<PluginHookModel> GetHooks()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var hooks = JsonSerializer.Deserialize<List<PluginHookModel>>(JsonContent, options);
                return hooks ?? new List<PluginHookModel>();
            }
            catch
            {
                return new List<PluginHookModel>();
            }
        }
    }
} 