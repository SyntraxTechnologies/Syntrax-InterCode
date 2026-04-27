using System;
using System.Drawing;
using System.Windows.Forms;

namespace SyntraxInterCode.IdeCore
{
    public class EditorEngine
    {
        private TabControl tabs;

        public EditorEngine(TabControl tabControl)
        {
            tabs = tabControl;
        }

        public void CreateTab(string name, string text)
        {
            TabPage page = new TabPage(name);

            RichTextBox box = new RichTextBox();
            box.Dock = DockStyle.Fill;
            box.Font = new Font("Consolas", 10);
            box.WordWrap = false;
            box.Text = text;

            box.TextChanged += delegate
            {
                SyntaxHighlighter.Highlight(box);
                IntellisenseEngine.Scan(box);
            };

            page.Controls.Add(box);
            tabs.TabPages.Add(page);
        }

        public RichTextBox CurrentEditor()
        {
            if (tabs.SelectedTab == null) return null;
            return tabs.SelectedTab.Controls[0] as RichTextBox;
        }
    }
}