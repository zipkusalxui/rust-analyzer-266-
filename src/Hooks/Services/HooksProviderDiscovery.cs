using System;
using System.Linq;
using System.Reflection;
using RustAnalyzer.src.Hooks.Attributes;
using RustAnalyzer.src.Hooks.Interfaces;
using RustAnalyzer.src.DeprecatedHooks.Interfaces;

namespace RustAnalyzer.src.Hooks.Services
{
    public static class HooksProviderDiscovery
    {
        public static IHooksProvider? CreateRegularProvider(string version)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            var providerType = assembly.GetTypes()
                .Where(t => typeof(IHooksProvider).IsAssignableFrom(t) && !t.IsAbstract)
                .Where(t => t.GetCustomAttribute<HooksVersionAttribute>()?.Version == version)
                .FirstOrDefault();

            return providerType != null ? 
                (IHooksProvider)Activator.CreateInstance(providerType) : 
                null;
        }

        public static IDeprecatedHooksProvider? CreateDeprecatedProvider(string version)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            var providerType = assembly.GetTypes()
                .Where(t => typeof(IDeprecatedHooksProvider).IsAssignableFrom(t) && !t.IsAbstract)
                .Where(t => t.GetCustomAttribute<HooksVersionAttribute>()?.Version == version)
                .FirstOrDefault();

            return providerType != null ? 
                (IDeprecatedHooksProvider)Activator.CreateInstance(providerType) : 
                null;
        }
    }
} 