using Microsoft.Toolkit.Uwp.Notifications;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

internal class NotificationsHelper
{
    internal static void Notify(string description, string url)
    {
        string title = R.TrayToolbar;
        
        var toast = new ToastContentBuilder()
            .AddText(title)
            .AddText(description)
            .AddAttributionText(url);
        if (Uri.TryCreate(url, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = true }, out var uri))
        {
            toast.SetProtocolActivation(uri);
        }
        toast.Show();
    }
}