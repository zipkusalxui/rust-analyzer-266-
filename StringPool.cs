using System.Collections.Generic;

namespace RustAnalyzer
{
    public class StringPool
    {
        public static Dictionary<string, uint> toNumber;

        static StringPool()
        {
            toNumber = StringPoolJson.GetToNumber();
        }

        public static uint Get(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0u;
            }

            if (toNumber.TryGetValue(str, out var value))
            {
                return value;
            }

            return 0u;
        }
    }
}