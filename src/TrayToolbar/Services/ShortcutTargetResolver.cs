using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using TrayToolbar.Extensions;

namespace TrayToolbar.Services;

internal static class ShortcutTargetResolver
{
    internal static bool TryCreateShortcutStartInfo(string shortcutPath, out ProcessStartInfo startInfo)
    {
        startInfo = new ProcessStartInfo();
        if (!TryReadFileShortcutLaunchInfo(shortcutPath, out var launchInfo))
        {
            return false;
        }

        if (!CanCreateDirectShortcutStartInfo(launchInfo))
        {
            startInfo = new ProcessStartInfo(shortcutPath)
            {
                UseShellExecute = true,
            };

            return true;
        }

        startInfo = new ProcessStartInfo(launchInfo.TargetPath)
        {
            UseShellExecute = true,
            WindowStyle = launchInfo.WindowStyle,
        };

        var canPassArguments = CanPassArgumentsDirectly(launchInfo.TargetPath);
        if (canPassArguments && launchInfo.Arguments.HasValue())
        {
            startInfo.Arguments = launchInfo.Arguments;
        }

        if (launchInfo.WorkingDirectory.HasValue())
        {
            startInfo.WorkingDirectory = launchInfo.WorkingDirectory;
        }

        if (launchInfo.RunAsUser)
        {
            startInfo.Verb = "runas";
        }

        return true;
    }

    internal static bool TryResolveFileShortcutTarget(string shortcutPath, out string targetPath, Func<string, string>? shortcutResolver = null)
    {
        targetPath = string.Empty;
        if (!shortcutPath.HasValue() || !shortcutPath.FileExtension().Is(".lnk"))
        {
            return false;
        }

        try
        {
            var resolvedPath = (shortcutResolver ?? (path => path.ResolveShortcutTarget()))(shortcutPath);
            if (!resolvedPath.HasValue() || resolvedPath.Is(shortcutPath))
            {
                return false;
            }

            targetPath = resolvedPath.ToLocalPath();
            return targetPath.HasValue();
        }
        catch
        {
            return false;
        }
    }

