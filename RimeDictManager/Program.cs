namespace RimeDictManager;

using Avalonia;

public static class Program {
    [STAThread]
    public static void Main(string[] args) {
        if (!UrlActivation.ParseArgs(args)) {
#if WINDOWS
            try {
                if (Environment.ProcessPath is not {} path) throw new();
                using var rimeDictKey
                    = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                        $@"Software\Classes\{Common.Meta.UrlScheme}");
                rimeDictKey.SetValue("", "URL:RIME Dictionary Protocol");
                rimeDictKey.SetValue("URL Protocol", "");
                using (var cmdKey = rimeDictKey.CreateSubKey(@"shell\open\command"))
                    cmdKey.SetValue("", $"\"{path}\" \"%1\"");
                Log.Info("写注册表成功，后续可使用 URL 冷启动");
            } catch (Exception) { Log.Err("写注册表失败，后续无法使用 URL 冷启动"); }
#endif
        }
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
