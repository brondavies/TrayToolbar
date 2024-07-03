using System.Diagnostics;

namespace TrayToolbar.Extensions
{
    internal class ThemeChangeMessageFilter : IMessageFilter
    {
        public static ThemeChangeMessageFilter? Instance;

        public static void Enable(bool enable)
        {
            if (enable)
            {
                if (Instance == null)
                {
                    Instance = new ThemeChangeMessageFilter();
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

        public static event EventHandler? ThemeChanged;
        private static DateTime eventTrigger = DateTime.Now;

        public bool PreFilterMessage(ref Message m)
        {
            //Debug.WriteLine($"{m.HWnd}: {m.WParam}, {m.LParam}, {m.Msg}");
            if (m.WParam == 0 && m.LParam == 0 && m.Msg == 49832 //not in the WM_* enum
                && (DateTime.Now - eventTrigger).TotalMilliseconds > 100)
            {
                ThemeChanged?.Invoke(null, EventArgs.Empty);
                eventTrigger = DateTime.Now;
            }
            return false;
        }
    }
}
