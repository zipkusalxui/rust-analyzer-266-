using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RustAnalyzer.Models;
using RustAnalyzer.src.DeprecatedHooks.Interfaces;
using RustAnalyzer.Utils;

namespace RustAnalyzer.src.Configuration
{
    internal static class DeprecatedHooksConfiguration
    {
        private static IDeprecatedHooksProvider _provider = null!;
        private static List<DeprecatedHookModel> _hooks = new();

        /// <summary>
        /// Initializes the hooks configuration with the specified provider
        /// </summary>
        public static void Initialize(IDeprecatedHooksProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            try
            {
                _provider = provider;
                _hooks = provider.GetHooks();
            }
            catch (Exception)
            {
                _provider = null;
                _hooks = new List<DeprecatedHookModel>();
            }
        }

        public static bool IsHook(IMethodSymbol method, out DeprecatedHookModel? hookInfo)
        {
            hookInfo = null;
            if (method == null) return false;

            foreach (var hook in _hooks)
            {
                if (hook.OldHook.HookName == method.Name)
                {
                    hookInfo = hook;
                    return true;
                }
            }

            return false;
        }

        public static bool IsHook(IMethodSymbol method) => IsHook(method, out _);
    }
}