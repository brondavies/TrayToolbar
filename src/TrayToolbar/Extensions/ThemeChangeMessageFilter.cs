namespace TrayToolbar.Extensions;

internal class ThemeChangeMessageFilter : IMessageFilter
{
    private static ThemeChangeMessageFilter? Instance;

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
        else if (Instance != null)
        {
            Application.RemoveMessageFilter(Instance);
            Instance = null;
        }
    }

    public static event EventHandler? ThemeChanged;
    private static DateTime eventTrigger = DateTime.Now;

    public bool PreFilterMessage(ref Message m)
    {
        //Debug.WriteLine($"{m.HWnd}: {m.WParam}, {m.LParam}, {m.Msg}");
        //TODO: I was expecting to be able to check for WM_SETTINGCHANGE but this doesn't actually consistently happen between Win 10/11
        //      Look for updated documentation on how to get a message when the theme changes
        if ((DateTime.Now - eventTrigger).TotalMilliseconds > 100)
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
            eventTrigger = DateTime.Now;
        }
        return false;
    }
}
