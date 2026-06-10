namespace RimeDictManager;

using Avalonia;
using Avalonia.Markup.Xaml;
using Views;
using Desktop = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

public sealed class App: Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        (ApplicationLifetime as Desktop)?.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
