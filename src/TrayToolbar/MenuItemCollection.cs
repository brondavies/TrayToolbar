using System.Collections.ObjectModel;

namespace TrayToolbar
{
    public class MenuItemCollection : ObservableCollection<ToolStripMenuItem>
    {
        public MenuItemCollection() { }
        public bool NeedsRefresh { get; set; } = true;

        public ToolStripMenuItem? CreateFolder(string path, ToolStripItemClickedEventHandler handler)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            ToolStripMenuItem? parent = null;
            foreach (var part in parts)
            {
                var added = AddFolder(parent, part, handler, out ToolStripMenuItem? menu);
                parent = menu;
                if (!added) { Add(menu!); }
            }
            return parent;
        }

        private bool AddFolder(ToolStripMenuItem? parent, string name, ToolStripItemClickedEventHandler handler, out ToolStripMenuItem? menu)
        {
            var result = false;//true if it was already added
            if (parent != null)
            {
                menu = (ToolStripMenuItem?)parent.DropDownItems.Find(name, false).FirstOrDefault();
                result = menu != null;
            }
            else
            {
                menu = this.FirstOrDefault(m => m.Name == name);
                result = menu != null;
            }
            if (menu == null)
            {
                menu = new ToolStripMenuItem(name) { Name = name };
                menu.DropDownItemClicked += handler;
                parent?.DropDownItems.Add(menu);
                result = parent != null;
            }
            return result;
        }
    }
}