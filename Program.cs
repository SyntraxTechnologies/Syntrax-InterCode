using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

class SyntraxIDE : Form
{
    MenuStrip menu;
    TabControl tabs;
    TreeView explorer;
    RichTextBox console;

    SplitContainer vertical;
    SplitContainer horizontal;

    string currentFolder = "";

    Color bgEditor = Color.FromArgb(30, 30, 30);
    Color bgSidebar = Color.FromArgb(37, 37, 38);
    Color textColor = Color.FromArgb(212, 212, 212);

    public SyntraxIDE()
    {
        Text = "Syntrax IDE";
        Width = 1200;
        Height = 750;

        KeyPreview = true;

        BuildLayout();
        BuildMenu();
        ApplyTheme();
        SetupShortcuts();
    }

    void BuildLayout()
    {
        menu = new MenuStrip();
        Controls.Add(menu);

        vertical = new SplitContainer();
        vertical.Dock = DockStyle.Fill;
        vertical.SplitterDistance = 250;

        horizontal = new SplitContainer();
        horizontal.Dock = DockStyle.Fill;
        horizontal.Orientation = Orientation.Horizontal;
        horizontal.SplitterDistance = 520;

        explorer = new TreeView();
        explorer.Dock = DockStyle.Fill;
        explorer.NodeMouseDoubleClick += OpenFromExplorer;

        tabs = new TabControl();
        tabs.Dock = DockStyle.Fill;

        console = new RichTextBox();
        console.Dock = DockStyle.Fill;
        console.Font = new Font("Consolas", 10);
        console.ReadOnly = true;

        horizontal.Panel1.Controls.Add(tabs);
        horizontal.Panel2.Controls.Add(console);

        vertical.Panel1.Controls.Add(explorer);
        vertical.Panel2.Controls.Add(horizontal);

        Controls.Add(vertical);
    }

    void BuildMenu()
    {
        var file = new ToolStripMenuItem("File");
        var edit = new ToolStripMenuItem("Edit");
        var run = new ToolStripMenuItem("Run");

        file.DropDownItems.Add("New", null, NewFile);
        file.DropDownItems.Add("Open File", null, OpenFile);
        file.DropDownItems.Add("Open Folder", null, OpenFolder);
        file.DropDownItems.Add("Save", null, SaveFile);
        file.DropDownItems.Add("Save As", null, SaveAs);

        edit.DropDownItems.Add("Find", null, FindText);

        run.DropDownItems.Add("Run (F5)", null, RunCode);

        menu.Items.Add(file);
        menu.Items.Add(edit);
        menu.Items.Add(run);
    }

    void ApplyTheme()
    {
        BackColor = bgEditor;

        menu.BackColor = bgSidebar;
        menu.ForeColor = textColor;

        explorer.BackColor = bgSidebar;
        explorer.ForeColor = Color.White;

        tabs.BackColor = bgEditor;

        console.BackColor = bgEditor;
        console.ForeColor = Color.LightGreen;
    }

