using System.Text;

namespace RustAnalyzer.Utils
{
    public static class TextAlignmentUtils
    {
        /// <summary>
        /// Computes the visual column of the start of a member name, accounting for tabs.
        /// </summary>
        /// <param name="line">The source line of code.</param>
        /// <param name="charColumn">The character position in the line (0-based).</param>
        /// <param name="tabSize">The size of a tab in spaces (default is 4).</param>
        /// <returns>The visual column in "spaces".</returns>
        public static int ComputeVisualColumn(string line, int charColumn, int tabSize = 4)
        {
            int visualColumn = 0;
            for (int i = 0; i < charColumn && i < line.Length; i++)
            {
                if (line[i] == '\t')
                {
                    int spacesToAdd = tabSize - (visualColumn % tabSize);
                    visualColumn += spacesToAdd;
                }
                else
                {
                    visualColumn++;
                }
            }
            return visualColumn;
        }

        /// <summary>
        /// Creates a pointer line with '^' characters under the specified member name in the code line.
        /// </summary>
        /// <param name="line">The source line of code.</param>
        /// <param name="charColumn">The starting character position (0-based).</param>
        /// <param name="length">The length of the member name.</param>
        /// <param name="tabSize">The size of a tab in spaces (default is 4).</param>
        /// <returns>A string with spaces and '^' characters highlighting the member.</returns>
        public static string CreatePointerLine(string line, int charColumn, int length, int tabSize = 4)
        {
            // Compute the visual position
            int visualColumn = ComputeVisualColumn(line, charColumn, tabSize);

            // Generate the pointer line
            return new string(' ', visualColumn) + new string('^', length);
        }

        /// <summary>
        /// Expands tabs in the line to spaces based on the specified tab size.
        /// </summary>
        /// <param name="line">The source line of code.</param>
        /// <param name="tabSize">The size of a tab in spaces (default is 4).</param>
        /// <returns>A string where all tabs are replaced with spaces.</returns>
        public static string ExpandTabs(string line, int tabSize = 4)
        {
            int column = 0;
            var result = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '\t')
                {
                    // Add spaces to match the tab size
                    int spacesToAdd = tabSize - (column % tabSize);
                    result.Append(' ', spacesToAdd);
                    column += spacesToAdd;
                }
                else
                {
                    result.Append(c);
                    column++;
                }
            }

            return result.ToString();
        }
    }
}
