using System;

namespace RustAnalyzer.src.Hooks.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HooksVersionAttribute : Attribute
    {
        public string Version { get; }

        public HooksVersionAttribute(string version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
} 