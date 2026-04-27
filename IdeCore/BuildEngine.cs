using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace SyntraxInterCode.IdeCore
{
    public class BuildEngine
    {
        ListBox output;
        ListBox errors;

        string exePath = Path.Combine(Application.StartupPath, "app.exe");

        public BuildEngine(ListBox outBox, ListBox errBox)
        {
            output = outBox;
            errors = errBox;
        }

        public void Build(string code)
        {
            output.Items.Clear();
            errors.Items.Clear();

            try
            {
                if (File.Exists(exePath))
                    File.Delete(exePath);
            }
            catch { }

            CSharpCodeProvider provider = new CSharpCodeProvider();

            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = true;
            cp.OutputAssembly = exePath;

            CompilerResults res = provider.CompileAssemblyFromSource(cp, code);

            if (res.Errors.Count > 0)
            {
                foreach (CompilerError err in res.Errors)
                {
                    errors.Items.Add("Line " + err.Line + ": " + err.ErrorText);
                }
            }
            else
            {
                output.Items.Add("Build succeeded");
            }
        }

        public void Run()
        {
            if (File.Exists(exePath))
                System.Diagnostics.Process.Start(exePath);
        }
    }
}