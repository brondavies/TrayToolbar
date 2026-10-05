using System.Runtime.InteropServices;

namespace TrayToolbar;

internal partial class NotificationsHelper
{
    private sealed class NotificationActivatorClassFactory(Type activatorType) : IClassFactory
    {
        private readonly Type _activatorType = activatorType;

        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            if (pUnkOuter != IntPtr.Zero)
            {
                return CLASS_E_NOAGGREGATION;
            }

            if (riid != typeof(INotificationActivationCallback).GUID && riid != IUnknownGuid)
            {
                return E_NOINTERFACE;
            }

            try
            {
                ppvObject = Marshal.GetComInterfaceForObject(Activator.CreateInstance(_activatorType)!, typeof(INotificationActivationCallback));
                return S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public int LockServer(bool fLock)
        {
            return S_OK;
        }
    }

    [ComVisible(true)]
    [Guid(ToastActivatorClsidValue)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class NotificationActivator : INotificationActivationCallback
    {
        public void Activate(string appUserModelId, string invokedArgs, NOTIFICATION_USER_INPUT_DATA[] data, uint dataCount)
        {
            HandleActivation(invokedArgs);
        }
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPersistFile
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
    private interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, out PROPERTYKEY pkey);
        void GetValue(ref PROPERTYKEY key, out PropVariant pv);
        void SetValue(ref PROPERTYKEY key, ref PropVariant pv);
        void Commit();
    }

    [ComImport]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IClassFactory
    {
        [PreserveSig]
        int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

        [PreserveSig]
        int LockServer(bool fLock);
    }

    [ComImport]
    [Guid("53E31837-6600-4A81-9395-75CFFE746F94")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface INotificationActivationCallback
    {
        void Activate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string invokedArgs,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] NOTIFICATION_USER_INPUT_DATA[] data,
            [In, MarshalAs(UnmanagedType.U4)] uint dataCount);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NOTIFICATION_USER_INPUT_DATA
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Key;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PROPERTYKEY(Guid fmtid, uint pid)
    {
        public Guid fmtid = fmtid;
        public uint pid = pid;
    }

    // Size matches the native 64-bit PROPVARIANT so GetValue can write the whole struct
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct PropVariant : IDisposable
    {
        [FieldOffset(0)]
        private ushort vt;

        [FieldOffset(8)]
        private IntPtr pointerValue;

        public readonly string? GetString()
        {
            return vt == (ushort)VarEnum.VT_LPWSTR ? Marshal.PtrToStringUni(pointerValue) : null;
        }

        public void Dispose()
        {
            PropVariantClear(ref this);
        }
    }

    [DllImport("ole32.dll")]
    private static extern int CoRegisterClassObject(
        ref Guid rclsid,
        [MarshalAs(UnmanagedType.Interface)] IClassFactory pUnk,
        uint dwClsContext,
        uint flags,
        out uint lpdwRegister);

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PropVariant pvar);

    [DllImport("combase.dll")]
    private static extern int RoInitialize(uint initType);

    [DllImport("combase.dll")]
    private static extern int RoActivateInstance(IntPtr activatableClassId, out IntPtr instance);

    [DllImport("combase.dll")]
    private static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    private static extern int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

    [DllImport("combase.dll")]
    private static extern int WindowsDeleteString(IntPtr hstring);

    private static readonly Guid IUnknownGuid = new("00000000-0000-0000-C000-000000000046");
    private const int S_OK = 0;
    private const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
    private const uint CLSCTX_LOCAL_SERVER = 0x4;
    private const uint REGCLS_MULTIPLEUSE = 0x1;
    private const uint STGM_READ = 0x00000000;
    private const uint RO_INIT_SINGLETHREADED = 0;
    private const uint RO_INIT_MULTITHREADED = 1;

    private struct HString(string value) : IDisposable
    {
        public IntPtr Handle { get; private set; } = Create(value);

        public void Dispose()
        {
            if (Handle == IntPtr.Zero)
            {
                return;
            }

            WindowsDeleteString(Handle);
            Handle = IntPtr.Zero;
        }

        private static IntPtr Create(string value)
        {
            var hr = WindowsCreateString(value, value.Length, out var hstring);
            Marshal.ThrowExceptionForHR(hr);
            return hstring;
        }
    }
}