using static Windows.Win32.PInvoke;

namespace TrayToolbar.Extensions;

//See https://stackoverflow.com/a/27390000/396005
public class DropDownMenuScrollWheelHandler : IMessageFilter
{
    private static DropDownMenuScrollWheelHandler? Instance;
    public static void Enable(bool enabled)
    {
        if (enabled)
        {
            if (Instance == null)
            {
                Instance = new DropDownMenuScrollWheelHandler();
                Application.AddMessageFilter(Instance);
            }
        }
        else
        {
            if (Instance != null)
            {
                Application.RemoveMessageFilter(Instance);
                Instance = null;
            }
        }
    }

    private const int WheelDelta = 120;

    private IntPtr activeHwnd = 0;
    private ToolStripDropDown? activeMenu;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == WM_MOUSEMOVE && activeHwnd != m.HWnd)
        {
            activeHwnd = m.HWnd;
            this.activeMenu = FindDropDown(m.HWnd);
        }
        else if (m.Msg == WM_MOUSEWHEEL && this.activeMenu != null)
        {
            int delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
            HandleDelta(this.activeMenu, delta);
            return true;
        }
        return false;
    }

    // The scroll buttons are child windows, so the pointer over them reports the button's handle
    private static ToolStripDropDown? FindDropDown(IntPtr hwnd)
    {
        var control = Control.FromHandle(hwnd);
        return control as ToolStripDropDown ?? control?.Parent as ToolStripDropDown;
    }

    private static readonly Action<ToolStrip, int> ScrollInternal
        = (Action<ToolStrip, int>)Delegate.CreateDelegate(typeof(Action<ToolStrip, int>),
            typeof(ToolStrip).GetMethod("ScrollInternal",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance)!);

    // Keeps the up/down scroll buttons enabled only while there is more to scroll
    internal static readonly System.Reflection.MethodInfo? UpdateScrollButtonStatus
        = typeof(ToolStripDropDownMenu).GetMethod("UpdateScrollButtonStatus",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance,
            Type.EmptyTypes);

    private static void HandleDelta(ToolStripDropDown toolStripDropDown, int wheelDelta)
    {
        var itemCount = toolStripDropDown.Items.Count;
        if (itemCount == 0)
            return;
        var firstItem = toolStripDropDown.Items[0];
        var lastItem = toolStripDropDown.Items[itemCount - 1];
        var view = toolStripDropDown.DisplayRectangle;
        var lineHeight = (lastItem.Bounds.Bottom - firstItem.Bounds.Top) / itemCount;
        var delta = GetWheelScrollDelta(wheelDelta, SystemInformation.MouseWheelScrollLines, lineHeight, view.Height);
        delta = ClampScrollDelta(delta, firstItem.Bounds.Top, lastItem.Bounds.Bottom, view.Top, view.Bottom);
        Scroll(toolStripDropDown, delta);
    }

    private static void Scroll(ToolStripDropDown toolStripDropDown, int delta)
    {
        if (delta == 0)
            return;
        ScrollInternal(toolStripDropDown, delta);
        if (toolStripDropDown is ToolStripDropDownMenu)
            UpdateScrollButtonStatus?.Invoke(toolStripDropDown, null);
    }

    /// <summary>
    /// Pixels to scroll for a wheel movement: Windows' "lines to scroll" setting times the item
    /// height per notch, or one page when the setting is "one screen at a time"
    /// </summary>
    internal static int GetWheelScrollDelta(int wheelDelta, int scrollLines, int lineHeight, int pageHeight)
    {
        var pixelsPerNotch = scrollLines < 0 ? pageHeight : scrollLines * lineHeight;
        return -wheelDelta * pixelsPerNotch / WheelDelta;
    }

    /// <summary>
    /// Limits a scroll so the first item never moves below the top of the view
    /// and the last item never moves above the bottom of it
    /// </summary>
    internal static int ClampScrollDelta(int delta, int firstItemTop, int lastItemBottom, int viewTop, int viewBottom)
    {
        if (delta < 0)
            return Math.Max(delta, Math.Min(0, firstItemTop - viewTop));
        if (delta > 0)
            return Math.Min(delta, Math.Max(0, lastItemBottom - viewBottom));
        return 0;
    }
}