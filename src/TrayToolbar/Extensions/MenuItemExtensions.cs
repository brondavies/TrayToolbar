using System.Diagnostics;

namespace TrayToolbar.Extensions;

public static class MenuItemExtensions
{
    public static void SetAutoClose(this ToolStripDropDown menu, bool autoClose)
    {
        var dropDown = menu;
        dropDown.AutoClose = autoClose;
        while (dropDown.OwnerItem != null && dropDown.OwnerItem is ToolStripDropDownItem owner)
        {
            //There's an extra level to get to the parent menu
            if (owner.OwnerItem != null && owner.OwnerItem is ToolStripDropDownItem parent)
            {
                dropDown = parent.DropDown;
                Debug.WriteLine(parent.Name);
            }
            else
            {
                break;
            }
            dropDown.AutoClose = autoClose;
        }
        Debug.WriteLine($"Set autoclose={autoClose}");
    }
}
