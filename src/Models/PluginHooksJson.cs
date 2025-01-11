using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RustAnalyzer.Models
{
    public static class PluginHooksJson
    {
        private const string JsonContent = @"[
            {
                ""HookName"": ""OnVanishReappear"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""Vanish""  
            },

            {
                ""HookName"": ""OnPlayerMuted"",
                ""HookParameters"": [""BasePlayer"",""BasePlayer"",""int"",""String""],
                ""PluginName"": ""IQChat""  
            },

            {
                ""HookName"": ""OnModeratorSendBadWords"",
                ""HookParameters"": [""BasePlayer"",""string""],
                ""PluginName"": ""IQChat""  
            },

            {
                ""HookName"": ""OnPlayerSendBadWords"",
                ""HookParameters"": [""BasePlayer"",""string""],
                ""PluginName"": ""IQChat""  
            },

            {
                ""HookName"": ""OnHomeRemoved"",
                ""HookParameters"": [""BasePlayer"",""Vector3"",""string""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnHomeAdded"",
                ""HookParameters"": [""BasePlayer"",""Vector3"",""string""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnHomeAccepted"",
                ""HookParameters"": [""BasePlayer"",""string"",""int""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnHomeAccepted"",
                ""HookParameters"": [""BasePlayer"",""string"",""int""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnPlayerTeleported"",
                ""HookParameters"": [""BasePlayer"",""Vector3"",""Vector3""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnTeleportAccepted"",
                ""HookParameters"": [""BasePlayer"",""BasePlayer"",""int""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnTeleportRejected"",
                ""HookParameters"": [""BasePlayer"",""BasePlayer""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""CanTeleport"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""canTeleport"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""IQTeleportation""  
            },

            {
                ""HookName"": ""OnTeleportRejected"",
                ""HookParameters"": [""BasePlayer"",""BasePlayer""],
                ""PluginName"": ""IQTeleportation""  
            },
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