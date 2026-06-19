namespace RimeDictManager;

using Avalonia;

public static class Program {
    [STAThread]
    public static void Main(string[] args) {
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
