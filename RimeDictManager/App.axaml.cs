namespace RimeDictManager;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Views;

public sealed class App: Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
            = new MainWindow();

        // macOS专用
        this.TryGetFeature<IActivatableLifetime>()?.Activated += static (_, e) => {
            if (e is ProtocolActivatedEventArgs { Kind: ActivationKind.OpenUri } args)
                UrlActivation.ParseUrl(args.Uri.ToString());
        };

        base.OnFrameworkInitializationCompleted();
    }
}
