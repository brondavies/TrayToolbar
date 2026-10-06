using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TrayToolbar.Tests;

[TestClass]
[SupportedOSPlatform("windows10.0.10240.0")]
public class NotificationsHelperTests
{
    [TestMethod]
    public void RemoveStartMenuShortcut_deletes_the_shortcut_created_by_earlier_versions()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-NotificationTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "TrayToolbar.lnk");

        try
        {
            CreateShortcut(shortcutPath, "Brontech.TrayToolbar");

            NotificationsHelper.RemoveStartMenuShortcut(shortcutPath);

            Assert.IsFalse(File.Exists(shortcutPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("Contoso.OtherApp")]
    public void RemoveStartMenuShortcut_keeps_shortcuts_without_the_TrayToolbar_AppUserModelId(string? appUserModelId)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-NotificationTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "TrayToolbar.lnk");

        try
        {
            CreateShortcut(shortcutPath, appUserModelId);

            NotificationsHelper.RemoveStartMenuShortcut(shortcutPath);

            Assert.IsTrue(File.Exists(shortcutPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    static void CreateShortcut(string shortcutPath, string? appUserModelId)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        Assert.IsNotNull(shellType);

        dynamic? shell = Activator.CreateInstance(shellType!);
        Assert.IsNotNull(shell);
        dynamic nonNullShell = shell!;

        dynamic? shortcut = nonNullShell.CreateShortcut(shortcutPath);
        Assert.IsNotNull(shortcut);
        dynamic nonNullShortcut = shortcut!;
        nonNullShortcut.TargetPath = Path.Combine(Environment.SystemDirectory, "notepad.exe");
        nonNullShortcut.Save();

        if (appUserModelId != null)
        {
            SetShortcutAppUserModelId(shortcutPath, appUserModelId);
        }
    }

    static void SetShortcutAppUserModelId(string shortcutPath, string appUserModelId)
    {
        object? shellLink = null;
        var propVariant = IntPtr.Zero;
        var value = IntPtr.Zero;

        try
        {
            var shellLinkType = Type.GetTypeFromCLSID(ShellLinkClsid, throwOnError: true);
            shellLink = Activator.CreateInstance(shellLinkType!);
            Assert.IsNotNull(shellLink);

            var persistFile = (IPersistFile)shellLink!;
            persistFile.Load(shortcutPath, StgmReadWrite);

            // PROPVARIANT: vt at offset 0, string pointer at offset 8
            propVariant = Marshal.AllocHGlobal(PropVariantSize);
            for (var offset = 0; offset < PropVariantSize; offset += 4)
            {
                Marshal.WriteInt32(propVariant, offset, 0);
            }
            value = Marshal.StringToCoTaskMemUni(appUserModelId);
            Marshal.WriteInt16(propVariant, 0, (short)VarEnum.VT_LPWSTR);
            Marshal.WriteIntPtr(propVariant, 8, value);

            var propertyStore = (IPropertyStore)shellLink!;
            var key = AppUserModelIdKey;
            propertyStore.SetValue(ref key, propVariant);
            propertyStore.Commit();

            persistFile.Save(shortcutPath, true);
        }
        finally
        {
            if (value != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(value);
            }
            if (propVariant != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(propVariant);
            }
            if (shellLink != null && Marshal.IsComObject(shellLink))
            {
                Marshal.FinalReleaseComObject(shellLink);
            }
        }
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [ComImport]
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, out PropertyKey pkey);
        void GetValue(ref PropertyKey key, IntPtr pv);
        void SetValue(ref PropertyKey key, IntPtr pv);
        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct PropertyKey(Guid fmtid, uint pid)
    {
        public Guid fmtid = fmtid;
        public uint pid = pid;
    }

    static readonly Guid ShellLinkClsid = new("00021401-0000-0000-C000-000000000046");
    static readonly PropertyKey AppUserModelIdKey = new(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
    const uint StgmReadWrite = 0x00000002;
    const int PropVariantSize = 24;
}
