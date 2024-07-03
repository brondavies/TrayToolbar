using static Windows.Win32.PInvoke;

namespace TrayToolbar.Extensions
{
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

        public static int ScrollMargin = 24;

        private IntPtr activeHwnd = 0;
        private ToolStripDropDown? activeMenu;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE && activeHwnd != m.HWnd)
            {
                activeHwnd = m.HWnd;
                this.activeMenu = Control.FromHandle(m.HWnd) as ToolStripDropDown;
            }
            else if (m.Msg == WM_MOUSEWHEEL && this.activeMenu != null)
            {
                int delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
                HandleDelta(this.activeMenu, delta);
                return true;
            }
            return false;
        }

        private static readonly Action<ToolStrip, int> ScrollInternal
            = (Action<ToolStrip, int>)Delegate.CreateDelegate(typeof(Action<ToolStrip, int>),
                typeof(ToolStrip).GetMethod("ScrollInternal",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance)!);

        private void HandleDelta(ToolStripDropDown toolStripDropDown, int delta)
        {
            if (toolStripDropDown.Items.Count == 0)
                return;
            var firstItem = toolStripDropDown.Items[0];
            var lastItem = toolStripDropDown.Items[toolStripDropDown.Items.Count - 1];
            if (lastItem.Bounds.Bottom < toolStripDropDown.Height && firstItem.Bounds.Top > 0)
                return;
            delta = delta / -4;
            if (delta < 0 && firstItem.Bounds.Top - delta > ScrollMargin)
            {
                delta = firstItem.Bounds.Top - ScrollMargin;
            }
            else if (delta > 0 && delta > lastItem.Bounds.Bottom - toolStripDropDown.Height + ScrollMargin)
            {
                delta = lastItem.Bounds.Bottom - toolStripDropDown.Height + ScrollMargin;
            }
            if (delta != 0)
                ScrollInternal(toolStripDropDown, delta);
        }
    }
}
