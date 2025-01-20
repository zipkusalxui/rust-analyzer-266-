using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using RustAnalyzer.Models;
using RustAnalyzer.Utils;
using RustAnalyzer.src.Hooks.Providers;
using RustAnalyzer.src.Hooks.Interfaces;

namespace RustAnalyzer
{
    /// <summary>
    /// Contains Rust-specific hook signatures and logic to identify them.
    /// </summary>
    public static class HooksConfiguration
    {
        private static IHooksProvider? _currentProvider;
        private static ImmutableList<HookModel> _hooks = ImmutableList<HookModel>.Empty;

        static HooksConfiguration()
        {
            try
            {
                _currentProvider = new HooksLastProvider() as IHooksProvider;
                if (_currentProvider != null)
                {
                    _hooks = ImmutableList.CreateRange(_currentProvider.GetHooks());
                }
            }
            catch (Exception)
            {
                _currentProvider = null;
                _hooks = ImmutableList<HookModel>.Empty;
            }
        }

        /// <summary>
        /// Get current version of game
        /// </summary>
        public static string? CurrentVersion => _currentProvider?.Version;

        /// <summary>
        /// Gets all configured hook signatures.
        /// </summary>
        public static ImmutableList<HookModel> HookSignatures => _hooks;

        /// <summary>
        /// Checks if a given method name or signature is a known hook.
        /// This method supports both full signatures and just method names.
        /// </summary>
        public static bool IsHook(IMethodSymbol method)
        {
            if (method == null || method.ContainingType == null ||
                !HooksUtils.IsRustClass(method.ContainingType))
                return false;

            var methodSignature = HooksUtils.GetMethodSignature(method);
            if (methodSignature == null) return false;

            // Находим все хуки с таким же именем
            var matchingHooks = _hooks.Where(s => s.HookName == methodSignature.HookName).ToList();

            foreach (var hook in matchingHooks)
            {
                // Проверяем количество параметров
                if (hook.HookParameters.Count != method.Parameters.Length)
                    continue;

                bool allParametersMatch = true;
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    var methodParam = method.Parameters[i].Type;
                    var hookParamName = hook.HookParameters[i];

                    // Проверяем соответствие типов
                    if (!HooksUtils.IsTypeCompatible(methodParam, hookParamName))
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a given method signature exactly matches a known hook signature.
        /// This method requires the full signature to match.
        /// </summary>
        public static bool IsKnownHook(IMethodSymbol method)
        {
            if (method == null || method.ContainingType == null ||
                !HooksUtils.IsRustClass(method.ContainingType))
                return false;

            var methodSignature = HooksUtils.GetMethodSignature(method);
            if (methodSignature == null)
                return false;

            return _hooks.Any(s => s.HookName == methodSignature.HookName);
        }

        /// <summary>
        /// Returns hooks with similar names to the method.
        /// </summary>
        public static IEnumerable<string> GetSimilarHooks(IMethodSymbol method, int maxSuggestions = 3)
        {
            if (method == null || method.ContainingType == null ||
                !HooksUtils.IsRustClass(method.ContainingType))
                return Enumerable.Empty<string>();

            return StringDistance.FindSimilarShortNames(
                method.Name,
                _hooks.Select(h => h.HookName).Distinct(),
                maxSuggestions);
        }
    }
}
