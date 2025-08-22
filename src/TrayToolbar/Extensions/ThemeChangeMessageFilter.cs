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
        if ((DateTime.Now - eventTrigger).TotalMilliseconds > 100)
        {
            TriggerThemeChanged();
        }
        return false;
    }

    internal static void TriggerThemeChanged()
    {
        ThemeChanged?.Invoke(null, EventArgs.Empty);
        eventTrigger = DateTime.Now;
    }
}
