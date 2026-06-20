namespace RimeDictManager;

using Avalonia;

public static class Program {
    [STAThread]
    public static void Main(string[] args) {
#if WINDOWS
        try {
            using var rimeDictKey
                = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\rime-dict");
            rimeDictKey.SetValue("", "URL:RIME Dictionary Protocol");
            rimeDictKey.SetValue("URL Protocol", "");
            if (Environment.ProcessPath is {} path) {
                using var cmdKey = rimeDictKey.CreateSubKey(@"shell\open\command");
                cmdKey.SetValue("", $"\"{path}\" \"%1\"");
            }
        } catch (Exception) { Log.Err("写注册表失败，后续无法使用URL冷启动"); }
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
