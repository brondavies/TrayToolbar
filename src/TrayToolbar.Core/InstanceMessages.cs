using System.Runtime.InteropServices;

namespace TrayToolbar;

/// <summary>
/// Window messages broadcast between TrayToolbar processes: a second instance asks the
/// running one to show Settings, and a staged updater asks it to exit
/// </summary>
internal static partial class InstanceMessages
{
    const nint HWND_BROADCAST = 0xFFFF;

    internal static readonly uint WM_EXITSETTINGSFORM = RegisterWindowMessage("TrayToolbar.ExitSettingsForm.241ba8ec-76fa-4b62-91ff-2f5d060f5db7");
    internal static readonly uint WM_SHOWSETTINGSFORM = RegisterWindowMessage("TrayToolbar.ShowSettingsForm.729a0e10-3131-4e69-ba45-23660c5a91bf");

    internal static void NotifyExistingInstance()
    {
        PostMessage(HWND_BROADCAST, WM_SHOWSETTINGSFORM, 0, 0);
    }

    internal static void RequestExit()
    {
        PostMessage(HWND_BROADCAST, WM_EXITSETTINGSFORM, 0, 0);
    }

    [LibraryImport("user32.dll", EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint RegisterWindowMessage(string message);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(nint hWnd, uint msg, nuint wParam, nint lParam);
}
