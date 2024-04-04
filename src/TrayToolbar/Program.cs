using System.Reflection;

namespace TrayToolbar
{
    internal static class Program
    {
        internal static readonly string ApplicationRoot = 
            new FileInfo(
                    Assembly.GetExecutingAssembly().Location
                ).DirectoryName!;
        internal static string ConfigurationFile = Path.Combine(ApplicationRoot, "TrayToolbar.json");

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new SettingsForm());
        }
    }
}