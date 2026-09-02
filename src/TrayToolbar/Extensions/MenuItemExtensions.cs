using System.Diagnostics;

namespace TrayToolbar.Extensions;

public static class MenuItemExtensions
{
    /// <summary>
    /// Sets <see cref="ToolStripDropDown.AutoClose"/> on the menu and every ancestor dropdown
    /// and returns the dropdowns that were changed so callers can restore exactly the same set.
    /// </summary>
    public static List<ToolStripDropDown> SetAutoClose(this ToolStripDropDown menu, bool autoClose)
    {
        var affected = new List<ToolStripDropDown>();
        var dropDown = menu;
        dropDown.AutoClose = autoClose;
        affected.Add(dropDown);
        while (dropDown.OwnerItem != null && dropDown.OwnerItem is ToolStripDropDownItem owner)
        {
            //There's an extra level to get to the parent menu
            if (owner.OwnerItem != null && owner.OwnerItem is ToolStripDropDownItem parent)
            {
                dropDown = parent.DropDown;
            }
            else
            {
                break;
            }
            dropDown.AutoClose = autoClose;
            affected.Add(dropDown);
        }
        return affected;
    }
}