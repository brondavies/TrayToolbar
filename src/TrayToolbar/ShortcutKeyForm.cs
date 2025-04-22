using TrayToolbar.Extensions;
using Windows.Win32;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

public partial class ShortcutKeyForm : Form
{
    public ShortcutKeyForm()
    {
        InitializeComponent();
        Text = R.Shortcut_Key;
        MessageLabel.Text = R.Press_the_keys_to_use_as_the_shortcut_key;
        OKButton.Text = R.OK;
        CancelButton.Text = R.Cancel;
        HandleCreated += SetTheme;
        if (Handle != 0)
        {
            SetTheme(null, EventArgs.Empty);
        }
    }

    public string Hotkey
    {
        get => HotkeyValue.Text;
        set => HotkeyValue.Text = value;
    }

    private void SetTheme(object? sender, EventArgs e)
    {
        var darkmode = SystemTheme.DarkModeEnabled == true;
        SystemTheme.UseImmersiveDarkMode(Handle, darkmode);
        SystemTheme.SetThemeColors(this, darkmode);
    }

    private void ShortcutKeyForm_KeyDown(object sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;
        e.Handled = true;
        SetText(keyCode: e.KeyCode,
            win: PInvoke.GetAsyncKeyState((int)Keys.LWin) < 0 || PInvoke.GetAsyncKeyState((int)Keys.RWin) < 0,
            ctrl: e.Modifiers.HasFlag(Keys.Control),
            alt: e.Modifiers.HasFlag(Keys.Alt),
            shift: e.Modifiers.HasFlag(Keys.Shift)
            );
    }

    static readonly Keys[] ExcludedKeys = [
        Keys.LWin, Keys.RWin,
        Keys.Control, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey,
        Keys.Alt, Keys.Menu, Keys.LMenu, Keys.RMenu,
        Keys.Shift, Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey,
        Keys.Escape, Keys.Back,
        Keys.NumLock, Keys.Scroll,
        Keys.Pause, Keys.Print, Keys.PrintScreen,
        Keys.Up, Keys.Down, Keys.Left, Keys.Right,
        Keys.Clear, Keys.OemClear,
    ];

    private void SetText(Keys keyCode, bool win, bool ctrl, bool alt, bool shift)
    {
        var text = "";
        var keyOK = false;

        if (win)
        {
            text = "\u229E";
        }
        if (ctrl)
        {
            text += " CTRL";
        }
        if (alt)
        {
            text += " ALT";
        }
        if (shift)
        {
            text += " SHIFT";
        }
        if (!ExcludedKeys.Contains(keyCode))
        {
            text += $" {keyCode}";
            keyOK = true;
        }
        OKButton.Enabled = keyOK;
        if (keyOK) OKButton.Focus();

        Hotkey = string.Join(" + ", text.Trim().Split(' '));
    }
}
