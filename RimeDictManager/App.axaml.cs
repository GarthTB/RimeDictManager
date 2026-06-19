namespace RimeDictManager;

using Avalonia;
using Avalonia.Markup.Xaml;
using Views;
using Desktop = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

public sealed class App: Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        (ApplicationLifetime as Desktop)?.MainWindow = new MainWindow();
#if MACOS
        this.TryGetFeature<IActivatableLifetime>()?.Activated += static (_, e) => {
            if (e is ProtocolActivatedEventArgs { Kind: ActivationKind.OpenUri } args)
                UrlActivation.ParseUrl(args.Uri.ToString());
        };
#endif
        base.OnFrameworkInitializationCompleted();
    }
}
