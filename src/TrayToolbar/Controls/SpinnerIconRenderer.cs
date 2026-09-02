using System.Drawing.Drawing2D;

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TrayToolbar.Controls;

/// <summary>
/// Renders tray icon frames with a spinner arc overlaid in the bottom-right corner
/// to show that the menu is still loading
/// </summary>
internal static class SpinnerIconRenderer
{
    internal const int FrameCount = 8;

    static readonly Color SpinnerColor = Color.FromArgb(0, 103, 192); // Windows accent blue
    static readonly Color SpinnerBackground = Color.FromArgb(220, Color.White);

    internal static Icon RenderFrame(Icon baseIcon, int frame)
    {
        var size = baseIcon.Size;
        using var bitmap = new Bitmap(size.Width, size.Height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawIcon(baseIcon, new Rectangle(Point.Empty, size));

            var diameter = Math.Max(size.Width / 2, 8);
            var badge = new Rectangle(size.Width - diameter, size.Height - diameter, diameter - 1, diameter - 1);
            using var background = new SolidBrush(SpinnerBackground);
            graphics.FillEllipse(background, badge);

            var thickness = Math.Max(diameter / 5f, 1.5f);
            var inset = thickness / 2f + 0.5f;
            var arc = new RectangleF(badge.X + inset, badge.Y + inset, badge.Width - 2 * inset, badge.Height - 2 * inset);
            using var pen = new Pen(SpinnerColor, thickness);
            graphics.DrawArc(pen, arc, frame * (360f / FrameCount), 270f);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var frameIcon = Icon.FromHandle(handle);
            return (Icon)frameIcon.Clone();
        }
        finally
        {
            PInvoke.DestroyIcon((HICON)handle);
        }
    }
}