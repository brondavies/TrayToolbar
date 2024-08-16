using System.Collections.ObjectModel;
using TrayToolbar.Extensions;

namespace TrayToolbar
{
    public class MenuItemCollection : ObservableCollection<ToolStripMenuItem>
    {
        public MenuItemCollection() { }
        public bool NeedsRefresh { get; set; } = true;

        public ToolStripMenuItem? CreateFolder(string path, ToolStripItemClickedEventHandler handler, MouseEventHandler mouseDownHandler)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            ToolStripMenuItem? parent = null;
            foreach (var part in parts)
            {
                var added = AddFolder(parent, part.ToMenuName(), handler, mouseDownHandler, out ToolStripMenuItem? menu);
                parent = menu;
                if (!added && menu != null)
                {
                    //Adds to the menu with folders first, alphabetically
                    Insert(LastSubMenuIndex(this), menu);
                }
            }
            return parent;
        }

        private static int LastSubMenuIndex(MenuItemCollection list)
        {
            return list.Count(e => e.HasDropDown);
        }

        private static int LastSubMenuIndex(ToolStripItemCollection list)
        {
            int i = 0;
            foreach (ToolStripMenuItem item in list)
            {
                if (item.HasDropDown) i++;
            }
            return i;
        }

        private bool NextMenu(ToolStripItem i, string? name)
        {
            return i is ToolStripMenuItem { HasDropDown: true } && (string.Compare(i.Name, name, true) > 0);
        }

        private bool AddFolder(ToolStripMenuItem? parent, string name, ToolStripItemClickedEventHandler handler, MouseEventHandler mouseDownHandler, out ToolStripMenuItem? menu)
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
                menu.MouseDown += mouseDownHandler;
                result = parent != null;
                if (result)
                {
                    var items = parent!.DropDownItems;
                    if (items != null)
                    {
                        //Adds to the menu with folders first, alphabetically
                        items.Insert(LastSubMenuIndex(parent.DropDownItems), menu);
                    }
                }
            }
            return result;
        }
    }
}