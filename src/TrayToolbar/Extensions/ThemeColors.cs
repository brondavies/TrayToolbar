namespace TrayToolbar.Extensions
{
    internal class ThemeColors
    {
        public static IThemeColors Current { get; set; } = new LightTheme();
        public static IThemeColors Dark { get; } = new DarkTheme();
        public static IThemeColors Light { get; } = new LightTheme();
    }

    internal class DarkTheme : IThemeColors
    {
        public Color DefaultBackColor => Color.FromArgb(0x20, 0x20, 0x20);

        public Color DefaultForeColor => Color.FromArgb(0xEE, 0xEE, 0xEE);

        public Color ComboBorderColor => SystemColors.WindowFrame;

        public Color ControlBackColor => Color.FromArgb(0x30, 0x30, 0x30);

        public BorderStyle TextBoxBorderStyle => BorderStyle.FixedSingle;

        public FlatStyle ButtonFlatStyle => FlatStyle.Flat;

        public Color ButtonBorderColor => Color.FromArgb(0xEE, 0xEE, 0xEE);

        public Color LinkColor => Color.FromArgb(0x88, 0x88, 0xFF);
    
        public Color MenuItemBackColor => Color.FromArgb(0x20, 0x20, 0x20);

        public Color MenuStripBackColor => Color.FromArgb(0x20, 0x20, 0x20);

        public Color MenuItemSelectedColor => Color.FromArgb(0x20, 0x20, 0x40);
    }

    internal class LightTheme : IThemeColors
    {
        public Color DefaultBackColor => SystemColors.Control;

        public Color DefaultForeColor => SystemColors.ControlText;

        public Color ComboBorderColor => SystemColors.Window;

        public Color ControlBackColor => SystemColors.Window;

        public BorderStyle TextBoxBorderStyle => BorderStyle.Fixed3D;

        public FlatStyle ButtonFlatStyle => FlatStyle.Standard;

        public Color ButtonBorderColor => SystemColors.ControlDark;

        public Color LinkColor => Color.FromArgb(0, 0, 0xFF);

        public Color MenuItemBackColor => SystemColors.Window;

        public Color MenuStripBackColor => SystemColors.Window;

        public Color MenuItemSelectedColor => SystemColors.MenuHighlight;
    }

    internal interface IThemeColors
    {
        Color DefaultBackColor { get; }
        Color DefaultForeColor { get; }
        Color ComboBorderColor { get; }
        Color ControlBackColor { get; }
        BorderStyle TextBoxBorderStyle { get; }
        FlatStyle ButtonFlatStyle { get; }
        Color ButtonBorderColor { get; }
        Color LinkColor { get; }
        Color MenuItemBackColor { get; }
        Color MenuStripBackColor { get; }
        Color MenuItemSelectedColor { get; }
    }
}
