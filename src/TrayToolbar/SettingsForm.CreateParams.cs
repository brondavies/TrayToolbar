using Windows.Win32.UI.WindowsAndMessaging;

namespace TrayToolbar
{
    partial class SettingsForm
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= (int)WINDOW_EX_STYLE.WS_EX_COMPOSITED;
                return handleParam;
            }
        }
    }
}
