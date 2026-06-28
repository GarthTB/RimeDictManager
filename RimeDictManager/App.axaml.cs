namespace RimeDictManager;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Views;

public sealed class App: Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            MainWindow mainWindow = new();
            desktop.MainWindow = mainWindow;
#if MACOS
            if (this.TryGetFeature<IActivatableLifetime>() is {} lifetime)
                lifetime.Activated += async (_, e) => {
                    if (e is not ProtocolActivatedEventArgs { Kind: ActivationKind.OpenUri } args)
                        return;
                    UrlActivation.ParseUrl(args.Uri.OriginalString);
                    if (mainWindow.IsLoaded) await mainWindow.AutoLoadDir();
                };
#endif
        }
        base.OnFrameworkInitializationCompleted();
    }
}
