using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RustAnalyzer.src.Configuration;
using RustAnalyzer.src.DeprecatedHooks.Providers;
using RustAnalyzer.src.Hooks.Providers;
using System;

namespace RustAnalyzer.Configuration
{
    public static class RustVersionProvider
    {
        private static string _version = "unknown";
        private static bool _isInitialized = false;

        public static void Initialize(AnalyzerConfigOptions options)
        {
            Console.WriteLine("[RustAnalyzer] Starting initialization...");
            
            if (!options.TryGetValue("build_property.rustversion", out var version))
            {
                return;
            }

            Console.WriteLine($"[RustAnalyzer] Found RustVersion: {version}");
            _version = version;
            
            if (_version == "LastVersion" && !_isInitialized)
            {
                HooksConfiguration.Initialize(new HooksLastProvider());
                DeprecatedHooksConfiguration.Initialize(new DeprecatedHooksProvider());
                _isInitialized = true;
                Console.WriteLine($"[RustAnalyzer] Successfully initialized with version: {_version}");
            }
            else
            {
                Console.WriteLine($"[RustAnalyzer] Failed to initialize: version must be 'LastV2ersion1', but got '{_version}'");
            }
        }

        public static string GetVersion() => _version;

        public static bool IsInitialized() => _isInitialized;
    }
}
