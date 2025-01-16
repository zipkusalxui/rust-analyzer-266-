using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RustAnalyzer.Models;

namespace RustAnalyzer.src.Configuration
{
    internal class DeprecatedHooksConfiguration
    {
        public static bool IsHook(IMethodSymbol method, out DeprecatedHookModel hookInfo)
        {
            var deprecatedHooks = DeprecatedHooksJson.GetHooks();

            var parameterTypes = string.Join(", ", method.Parameters.Select(p => p.Type.ToString()));
            var methodSignature = $"{method.Name}({parameterTypes})";

            hookInfo = deprecatedHooks.FirstOrDefault(h =>
                $"{h.OldHook.HookName}({string.Join(", ", h.OldHook.HookParameters)})" == methodSignature);

            return hookInfo != null;
        }

        public static bool IsHook(IMethodSymbol method) => IsHook(method, out _);
    }
}