    void SetupShortcuts()
    {
        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.S) SaveFile(null, null);
            if (e.Control && e.KeyCode == Keys.O) OpenFile(null, null);
            if (e.Control && e.KeyCode == Keys.N) NewFile(null, null);
            if (e.KeyCode == Keys.F5) RunCode(null, null);
        };
    }

    RichTextBox CreateEditor(string text)
    {
        RichTextBox editor = new RichTextBox();

        editor.Dock = DockStyle.Fill;
        editor.Font = new Font("Consolas", 11);
        editor.Text = text;
        editor.AcceptsTab = true;
        editor.WordWrap = false;

        editor.BackColor = bgEditor;
        editor.ForeColor = textColor;
        editor.BorderStyle = BorderStyle.None;

        editor.TextChanged += (s, e) => Highlight(editor);

        return editor;
    }

    void Highlight(RichTextBox editor)
    {
        int pos = editor.SelectionStart;

        string[] keywords =
        {
            "class","void","int","string","public","using",
            "return","if","else","for","while","import",
            "def","print"
        };

        editor.SelectAll();
        editor.SelectionColor = textColor;

        foreach (string word in keywords)
        {
            int start = 0;

            while ((start = editor.Text.IndexOf(word, start)) != -1)
            {
                editor.Select(start, word.Length);
                editor.SelectionColor = Color.DeepSkyBlue;
                start += word.Length;
            }
        }

        editor.SelectionStart = pos;
        editor.SelectionColor = textColor;
    }

    void NewFile(object sender, EventArgs e)
    {
        var editor = CreateEditor("");

        TabPage tab = new TabPage("Untitled");
        tab.Controls.Add(editor);

        tabs.TabPages.Add(tab);
    }

    void OpenFile(object sender, EventArgs e)
    {
        OpenFileDialog dlg = new OpenFileDialog();

        if (dlg.ShowDialog() != DialogResult.OK) return;

        string text = File.ReadAllText(dlg.FileName);

        var editor = CreateEditor(text);

        TabPage tab = new TabPage(Path.GetFileName(dlg.FileName));
        tab.Tag = dlg.FileName;
        tab.Controls.Add(editor);

        tabs.TabPages.Add(tab);
    }

    void SaveFile(object sender, EventArgs e)
    {
        if (tabs.SelectedTab == null) return;

        RichTextBox editor = tabs.SelectedTab.Controls[0] as RichTextBox;

        if (tabs.SelectedTab.Tag == null)
        {
            SaveAs(null, null);
            return;
        }

        File.WriteAllText(tabs.SelectedTab.Tag.ToString(), editor.Text);
    }

    void SaveAs(object sender, EventArgs e)
    {
        if (tabs.SelectedTab == null) return;

        SaveFileDialog dlg = new SaveFileDialog();

        if (dlg.ShowDialog() != DialogResult.OK) return;

        RichTextBox editor = tabs.SelectedTab.Controls[0] as RichTextBox;

        File.WriteAllText(dlg.FileName, editor.Text);

        tabs.SelectedTab.Tag = dlg.FileName;
        tabs.SelectedTab.Text = Path.GetFileName(dlg.FileName);
    }

    void OpenFolder(object sender, EventArgs e)
    {
        FolderBrowserDialog dlg = new FolderBrowserDialog();

        if (dlg.ShowDialog() != DialogResult.OK) return;

        explorer.Nodes.Clear();

        currentFolder = dlg.SelectedPath;

        TreeNode root = new TreeNode(currentFolder);
        explorer.Nodes.Add(root);

        LoadFolder(root, currentFolder);
    }

    void LoadFolder(TreeNode node, string path)
    {
        foreach (string dir in Directory.GetDirectories(path))
        {
            TreeNode d = new TreeNode(Path.GetFileName(dir));
            node.Nodes.Add(d);
            LoadFolder(d, dir);
        }

        foreach (string file in Directory.GetFiles(path))
        {
            node.Nodes.Add(file);
        }
    }

    void OpenFromExplorer(object sender, TreeNodeMouseClickEventArgs e)
    {
        string file = e.Node.Text;

        if (!File.Exists(file)) return;

        string text = File.ReadAllText(file);

        var editor = CreateEditor(text);

        TabPage tab = new TabPage(Path.GetFileName(file));
        tab.Tag = file;
        tab.Controls.Add(editor);

        tabs.TabPages.Add(tab);
    }

    void RunCode(object sender, EventArgs e)
    {
        if (tabs.SelectedTab == null) return;

        RichTextBox editor = tabs.SelectedTab.Controls[0] as RichTextBox;

        string file = tabs.SelectedTab.Tag as string;

        if (file == null)
        {
            file = Path.Combine(Path.GetTempPath(), "temp.cs");
            File.WriteAllText(file, editor.Text);
        }

        string ext = Path.GetExtension(file).ToLower();

        if (ext == ".py")
            RunPython(file);
        else
            RunCSharp(file);
    }

    void RunPython(string file)
    {
        Process p = new Process();

        p.StartInfo.FileName = "python";
        p.StartInfo.Arguments = "\"" + file + "\"";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;

        p.Start();

        console.Text = p.StandardOutput.ReadToEnd();

        p.WaitForExit();
    }

    void RunCSharp(string file)
    {
        string exe = Path.Combine(Path.GetTempPath(), "run.exe");

        Process p = new Process();

        p.StartInfo.FileName = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
        p.StartInfo.Arguments = "/out:" + exe + " \"" + file + "\"";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;

        p.Start();

        console.Text = p.StandardOutput.ReadToEnd();

        p.WaitForExit();

        if (File.Exists(exe))
            Process.Start(exe);
    }

    void FindText(object sender, EventArgs e)
    {
        if (tabs.SelectedTab == null) return;

        Form f = new Form();
        f.Text = "Find";
        f.Width = 300;
        f.Height = 120;

        TextBox box = new TextBox();
        box.Dock = DockStyle.Top;

        Button btn = new Button();
        btn.Text = "Find";
        btn.Dock = DockStyle.Bottom;

        f.Controls.Add(box);
        f.Controls.Add(btn);

        btn.Click += (s, ev) =>
        {
            RichTextBox editor = tabs.SelectedTab.Controls[0] as RichTextBox;

            int index = editor.Text.IndexOf(box.Text);

            if (index >= 0)
            {
                editor.Select(index, box.Text.Length);
                editor.ScrollToCaret();
            }

            f.Close();
        };

        f.ShowDialog();
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new SyntraxIDE());
    }
}