using System;
using System.Collections.Generic;
using System.Text;

namespace RustAnalyzer.Models
{
    public class DeprecatedHookModel
    {
        public HookModel OldHook { get; set; }
        public HookModel? NewHook { get; set; }
    }
}
