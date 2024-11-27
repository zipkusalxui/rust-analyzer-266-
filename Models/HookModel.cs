using System;
using System.Collections.Generic;
using System.Text;

namespace RustAnalyzer.Models
{
    public class HookModel
    {
        public string HookName { get; set; }
        public List<string> HookParameters { get; set; }

        public override string ToString()
        {
            return (HookName + "(" + string.Join(", ", HookParameters) + ")").Replace(" ", "");
        }
    }
}
