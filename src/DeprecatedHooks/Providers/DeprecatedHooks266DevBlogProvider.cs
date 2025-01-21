using RustAnalyzer.src.Hooks.Attributes;
using RustAnalyzer.src.DeprecatedHooks.Interfaces;
using RustAnalyzer.Models;
using System;
using System.Reflection;

namespace RustAnalyzer.src.DeprecatedHooks.Providers
{
    [HooksVersion("266Dev")]
    public class DeprecatedHooks266DevBlogProvider : BaseDeprecatedJsonHooksProvider
    {
        protected override string JsonContent => "{\r\n  \"deprecated\": {\r\n} }";
    }
}
