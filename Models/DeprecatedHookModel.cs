using System;
using System.Collections.Generic;
using System.Text;

namespace RustAnalyzer.Models
{
    internal class DeprecatedHookModel
    {
        public HookModel OldHook { get; set; }
        public HookModel NewHook { get; set; }
    }
}
