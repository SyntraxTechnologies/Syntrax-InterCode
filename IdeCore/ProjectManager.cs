using System.Windows.Forms;

namespace SyntraxInterCode.IdeCore
{
    public class ProjectManager
    {
        TreeView tree;
        EditorEngine editor;

        public ProjectManager(TreeView t, EditorEngine e)
        {
            tree = t;
            editor = e;

            tree.Nodes.Clear();
            tree.Nodes.Add("Solution");
        }
    }
}