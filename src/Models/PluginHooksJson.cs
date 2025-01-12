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
            },
            {
                ""HookName"": ""OnTeleportInterrupted"",
                ""HookParameters"": [""BasePlayer"", ""string"", ""ulong"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeAdded"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeRemoved"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""string""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnHomeAccepted"",
                ""HookParameters"": [""BasePlayer"", ""string"", ""int""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportAccepted"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer"", ""int""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportRequestCompleted"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnTeleportRejected"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer""],
                ""PluginName"": ""NTeleportation""
            },
            {
                ""HookName"": ""OnPlayerTeleported"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""Vector3""],
                ""PluginName"": ""NTeleportation""
            },

            {
                ""HookName"": ""StartRaidBlocking"",
                ""HookParameters"": [""BasePlayer"", ""Vector3"", ""bool""],
                ""PluginName"": ""NoEscape""
            },
            {
                ""HookName"": ""StopBlocking"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""NoEscape""
            },
            {
                ""HookName"": ""StopRaidBlocking"",
                ""HookParameters"": [""string""],
                ""PluginName"": ""NoEscape""
            },
            {
                ""HookName"": ""StartCombatBlocking"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""NoEscape""
            },
            {
                ""HookName"": ""StopCombatBlocking"",
                ""HookParameters"": [""string""],
                ""PluginName"": ""NoEscape""
            },

            {
                ""HookName"": ""API_AddOnlinePrisoner"",
                ""HookParameters"": [""BasePlayer"", ""bool"", ""int""],
                ""PluginName"": ""XPrison""
            },
            {
                ""HookName"": ""API_RemoveOnlinePrisoner"",
                ""HookParameters"": [""BasePlayer""],
                ""PluginName"": ""XPrison""
            },
            {
                ""HookName"": ""API_AddOfflinePrisoner"",
                ""HookParameters"": [""BasePlayer"", ""bool"", ""int""],
                ""PluginName"": ""XPrison""
            },
            {
                ""HookName"": ""API_RemoveOfflinePrisoner"",
                ""HookParameters"": [""ulong""],
                ""PluginName"": ""XPrison""
            },

            {
                ""HookName"": ""OnQuestCompleted"",
                ""HookParameters"": [""BasePlayer"", ""string""],
                ""PluginName"": ""XDQuest""
            },
            {
                ""HookName"": ""OnQuestProgress"",
                ""HookParameters"": [""ulong"", ""QuestType"", ""string"", ""string"", ""List<Item>"", ""int""],
                ""PluginName"": ""XDQuest""
            },

            {
                ""HookName"": ""API_SET_BALANCE"",
                ""HookParameters"": [""string"", ""int"", ""ItemContainer""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_SET_BALANCE"",
                ""HookParameters"": [""ulong"", ""int"", ""ItemContainer""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_SET_BALANCE"",
                ""HookParameters"": [""BasePlayer"", ""int"", ""ItemContainer""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_REMOVE_BALANCE"",
                ""HookParameters"": [""string"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_REMOVE_BALANCE"",
                ""HookParameters"": [""BasePlayer"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_REMOVE_BALANCE"",
                ""HookParameters"": [""ulong"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""ulong"", ""ulong"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""BasePlayer"", ""BasePlayer"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""string"", ""string"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""ulong"", ""string"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""string"", ""ulong"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""BasePlayer"", ""ulong"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""BasePlayer"", ""string"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""ulong"", ""BasePlayer"", ""int""],
                ""PluginName"": ""IQEconomic""
            },
            {
                ""HookName"": ""API_TRANSFERS"",
                ""HookParameters"": [""string"", ""BasePlayer"", ""int""],
                ""PluginName"": ""IQEconomic""
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