namespace RimeDictManager;

using Avalonia;

public static class Program {
    [STAThread]
    public static void Main(string[] args) {
#if WINDOWS
        // Windows写注册表
        using var rimeDictKey
            = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\rime-dict");
        rimeDictKey.SetValue("", "URL:RIME Dictionary Protocol");
        rimeDictKey.SetValue("URL Protocol", "");

        var exePath = Environment.ProcessPath
                   ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (exePath is {}) {
            using var commandKey = rimeDictKey.CreateSubKey(@"shell\open\command");
            commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
        }
#endif
        UrlActivation.ParseArgs(args);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary> 设计器专用 </summary>
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
