using RustAnalyzer.src.Hooks.Attributes;
using RustAnalyzer.src.DeprecatedHooks.Interfaces;
using RustAnalyzer.Models;
using System;
using System.Reflection;

namespace RustAnalyzer.src.DeprecatedHooks.Providers
{
    [HooksVersion("240Dev")]
    public class DeprecatedHooks240DevBlogProvider : BaseDeprecatedJsonHooksProvider
    {
        protected override string JsonContent => "{\r\n  \"deprecated\": {\r\n} }";
    }
}
