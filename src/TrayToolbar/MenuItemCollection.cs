using System.Collections.ObjectModel;

namespace TrayToolbar
{
    public class MenuItemCollection : ObservableCollection<ToolStripMenuItem>
    {
        public MenuItemCollection() { }
        public bool NeedsRefresh { get; set; } = true;
    }
}