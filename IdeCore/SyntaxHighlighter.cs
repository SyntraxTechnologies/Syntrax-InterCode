using System.Drawing;
using System.Windows.Forms;

namespace SyntraxInterCode.IdeCore
{
    public static class SyntaxHighlighter
    {
        static string[] keywords =
        {
            "class","public","void","int","string",
            "using","return","if","else","for","while"
        };

        public static void Highlight(RichTextBox box)
        {
            int pos = box.SelectionStart;

            box.SelectAll();
            box.SelectionColor = Color.Black;

            string text = box.Text;

            for (int i = 0; i < keywords.Length; i++)
            {
                int index = 0;

                while ((index = text.IndexOf(keywords[i], index)) != -1)
                {
                    box.Select(index, keywords[i].Length);
                    box.SelectionColor = Color.RoyalBlue;
                    index += keywords[i].Length;
                }
            }

            box.SelectionStart = pos;
        }
    }
}