    internal static bool TryResolveInternetShortcutTarget(IFileSystem fileSystem, string shortcutPath, out string target)
    {
        target = string.Empty;
        if (!shortcutPath.HasValue() || !shortcutPath.FileExtension().Is(".url") || !fileSystem.FileExists(shortcutPath))
        {
            return false;
        }

        try
        {
            var contents = fileSystem.ReadAllText(shortcutPath);
            foreach (var rawLine in contents.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!rawLine.StartsWith("URL=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return TryNormalizeShortcutTarget(rawLine[4..], out target);
            }
        }
        catch
        {
        }

        return false;
    }

    static bool TryReadFileShortcutLaunchInfo(string shortcutPath, out FileShortcutLaunchInfo launchInfo)
    {
        launchInfo = default;
        if (!shortcutPath.HasValue() || !shortcutPath.FileExtension().Is(".lnk") || !File.Exists(shortcutPath))
        {
            return false;
        }

        object? shellLink = null;

        try
        {
            var shellLinkType = Type.GetTypeFromCLSID(ShellLinkClsid, throwOnError: true);
            shellLink = Activator.CreateInstance(shellLinkType!);
            if (shellLink == null)
            {
                return false;
            }

            var persistFile = (IPersistFile)shellLink;
            persistFile.Load(shortcutPath, StgmRead);

            var shellLinkReader = (IShellLinkW)shellLink;
            var targetPath = ReadShellLinkPath((buffer, capacity) => shellLinkReader.GetPath(buffer, capacity, IntPtr.Zero, SlgpRawPath));
            if (targetPath.Is(shortcutPath))
            {
                return false;
            }

            shellLinkReader.GetHotkey(out var hotKey);
            shellLinkReader.GetShowCmd(out var showCommand);
            var iconLocation = ReadShellLinkIconLocation(shellLinkReader, out var iconIndex);

            var dataList = (IShellLinkDataList)shellLink;
            dataList.GetFlags(out var flags);

            launchInfo = new FileShortcutLaunchInfo(
                shortcutPath,
                targetPath,
                ReadShellLinkString((buffer, capacity) => shellLinkReader.GetArguments(buffer, capacity)),
                ReadShellLinkPath((buffer, capacity) => shellLinkReader.GetWorkingDirectory(buffer, capacity)),
                ReadShellLinkString((buffer, capacity) => shellLinkReader.GetDescription(buffer, capacity)),
                FormatHotKey(hotKey),
                FormatIconLocation(iconLocation, iconIndex),
                string.Empty,
                MapWindowStyle(showCommand),
                (flags & SldfRunAsUser) != 0);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            ReleaseComObject(shellLink);
        }
    }

    static bool CanCreateDirectShortcutStartInfo(FileShortcutLaunchInfo launchInfo)
    {
        if (!launchInfo.TargetPath.HasValue()
            || launchInfo.TargetPath.Is(launchInfo.ShortcutPath)
            || (!File.Exists(launchInfo.TargetPath) && !Directory.Exists(launchInfo.TargetPath)))
        {
            return false;
        }

        return !launchInfo.Arguments.HasValue() || CanPassArgumentsDirectly(launchInfo.TargetPath);
    }

    static bool CanPassArgumentsDirectly(string targetPath)
    {
        return targetPath.FileExtension().IsOneOf(".exe", ".com", ".bat", ".cmd");
    }

    static string ReadShellLinkPath(Action<StringBuilder, int> readValue)
    {
        return ReadShellLinkString(readValue).ToLocalPath();
    }

    static string ReadShellLinkString(Action<StringBuilder, int> readValue)
    {
        var buffer = new StringBuilder(MaxShellLinkStringLength);

        try
        {
            readValue(buffer, buffer.Capacity);
            return buffer.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    static string ReadShellLinkIconLocation(IShellLinkW shellLink, out int iconIndex)
    {
        var buffer = new StringBuilder(MaxShellLinkStringLength);
        iconIndex = 0;

        try
        {
            shellLink.GetIconLocation(buffer, buffer.Capacity, out iconIndex);
            return buffer.ToString();
        }
        catch
        {
            iconIndex = 0;
            return string.Empty;
        }
    }

    static ProcessWindowStyle MapWindowStyle(int showCommand)
    {
        return showCommand switch
        {
            0 => ProcessWindowStyle.Hidden,
            3 => ProcessWindowStyle.Maximized,
            7 => ProcessWindowStyle.Minimized,
            _ => ProcessWindowStyle.Normal,
        };
    }

    static string FormatHotKey(short hotKey)
    {
        return hotKey == 0 ? string.Empty : hotKey.ToString();
    }

    static string FormatIconLocation(string iconLocation, int iconIndex)
    {
        if (!iconLocation.HasValue())
        {
            return string.Empty;
        }

        return $"{iconLocation},{iconIndex}";
    }

    static void ReleaseComObject(object? comObject)
    {
        if (comObject != null && Marshal.IsComObject(comObject))
        {
            Marshal.FinalReleaseComObject(comObject);
        }
    }

    static bool TryNormalizeShortcutTarget(string rawTarget, out string target)
    {
        target = string.Empty;
        var candidate = Environment.ExpandEnvironmentVariables(rawTarget.Trim());
        if (!candidate.HasValue())
        {
            return false;
        }

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            target = uri.IsFile
                ? uri.LocalPath
                : uri.AbsoluteUri;
            return target.HasValue();
        }

        if (!Path.IsPathFullyQualified(candidate))
        {
            return false;
        }

        target = Path.GetFullPath(candidate);
        return true;
    }

    readonly record struct FileShortcutLaunchInfo(
        string ShortcutPath,
        string TargetPath,
        string Arguments,
        string WorkingDirectory,
        string Description,
        string HotKey,
        string IconLocation,
        string RelativePath,
        ProcessWindowStyle WindowStyle,
        bool RunAsUser);

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
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
    [Guid("45E2B4AE-B1C3-11D0-B92F-00A0C90312E1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkDataList
    {
        void AddDataBlock(IntPtr pDataBlock);
        void CopyDataBlock(uint dwSig, out IntPtr ppDataBlock);
        void RemoveDataBlock(uint dwSig);
        void GetFlags(out uint pdwFlags);
        void SetFlags(uint dwFlags);
    }

    static readonly Guid ShellLinkClsid = new("00021401-0000-0000-C000-000000000046");
    const int MaxShellLinkStringLength = 1024;
    const uint SlgpRawPath = 0x00000004;
    const uint StgmRead = 0;
    const uint SldfRunAsUser = 0x00002000;
}