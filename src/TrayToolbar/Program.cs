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
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 0 && args[0] == "--runonlogin")
            {
                ConfigHelper.SetStartupKey(args.Length > 1);
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new SettingsForm());
        }
    }
}