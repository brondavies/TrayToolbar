using TrayToolbar.Extensions;
using TrayToolbar.Models;
using Windows.Win32;
using Windows.Win32.Foundation;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

public partial class FolderControl : UserControl
{
    private FolderConfig config = new();

    public event EventHandler? BrowseFolder;
    public event EventHandler? DeleteFolder;

    public FolderControl()
    {
        InitializeComponent();
        UpdateConfig();
    }

    public bool ShowRemoveButton
    {
        get => DeleteFolderButton.Visible;
        set => DeleteFolderButton.Visible = value;
    }

    public FolderConfig Config
    {
        get => config;
        set
        {
            config = value;
            UpdateConfig();
        }
    }

    public bool Error
    {
        get => ErrorIcon.Visible;
        set
        {
            ErrorIcon.BackColor = FolderComboBox.BackColor;
            ErrorIcon.Visible = value;
        }
    }

    public void OnThemeChanged(bool _)
    {
        ErrorIcon.BackColor = FolderComboBox.BackColor;
    }

    public void UpdateConfig()
    {
        if (InvokeRequired)
        {
            Invoke(UpdateConfig);
            return;
        }
        RecursiveCheckbox.Text = R.Include_Subfolders;
        HotkeyLabel.Text = R.Shortcut_Key;
        toolTips.SetToolTip(BrowseFolderButton, R.Browse_Folder);
        toolTips.SetToolTip(DeleteFolderButton, R.Remove_Folder);
        toolTips.SetToolTip(SelectIconButton, R.Select_icon);
        toolTips.SetToolTip(ClearIconButton, R.Clear);
        Image? folderImage = null;
        if (config != null)
        {
            if (FolderComboBox != null) FolderComboBox.Text = config.Name;
            if (RecursiveCheckbox != null) RecursiveCheckbox.Checked = config.Recursive;
            if (HotkeyValue != null)
            {
                if (config.Hotkey.HasValue())
                {
                    HotkeyValue.Text = config.Hotkey;
                }
                else
                {
                    HotkeyValue.Text = "";
                }
            }
            if (SetHotKey != null)
            {
                SetHotKey.Text = config.Hotkey.HasValue() ? R.Clear : R.Set;
            }
            folderImage = config.GetIcon();
            ClearIconButton.Visible = config.Icon.HasValue();
        }
        SelectIconButton.BackgroundImage = folderImage ?? GetDefaultIcon();
    }

    private static Bitmap GetDefaultIcon() => SystemIcons.GetStockIcon(StockIconId.Folder, StockIconOptions.ShellIconSize).ToBitmap();

    private void BrowseFolderButton_Click(object sender, EventArgs e)
    {
        BrowseFolder?.Invoke(this, EventArgs.Empty);
    }

    private void DeleteFolderButton_Click(object sender, EventArgs e)
    {
        DeleteFolder?.Invoke(this, EventArgs.Empty);
    }

    private void FolderComboBox_TextUpdate(object sender, EventArgs e)
    {
        Config.Name = FolderComboBox.Text;
    }

    private void RecursiveCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        Config.Recursive = RecursiveCheckbox.Checked;
    }

    private void SetHotKey_Click(object sender, EventArgs e)
    {
        if (Config.Hotkey.HasValue())
        {
            Config.Hotkey = null;
        }
        else
        {
            var dialog = new ShortcutKeyForm
            {
                Hotkey = Config?.Hotkey ?? ""
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                config.Hotkey = dialog.Hotkey;
            }
        }
        UpdateConfig();
    }

    private void SelectIconButton_Click(object sender, EventArgs e)
    {
        var hwnd = new HWND(ParentForm!.Handle);
        var path = config.Icon?.ToLocalPath() ?? config.Name?.ToLocalPath() ?? ShellIcons.Shell32Dll;
        if (!File.Exists(path))
        {
            path = ShellIcons.Shell32Dll; // fallback to shell32.dll if the path is invalid  
        }

        uint capacity = 512;
        Span<char> iconPathBuffer = stackalloc char[(int)capacity];
        path.AsSpan().CopyTo(iconPathBuffer);

        int index = 0;
        int result = 0;
        unsafe
        {
            result = PInvoke.PickIconDlg(hwnd, ref iconPathBuffer, capacity, &index);
        }
        if (result != 0)
        {
            config.Icon = iconPathBuffer.ToString();
            config.IconIndex = index;
            UpdateConfig();
        }
    }

    private void ClearIconButton_Click(object sender, EventArgs e)
    {
        config.Icon = null;
        config.IconIndex = 0;
        UpdateConfig();
    }
}
