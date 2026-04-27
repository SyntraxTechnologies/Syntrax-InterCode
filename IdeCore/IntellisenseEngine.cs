using System.Windows.Forms;

namespace SyntraxInterCode.IdeCore
{
    public static class IntellisenseEngine
    {
        static string[] words =
        {
            "class","public","private","void","int","string",
            "return","if","else","for","while","using"
        };

        public static void Scan(RichTextBox box)
        {
            // lightweight placeholder engine (stable foundation)
            // real IntelliSense would require parsing engine (Roslyn-level)
        }
    }
}