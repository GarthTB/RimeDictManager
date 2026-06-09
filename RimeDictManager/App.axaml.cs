namespace RimeDictManager;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ViewModels;
using Views;

public sealed class App: Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
            = new MainWindow { DataContext = new MainWindowVM() };
        base.OnFrameworkInitializationCompleted();
    }
}
