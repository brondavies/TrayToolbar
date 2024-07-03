using System.ComponentModel;
using TrayToolbar.Extensions;

namespace TrayToolbar.Controls
{
    public class CustomComboBox : ComboBox
    {
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == Windows.Win32.PInvoke.WM_PAINT)
            {
                if (Parent != null)
                {
                    using var g = Graphics.FromHwnd(Handle);
                    using var p = new Pen(Parent.BackColor, 1);
                    g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                    using var b = new Pen(BorderColor, 1);
                    g.DrawRectangle(b, 1, 1, Width - 3, Height - 3);
                    using var ba = new Pen(BackColor, 1);
                    g.DrawRectangle(ba, 2, 2, Width - 5, Height - 5);
                }
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "WindowFrame")]
        public Color BorderColor { get; set; } = SystemColors.WindowFrame;
    }
}
