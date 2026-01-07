using Microsoft.Toolkit.Uwp.Notifications;
using TrayToolbar.Extensions;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

internal class NotificationsHelper
{
    public const string UPDATE_ACTION = "update";

    internal static void Notify(string description, string url, string actionText = "", string action = "")
    {
        string title = R.TrayToolbar;

        var toast = new ToastContentBuilder()
            .AddText(title)
            .AddText(description)
            .AddAttributionText(url);
        if (actionText.HasValue() && action.HasValue())
        {
            toast = toast.AddButton(actionText, ToastActivationType.Foreground, action);
        }
        if (Uri.TryCreate(url, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = true }, out var uri))
        {
            toast.SetProtocolActivation(uri);
        }
        toast.Show();
    }

    internal static void Activate()
    {
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var args = ToastArguments.Parse(toastArgs.Argument);

            if (args.Contains(UPDATE_ACTION))
            {
                ConfigHelper.UpdateToLatestVersion();
            }
        };
    }
}