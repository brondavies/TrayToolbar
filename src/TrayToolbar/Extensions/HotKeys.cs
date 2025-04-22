using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace TrayToolbar.Extensions;

internal class HotKeys : IMessageFilter
{
    private const int BASE_ID = 1000;

    public static HotKeys? Instance;

    public static event HotKeyPressedHandler? HotKeyPressed;
    public delegate void HotKeyPressedHandler(int value, EventArgs e);

    private static readonly List<int> RegisteredHotKeys = [];

    public static void Enable(bool enable)
    {
        if (enable)
        {
            if (Instance == null)
            {
                Instance = new HotKeys();
                Application.AddMessageFilter(Instance);
            }
        }
        else if (Instance != null)
        {
            Application.RemoveMessageFilter(Instance);
            Instance = null;
        }
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == PInvoke.WM_HOTKEY)
        {
            var id = m.WParam.ToInt32();
            if (id >= BASE_ID)
            {
                HotKeyPressed?.Invoke(id - BASE_ID, EventArgs.Empty);
            }
        }
        return false;
    }

    public static void Register(int id, string hotkey)
    {
        var keys = Keys.None;
        var modifiers = HOT_KEY_MODIFIERS.MOD_NOREPEAT;
        var k = hotkey.Split('+').Select(i => i.Trim()).ToArray();
        foreach (var key in k)
        {
            if (key == "\u229E")
            {
                modifiers |= HOT_KEY_MODIFIERS.MOD_WIN;
            }
            else if (key == "CTRL")
            {
                modifiers |= HOT_KEY_MODIFIERS.MOD_CONTROL;
            }
            else if (key == "ALT")
            {
                modifiers |= HOT_KEY_MODIFIERS.MOD_ALT;
            }
            else if (key == "SHIFT")
            {
                modifiers |= HOT_KEY_MODIFIERS.MOD_SHIFT;
            }
            else if (Enum.TryParse(key, true, out Keys parsedKey))
            {
                keys |= parsedKey;
            }
        }
        Register(id, modifiers, keys);
    }

    public static void Register(int id, HOT_KEY_MODIFIERS modifiers, Keys key)
    {
        var hwnd = new HWND(0);
        PInvoke.RegisterHotKey(hwnd, BASE_ID + id,
            modifiers,
            (uint)key);
        RegisteredHotKeys.Add(id);
    }

    public static void Unregister(int id)
    {
        var hwnd = new HWND(0);
        PInvoke.UnregisterHotKey(hwnd, BASE_ID + id);
    }

    public static void UnregisterAll()
    {
        var hwnd = new HWND(0);
        RegisteredHotKeys.ForEach(i =>
        {
            PInvoke.UnregisterHotKey(hwnd, BASE_ID + i);
        });
    }
}
