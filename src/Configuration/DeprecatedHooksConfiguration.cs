using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RustAnalyzer.Models;
using RustAnalyzer.src.DeprecatedHooks.Providers;
using RustAnalyzer.Utils;

namespace RustAnalyzer.src.Configuration
{
    internal static class DeprecatedHooksConfiguration
    {
        private static DeprecatedHooksProvider _provider;
        private static ImmutableList<DeprecatedHookModel> _hooks = ImmutableList<DeprecatedHookModel>.Empty;

        /// <summary>
        /// Initializes the hooks configuration with the specified provider
        /// </summary>
        public static void Initialize(DeprecatedHooksProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            try
            {
                _provider = provider;
                _hooks = ImmutableList.CreateRange(_provider.GetHooks());
            }
            catch (Exception)
            {
                _provider = null;
                _hooks = ImmutableList<DeprecatedHookModel>.Empty;
            }
        }

        public static bool IsHook(IMethodSymbol method, out DeprecatedHookModel? hookInfo)
        {
            hookInfo = null;
            if (method == null || method.ContainingType == null ||
                !HooksUtils.IsRustClass(method.ContainingType))
                return false;

            var methodSignature = HooksUtils.GetMethodSignature(method);
            if (methodSignature == null) return false;

            // Находим все хуки с таким же именем
            var matchingHooks = _hooks
                .Where(h => h.OldHook.HookName == methodSignature.HookName)
                .ToList();

            foreach (var hook in matchingHooks)
            {
                // Проверяем количество параметров
                if (hook.OldHook.HookParameters.Count != method.Parameters.Length)
                    continue;

                bool allParametersMatch = true;
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    var methodParam = method.Parameters[i].Type;
                    var hookParamName = hook.OldHook.HookParameters[i];

                    // Проверяем соответствие типов
                    if (!HooksUtils.IsTypeCompatible(methodParam, hookParamName))
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
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