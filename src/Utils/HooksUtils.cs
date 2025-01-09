using RustAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RustAnalyzer.Utils
{
    internal static class HooksUtils
    {
        public static HookModel ParseHookString(string hookString)
        {
            if (string.IsNullOrWhiteSpace(hookString))
            {
                return null;
            }

            // Extracting the hook name and parameters
            var openParenIndex = hookString.IndexOf('(');
            var closeParenIndex = hookString.IndexOf(')');

            if (openParenIndex < 0 || closeParenIndex < 0 || closeParenIndex <= openParenIndex)
            {
                throw new FormatException($"Invalid hook format: {hookString}");
            }

            var hookName = hookString.Substring(0, openParenIndex);
            var parameters = hookString.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

            // Manually trim each parameter after splitting
            var parameterList = parameters
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            return new HookModel
            {
                HookName = hookName,
                HookParameters = parameterList
            };
        }
    }
}
