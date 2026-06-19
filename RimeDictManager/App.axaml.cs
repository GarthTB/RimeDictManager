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

            // macOS专用
            this.TryGetFeature<IActivatableLifetime>()?.Activated += (_, e) => {
                if (e is not ProtocolActivatedEventArgs { Kind: ActivationKind.OpenUri } args)
                    return;
                UrlActivation.ParseUrl(args.Uri.ToString());
                mainWindow.ActivateDictFromUrl();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
