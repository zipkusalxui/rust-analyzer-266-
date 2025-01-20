using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RustAnalyzer.Models;
using RustAnalyzer.Utils;

namespace RustAnalyzer.src.Hooks.Interfaces
{
    public abstract class BaseJsonHooksProvider : IHooksProvider
    {
        public abstract string Version { get; }
        protected abstract string JsonContent { get; }
        
        public List<HookModel> GetHooks()
        {
            // Общая логика парсинга JSON
            try
            {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                using var doc = JsonDocument.Parse(JsonContent);
                var hooksJson = doc.RootElement.GetProperty("hooks").GetRawText();
                var hooks = JsonSerializer.Deserialize<List<string>>(hooksJson, options);
                
                return hooks
                    .Select(HooksUtils.ParseHookString)
                    .Where(h => h != null)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<HookModel>();
            }
        }
    }
}