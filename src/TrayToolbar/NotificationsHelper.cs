using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

using Microsoft.Win32;

using TrayToolbar.Extensions;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

[SupportedOSPlatform("windows10.0.10240.0")]
internal partial class NotificationsHelper
{
    public const string UPDATE_ACTION = "update";
    private const string AppUserModelId = "Brontech.TrayToolbar";
    private const string ToastActivatedLaunchArgument = "--toast-activated";
    private const string AppUserModelRegistryPath = @"Software\Classes\AppUserModelId\" + AppUserModelId;
    private const string ClsidRegistryPath = @"SOFTWARE\Classes\CLSID\{" + ToastActivatorClsidValue + @"}\LocalServer32";
    private const string ToastActivatorClsidValue = "3A0F4F2C-5B61-4A9D-9B59-7E5C83A4E9E2";
    private static readonly Guid ToastActivatorClsid = new(ToastActivatorClsidValue);
    private static readonly Guid XmlDocumentGuid = new("F7F3A506-1E87-42D6-BCFB-B8C809FA5494");
    private static readonly Guid XmlDocumentIoGuid = new("6CD0E74E-EE65-4489-9EBF-CA43E87BA637");
    private static readonly Guid ToastNotificationFactoryGuid = new("04124B20-82C6-4229-B109-FD9ED4662B53");
    private static readonly Guid ToastNotificationManagerStaticsGuid = new("50AC103F-D235-4598-BBEF-98FE4D1A3AD4");
    private static readonly Guid ToastNotifierGuid = new("75927B93-03F3-41EC-91D3-6E5BAC1B38E7");
    private static readonly PROPERTYKEY PKEY_AppUserModel_ID = new(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
    private static readonly PROPERTYKEY PKEY_AppUserModel_ToastActivatorCLSID = new(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 26);
    private static readonly object SyncLock = new();
    private static bool _isActivated;
    private static uint _registrationCookie;

    internal static void Notify(string description, string url, string actionText = "", string action = "")
    {
        try
        {
            Activate();

            var title = EscapeXml(R.TrayToolbar);
            var message = EscapeXml(description);
            var attribution = EscapeXml(url);
            var xml = $"<toast{BuildBodyActivationAttributes(url)}><visual><binding template='ToastGeneric'><text>{title}</text><text>{message}</text><text placement='attribution'>{attribution}</text></binding></visual>{BuildActionsXml(actionText, action)}</toast>";
            ShowToast(xml);
        }
        catch
        {
        }
    }

    internal static void Activate()
    {
        if (_isActivated)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_isActivated)
            {
                return;
            }

            try
            {
                RegisterAppUserModel();
                EnsureStartMenuShortcut();
                RegisterComServer();
                RegisterActivator();
                _isActivated = true;
            }
            catch
            {
            }
        }
    }

    internal static bool WasCurrentProcessToastActivated()
    {
        return Environment.GetCommandLineArgs().Contains(ToastActivatedLaunchArgument);
    }

    private static void RegisterAppUserModel()
    {
        using var key = Registry.CurrentUser.CreateSubKey(AppUserModelRegistryPath);
        key?.SetValue("DisplayName", R.TrayToolbar, RegistryValueKind.String);
        key?.SetValue("IconUri", ConfigHelper.ApplicationExe, RegistryValueKind.String);
        key?.SetValue("IconBackgroundColor", "FFDDDDDD", RegistryValueKind.String);
        key?.SetValue("CustomActivator", $"{{{ToastActivatorClsid}}}", RegistryValueKind.String);
    }

    private static void RegisterComServer()
    {
        using var key = Registry.CurrentUser.CreateSubKey(ClsidRegistryPath);
        key?.SetValue(null, $"\"{ConfigHelper.ApplicationExe}\" {ToastActivatedLaunchArgument}", RegistryValueKind.String);
    }

    private static void RegisterActivator()
    {
        if (_registrationCookie != 0)
        {
            return;
        }

        var clsid = ToastActivatorClsid;
        var classFactory = new NotificationActivatorClassFactory(typeof(NotificationActivator));
        var hr = CoRegisterClassObject(ref clsid, classFactory, CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, out _registrationCookie);
        Marshal.ThrowExceptionForHR(hr);
    }

    private static void EnsureStartMenuShortcut()
    {
        var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        if (!programsFolder.HasValue())
        {
            return;
        }

        Directory.CreateDirectory(programsFolder);
        var shortcutPath = Path.Combine(programsFolder, $"{R.TrayToolbar}.lnk");

        var shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"), throwOnError: true)!;
        var shellLink = Activator.CreateInstance(shellLinkType)!;

        try
        {
            var link = (IShellLinkW)shellLink;
            link.SetPath(ConfigHelper.ApplicationExe);
            link.SetWorkingDirectory(ConfigHelper.ApplicationRoot);
            link.SetDescription(R.TrayToolbar);
            link.SetIconLocation(ConfigHelper.ApplicationExe, 0);

            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(shortcutPath, true);
            persistFile.Load(shortcutPath, STGM_READWRITE);

            var propertyStore = (IPropertyStore)shellLink;
            var appUserModelKey = PKEY_AppUserModel_ID;
            var appId = PropVariant.FromString(AppUserModelId);
            try
            {
                propertyStore.SetValue(ref appUserModelKey, ref appId);
            }
            finally
            {
                appId.Dispose();
            }

            var toastActivatorKey = PKEY_AppUserModel_ToastActivatorCLSID;
            var clsid = PropVariant.FromGuid(ToastActivatorClsid);
            try
            {
                propertyStore.SetValue(ref toastActivatorKey, ref clsid);
            }
            finally
            {
                clsid.Dispose();
            }

            propertyStore.Commit();
            persistFile.Save(shortcutPath, true);
        }
        finally
        {
            if (Marshal.IsComObject(shellLink))
            {
                Marshal.FinalReleaseComObject(shellLink);
            }
        }
    }

    private static void HandleActivation(string arguments)
    {
        var parsedArguments = ParseArguments(arguments);
        if (parsedArguments.TryGetValue("action", out var action) && action.Is(UPDATE_ACTION))
        {
            ConfigHelper.UpdateToLatestVersion();
        }
    }

    private static Dictionary<string, string> ParseArguments(string arguments)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        if (!arguments.HasValue())
        {
            return result;
        }

        foreach (var pair in arguments.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator < 0)
            {
                result[Uri.UnescapeDataString(pair)] = string.Empty;
                continue;
            }

            var key = Uri.UnescapeDataString(pair[..separator]);
            var value = Uri.UnescapeDataString(pair[(separator + 1)..]);
            result[key] = value;
        }

        return result;
    }

    private static string BuildActionsXml(string actionText, string action)
    {
        if (!actionText.HasValue() || !action.HasValue())
        {
            return string.Empty;
        }

        var buttonText = EscapeXml(actionText);
        var arguments = EscapeXml($"action={Uri.EscapeDataString(action)}");
        return $"<actions><action content='{buttonText}' arguments='{arguments}' activationType='foreground'/></actions>";
    }

    private static string BuildBodyActivationAttributes(string url)
    {
        if (!Uri.TryCreate(url, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = true }, out var uri))
        {
            return string.Empty;
        }

        return $" activationType='protocol' launch='{EscapeXml(uri.ToString())}'";
    }

    private static string EscapeXml(string? value)
    {
        return SecurityElement.Escape(value) ?? string.Empty;
    }

    private static void ShowToast(string xml)
    {
        EnsureWindowsRuntimeInitialized();

        IntPtr xmlDocumentInspectable = IntPtr.Zero;
        IntPtr xmlDocument = IntPtr.Zero;
        IntPtr xmlDocumentIo = IntPtr.Zero;
        IntPtr toastNotification = IntPtr.Zero;
        IntPtr toastNotifier = IntPtr.Zero;

        try
        {
            xmlDocumentInspectable = ActivateRuntimeClass("Windows.Data.Xml.Dom.XmlDocument");
            xmlDocument = QueryInterface(xmlDocumentInspectable, XmlDocumentGuid);
            xmlDocumentIo = QueryInterface(xmlDocumentInspectable, XmlDocumentIoGuid);
            LoadXml(xmlDocumentIo, xml);

            toastNotification = CreateToastNotification(xmlDocument);
            toastNotifier = CreateToastNotifier(AppUserModelId);
            ShowToastNotification(toastNotifier, toastNotification);
        }
        finally
        {
            ReleaseIfNeeded(ref toastNotifier);
            ReleaseIfNeeded(ref toastNotification);
            ReleaseIfNeeded(ref xmlDocumentIo);
            ReleaseIfNeeded(ref xmlDocument);
            ReleaseIfNeeded(ref xmlDocumentInspectable);
        }
    }

    private static void EnsureWindowsRuntimeInitialized()
    {
        var initType = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA
            ? RO_INIT_SINGLETHREADED
            : RO_INIT_MULTITHREADED;

        var hr = RoInitialize(initType);
        if (hr >= 0 || hr == RPC_E_CHANGED_MODE)
        {
            return;
        }

        Marshal.ThrowExceptionForHR(hr);
    }

    private static IntPtr ActivateRuntimeClass(string runtimeClassName)
    {
        using var className = new HString(runtimeClassName);
        var hr = RoActivateInstance(className.Handle, out var instance);
        Marshal.ThrowExceptionForHR(hr);
        return instance;
    }

    private static IntPtr QueryInterface(IntPtr instance, Guid interfaceId)
    {
        var iid = interfaceId;
        var hr = Marshal.QueryInterface(instance, ref iid, out var queriedInterface);
        Marshal.ThrowExceptionForHR(hr);
        return queriedInterface;
    }

    private static IntPtr GetActivationFactory(string runtimeClassName, Guid interfaceId)
    {
        using var className = new HString(runtimeClassName);
        var iid = interfaceId;
        var hr = RoGetActivationFactory(className.Handle, ref iid, out var factory);
        Marshal.ThrowExceptionForHR(hr);
        return factory;
    }

    private static unsafe void LoadXml(IntPtr xmlDocumentIo, string xml)
    {
        using var xmlText = new HString(xml);
        var vtable = *(IntPtr**)xmlDocumentIo;
        var loadXml = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)vtable[6];
        Marshal.ThrowExceptionForHR(loadXml(xmlDocumentIo, xmlText.Handle));
    }

    private static unsafe IntPtr CreateToastNotification(IntPtr xmlDocument)
    {
        var factory = GetActivationFactory("Windows.UI.Notifications.ToastNotification", ToastNotificationFactoryGuid);

        try
        {
            var vtable = *(IntPtr**)factory;
            var createToastNotification = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr*, int>)vtable[6];
            IntPtr toastNotification = IntPtr.Zero;
            Marshal.ThrowExceptionForHR(createToastNotification(factory, xmlDocument, &toastNotification));
            return toastNotification;
        }
        finally
        {
            Marshal.Release(factory);
        }
    }

    private static unsafe IntPtr CreateToastNotifier(string appUserModelId)
    {
        var statics = GetActivationFactory("Windows.UI.Notifications.ToastNotificationManager", ToastNotificationManagerStaticsGuid);

        try
        {
            using var appId = new HString(appUserModelId);
            var vtable = *(IntPtr**)statics;
            var createToastNotifier = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr*, int>)vtable[7];
            IntPtr toastNotifier = IntPtr.Zero;
            Marshal.ThrowExceptionForHR(createToastNotifier(statics, appId.Handle, &toastNotifier));
            return toastNotifier;
        }
        finally
        {
            Marshal.Release(statics);
        }
    }

    private static unsafe void ShowToastNotification(IntPtr toastNotifier, IntPtr toastNotification)
    {
        var notifier = QueryInterface(toastNotifier, ToastNotifierGuid);

        try
        {
            var vtable = *(IntPtr**)notifier;
            var show = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)vtable[6];
            Marshal.ThrowExceptionForHR(show(notifier, toastNotification));
        }
        finally
        {
            Marshal.Release(notifier);
        }
    }

    private static void ReleaseIfNeeded(ref IntPtr instance)
    {
        if (instance == IntPtr.Zero)
        {
            return;
        }

        Marshal.Release(instance);
        instance = IntPtr.Zero;
    }
}