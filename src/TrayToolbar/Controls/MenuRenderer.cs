using TrayToolbar.Extensions;

namespace TrayToolbar.Controls
{
    internal class MenuRenderer() : ToolStripProfessionalRenderer(new MyColors())
    {
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = ThemeColors.Current.DefaultForeColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ThemeColors.Current.DefaultForeColor;
            base.OnRenderArrow(e);
        }

        private class MyColors() : ProfessionalColorTable()
        {
            public override Color MenuItemSelected => ThemeColors.Current.MenuItemSelectedColor;

            public override Color MenuItemSelectedGradientBegin => ThemeColors.Current.MenuItemSelectedColor;

            public override Color MenuItemSelectedGradientEnd => ThemeColors.Current.MenuItemSelectedColor;

            public override Color MenuStripGradientBegin => ThemeColors.Current.MenuItemBackColor;

            public override Color MenuStripGradientEnd => ThemeColors.Current.MenuItemBackColor;

            public override Color ImageMarginGradientBegin => ThemeColors.Current.MenuItemBackColor;

            public override Color ImageMarginGradientMiddle => ThemeColors.Current.MenuItemBackColor;

            public override Color ImageMarginGradientEnd => ThemeColors.Current.MenuItemBackColor;

            public override Color ToolStripDropDownBackground => ThemeColors.Current.MenuItemBackColor;
        }
    }
}
