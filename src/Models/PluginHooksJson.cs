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

            {
                ""HookName"": ""OnPlayerTeleported"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""Vector3""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportRejected"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportRequestCompleted"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportAccepted"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer"", ""int""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeAccepted"",
                ""HookParameters"": [""BasePlayer"", ""string"", ""int""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeRemoved"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeAdded"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportInterrupted"",
                ""HookParameters"": [""BasePlayer"", ""string"", ""ulong"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""SetPermission"",
                ""HookParameters"": [""ulong"", ""string"", ""DateTime""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""SetPermission"",
                ""HookParameters"": [""ulong"", ""string"", ""string""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""SetGroup"",
                ""HookParameters"": [""ulong"", ""string"", ""DateTime""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""SetGroup"",
                ""HookParameters"": [""ulong"", ""string"", ""string""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""RevokePermission"",
                ""HookParameters"": [""ulong"", ""string""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""RevokeGroup"",
                ""HookParameters"": [""ulong"", ""string"", ""DateTime""],
                ""PluginName"": ""IQPermissions""
            },
            {
                ""HookName"": ""OnKickPlayer"",
                ""HookParameters"": [""string"", ""string"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnBannedPlayerIP"",
                ""HookParameters"": [""string"", ""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnBannedPlayerID"",
                ""HookParameters"": [""ulong"", ""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnUpdateTimeBannedID"",
                ""HookParameters"": [""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnUpdateTimeBannedIP"",
                ""HookParameters"": [""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnChangePermanentBannedID"",
                ""HookParameters"": [""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnChangePermanentBannedIP"",
                ""HookParameters"": [""string"", ""double"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnUnbannedID"",
                ""HookParameters"": [""string"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
            },
            {
                ""HookName"": ""OnUnbannedIP"",
                ""HookParameters"": [""string"", ""BasePlayer""],
                ""PluginName"": ""IQBanSystem""
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