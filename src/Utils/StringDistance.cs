using System;
using System.Collections.Generic;
using System.Linq;

namespace RustAnalyzer.Utils
{
    public static class StringDistance
    {
        public static int GetLevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2))
                return s1.Length;

            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,      // deletion
                        d[i, j - 1] + 1),     // insertion
                        d[i - 1, j - 1] + cost); // substitution
                }
            }

            return d[s1.Length, s2.Length];
        }

        public static IEnumerable<string> FindSimilarPrefabs(string input, IEnumerable<string> prefabs, int maxSuggestions = 3)
        {
            return prefabs
                .Select(p => new { Prefab = p, Distance = GetLevenshteinDistance(input, p) })
                .OrderBy(x => x.Distance)
                .Take(maxSuggestions)
                .Select(x => x.Prefab);
        }

        public static IEnumerable<string> FindSimilarShortNames(string input, IEnumerable<string> prefabs, int maxSuggestions = 3)
        {
            return prefabs
                .Select(p => System.IO.Path.GetFileNameWithoutExtension(p))
                .Distinct()
                .Select(p => new { ShortName = p, Distance = GetLevenshteinDistance(input, p) })
                .OrderBy(x => x.Distance)
                .Take(maxSuggestions)
                .Select(x => x.ShortName);
        }

        public static IEnumerable<(string key, string value)> FindKeyValues(string input, IEnumerable<(string key, string value)> prefabs, int maxSuggestions = 3)
        {
            return prefabs
                .Select(p => new { p.key, ShortName = p.value, Distance = GetLevenshteinDistance(input, p.value) })
                .OrderBy(x => x.Distance)
                .Take(maxSuggestions)
                .Select(x => (x.key, x.ShortName));
        }
    }
}
