using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using RustAnalyzer.Models;

namespace RustAnalyzer
{
    public static class HooksConfiguration
    {
        private static readonly ImmutableList<HookModel> _hooks;

        static HooksConfiguration()
        {
            try
            {
                var hooks = HooksJson.GetHooks();
                _hooks = ImmutableList.CreateRange(hooks);
            }
            catch (Exception ex)
            {
                _hooks = ImmutableList<HookModel>.Empty;
            }
        }

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
            if (method == null)
                return false;

            var methodSignature = GetMethodSignature(method);

            if (methodSignature == null)
                return false;

            // Находим все хуки с таким же именем
            var matchingHooks = _hooks.Where(s => s.HookName == methodSignature.HookName).ToList();

            foreach (var hook in matchingHooks)
            {
                // Проверяем количество параметров
                if (hook.HookParameters.Count != method.Parameters.Length)
                    continue;

                bool allParametersMatch = true;

                // Проверяем каждый параметр
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    var methodParam = method.Parameters[i].Type;
                    var hookParamName = hook.HookParameters[i];

                    // Проверяем соответствие типов
                    if (!IsTypeCompatible(methodParam, hookParamName))
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

        private static bool IsTypeCompatible(ITypeSymbol type, string expectedTypeName)
        {
            // Проверяем точное совпадение имен
            if (type.Name == expectedTypeName || type.ToDisplayString() == expectedTypeName)
                return true;

            // Проверяем базовые типы
            var currentType = type;
            while (currentType.BaseType != null)
            {
                currentType = currentType.BaseType;
                if (currentType.Name == expectedTypeName || currentType.ToDisplayString() == expectedTypeName)
                    return true;
            }

            // Проверяем интерфейсы
            foreach (var iface in type.AllInterfaces)
            {
                if (iface.Name == expectedTypeName || iface.ToDisplayString() == expectedTypeName)
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
            if (method == null)
                return false;

            var methodSignature = GetMethodSignature(method);

            if (methodSignature == null)
                return false;

            return _hooks.Any(s => s.HookName == methodSignature.HookName);
        }

        private static HookModel GetMethodSignature(IMethodSymbol method)
        {
            List<string> parameterTypes = new List<string>();

            foreach (var parameter in method.Parameters)
            {
                var paramType = parameter.Type;
                var typeString = GetFriendlyTypeName(paramType);
                parameterTypes.Add(typeString);
            }

            var hookModel = new HookModel
            {
                HookName = method.Name,
                HookParameters = parameterTypes
            };

            return hookModel;
        }

        private static string GetFriendlyTypeName(ITypeSymbol type)
        {
            if (SpecialTypeMap.TryGetValue(type.SpecialType, out var friendlyName))
            {
                return friendlyName;
            }

            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                var genericTypeName = namedType.ConstructedFrom.Name;
                var genericArguments = namedType.TypeArguments.Select(GetFriendlyTypeName);
                return $"{genericTypeName.Split('`')[0]}<{string.Join(", ", genericArguments)}>";
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                var elementType = GetFriendlyTypeName(arrayType.ElementType);
                return $"{elementType}[]";
            }

            return type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        }

        private static readonly Dictionary<SpecialType, string> SpecialTypeMap = new Dictionary<SpecialType, string>
        {
            { SpecialType.System_Object, "object" },
            { SpecialType.System_Boolean, "bool" },
            { SpecialType.System_Char, "char" },
            { SpecialType.System_SByte, "sbyte" },
            { SpecialType.System_Byte, "byte" },
            { SpecialType.System_Int16, "short" },
            { SpecialType.System_UInt16, "ushort" },
            { SpecialType.System_Int32, "int" },
            { SpecialType.System_UInt32, "uint" },
            { SpecialType.System_Int64, "long" },
            { SpecialType.System_UInt64, "ulong" },
            { SpecialType.System_Decimal, "decimal" },
            { SpecialType.System_Single, "float" },
            { SpecialType.System_Double, "double" },
            { SpecialType.System_String, "string" }
        };
    }
}
