using System.Collections.Generic;
using RustAnalyzer.Models;

namespace RustAnalyzer.src.DeprecatedHooks.Interfaces
{
    public interface IDeprecatedHooksProvider
    {
        string Version { get; }
        List<DeprecatedHookModel> GetHooks();
    }
} 