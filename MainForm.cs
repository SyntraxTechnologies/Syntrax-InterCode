using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace SyntraxInterCode
{
    public class MainForm : Form
    {
        enum Mode { Start, IDE }
        Mode currentMode = Mode.Start;

        class Document
        {
            public TabPage Tab;
            public RichTextBox Editor;
            public Panel LinePanel;
        }

        List<Document> docs = new List<Document>();

        Panel startPage;
        Panel recentPanel;
        TextBox projectBox;
        ComboBox langBox;

        MenuStrip menu;
        ToolStrip toolbar;

        SplitContainer mainSplit;
        SplitContainer bottomSplit;

        TreeView solution;
        TabControl tabs;

        ListBox output;
        ListBox errors;

        string exePath = Path.Combine(Application.StartupPath, "app.exe");
        string recentPath = Path.Combine(Application.StartupPath, "recent.txt");

        public MainForm()
        {
            InitWindow();
            InitStartPage();
            InitIDE();

            SwitchMode(Mode.Start);
            LoadRecent();
        }

        void InitWindow()
        {
            Text = "Syntrax InterCode a0.3";
            Width = 1300;
            Height = 800;
            BackColor = Color.FromArgb(235, 239, 245);
        }

        void InitStartPage()
        {
            startPage = new Panel();
            startPage.Dock = DockStyle.Fill;

            Label title = new Label();
            title.Text = "Syntrax InterCode";
            title.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            title.Location = new Point(30, 30);
            title.AutoSize = true;

            projectBox = new TextBox();
            projectBox.Location = new Point(30, 100);
            projectBox.Width = 250;

            langBox = new ComboBox();
            langBox.Location = new Point(30, 140);
            langBox.Width = 250;
            langBox.Items.AddRange(new object[] { "C#", "C++", "Java", "Python" });
            langBox.SelectedIndex = 0;

            Button newBtn = new Button();
            newBtn.Text = "New Project";
            newBtn.Location = new Point(30, 190);
            newBtn.Width = 250;
            newBtn.Click += NewProject;

            Button openBtn = new Button();
            openBtn.Text = "Open Project";
            openBtn.Location = new Point(30, 230);
            openBtn.Width = 250;
            openBtn.Click += OpenProject;

            recentPanel = new Panel();
            recentPanel.Location = new Point(320, 60);
            recentPanel.Size = new Size(600, 500);
            recentPanel.AutoScroll = true;
            recentPanel.BackColor = Color.White;

            Label recentLabel = new Label();
            recentLabel.Text = "Recent Projects";
            recentLabel.Location = new Point(320, 30);
            recentLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            recentLabel.AutoSize = true;

            startPage.Controls.Add(title);
            startPage.Controls.Add(projectBox);
            startPage.Controls.Add(langBox);
            startPage.Controls.Add(newBtn);
            startPage.Controls.Add(openBtn);
            startPage.Controls.Add(recentPanel);
            startPage.Controls.Add(recentLabel);

            Controls.Add(startPage);
        }

        void InitIDE()
        {
            menu = new MenuStrip();

            ToolStripMenuItem file = new ToolStripMenuItem("File");
            file.DropDownItems.Add("New", null, NewProject);
            file.DropDownItems.Add("Open", null, OpenProject);
            file.DropDownItems.Add("Save", null, SaveFile);

            menu.Items.Add(file);
            menu.Items.Add("Build", null, Build);
            menu.Items.Add("Run", null, Run);

            toolbar = new ToolStrip();
            toolbar.Items.Add("New", null, NewProject);
            toolbar.Items.Add("Open", null, OpenProject);
            toolbar.Items.Add("Save", null, SaveFile);
            toolbar.Items.Add("Build", null, Build);
            toolbar.Items.Add("Run", null, Run);

            menu.Dock = DockStyle.Top;
            toolbar.Dock = DockStyle.Top;

            Controls.Add(menu);
            Controls.Add(toolbar);

            mainSplit = new SplitContainer();
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.SplitterDistance = 240;

            solution = new TreeView();
            solution.Dock = DockStyle.Fill;
            solution.Nodes.Add("Solution");

            bottomSplit = new SplitContainer();
            bottomSplit.Dock = DockStyle.Fill;
            bottomSplit.Orientation = Orientation.Horizontal;
            bottomSplit.SplitterDistance = 520;
            bottomSplit.Panel2MinSize = 120;

            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;

            output = new ListBox();
            output.Dock = DockStyle.Fill;

            errors = new ListBox();
            errors.Dock = DockStyle.Fill;

            TabControl bottomTabs = new TabControl();

            TabPage outTab = new TabPage("Output");
            TabPage errTab = new TabPage("Errors");

            outTab.Controls.Add(output);
            errTab.Controls.Add(errors);

            bottomTabs.TabPages.Add(outTab);
            bottomTabs.TabPages.Add(errTab);

            bottomSplit.Panel1.Controls.Add(tabs);
            bottomSplit.Panel2.Controls.Add(bottomTabs);

            mainSplit.Panel1.Controls.Add(solution);
            mainSplit.Panel2.Controls.Add(bottomSplit);

            Controls.Add(mainSplit);
        }

        void SwitchMode(Mode m)
        {
            currentMode = m;

            bool start = (m == Mode.Start);

            startPage.Visible = start;
            mainSplit.Visible = !start;
            menu.Visible = !start;
            toolbar.Visible = !start;
        }

        void NewProject(object sender, EventArgs e)
        {
            SwitchMode(Mode.IDE);
            CreateTab("Untitled.cs", "");
        }

        void OpenProject(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();

            if (d.ShowDialog() == DialogResult.OK)
            {
                SwitchMode(Mode.IDE);
                CreateTab(Path.GetFileName(d.FileName), File.ReadAllText(d.FileName));
                AddRecent(d.FileName);
            }
        }

        void SaveFile(object sender, EventArgs e)
        {
            RichTextBox ed = GetEditor();
            if (ed == null) return;

            SaveFileDialog d = new SaveFileDialog();

            if (d.ShowDialog() == DialogResult.OK)
                File.WriteAllText(d.FileName, ed.Text);
        }

        void AddRecent(string path)
        {
            File.AppendAllText(recentPath, path + Environment.NewLine);
            LoadRecent();
        }

        void LoadRecent()
        {
            recentPanel.Controls.Clear();
            if (!File.Exists(recentPath)) return;

            string[] lines = File.ReadAllLines(recentPath);

            int y = 10;

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string p = lines[i];
                if (string.IsNullOrEmpty(p)) continue;

                Button b = new Button();
                b.Text = Path.GetFileName(p);
                b.Tag = p;
                b.Width = 560;
                b.Height = 35;
                b.Location = new Point(10, y);

                b.Click += OpenRecent;

                recentPanel.Controls.Add(b);
                y += 40;
            }
        }

        void OpenRecent(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;

            string path = b.Tag.ToString();
            if (!File.Exists(path)) return;

            SwitchMode(Mode.IDE);
            CreateTab(Path.GetFileName(path), File.ReadAllText(path));
        }

        void CreateTab(string name, string text)
        {
            Document d = new Document();

            TabPage tab = new TabPage(name);

            SplitContainer sc = new SplitContainer();
            sc.Dock = DockStyle.Fill;
            sc.SplitterDistance = 55;
            sc.IsSplitterFixed = true;

            Panel linePanel = new Panel();
            linePanel.Dock = DockStyle.Fill;
            linePanel.BackColor = Color.FromArgb(245, 245, 245);

            RichTextBox editor = new RichTextBox();
            editor.Dock = DockStyle.Fill;
            editor.Font = new Font("Consolas", 10);
            editor.WordWrap = false;
            editor.Text = text;

            editor.TextChanged += delegate { linePanel.Invalidate(); };
            editor.VScroll += delegate { linePanel.Invalidate(); };

            linePanel.Paint += delegate(object s, PaintEventArgs e)
            {
                DrawLines(e, editor);
            };

            sc.Panel1.Controls.Add(linePanel);
            sc.Panel2.Controls.Add(editor);

            tab.Controls.Add(sc);
            tabs.TabPages.Add(tab);

            d.Tab = tab;
            d.Editor = editor;
            d.LinePanel = linePanel;

            docs.Add(d);
        }

        RichTextBox GetEditor()
        {
            if (tabs.SelectedTab == null) return null;

            for (int i = 0; i < docs.Count; i++)
                if (docs[i].Tab == tabs.SelectedTab)
                    return docs[i].Editor;

            return null;
        }

        void DrawLines(PaintEventArgs e, RichTextBox box)
        {
            e.Graphics.Clear(Color.FromArgb(245, 245, 245));

            int first = box.GetCharIndexFromPosition(new Point(0, 0));
            int line = box.GetLineFromCharIndex(first);

            int h = box.Font.Height;
            int y = 0;

            int count = box.Height / h + 2;

            for (int i = 0; i < count; i++)
            {
                e.Graphics.DrawString((line + i + 1).ToString(),
                    box.Font, Brushes.Gray, 5, y);

                y += h;
            }
        }

        void Build(object sender, EventArgs e)
        {
            output.Items.Clear();
            errors.Items.Clear();

            RichTextBox ed = GetEditor();
            if (ed == null) return;

            CSharpCodeProvider p = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.GenerateExecutable = true;
            cp.OutputAssembly = exePath;

            CompilerResults r = p.CompileAssemblyFromSource(cp, ed.Text);

            if (r.Errors.Count > 0)
            {
                foreach (CompilerError err in r.Errors)
                    errors.Items.Add("Line " + err.Line + ": " + err.ErrorText);
            }
            else
            {
                output.Items.Add("Build succeeded");
            }
        }

        void Run(object sender, EventArgs e)
        {
            if (File.Exists(exePath))
                Process.Start(exePath);
            else
                MessageBox.Show("Build first");
        }
    }
}