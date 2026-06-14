namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Services;
using ViewModels;

public sealed partial class MainWindow: Window {
    private readonly MainWindowVM _vm = new();

    public MainWindow() {
        InitializeComponent();
        DataContext = _vm;
    }

    private async void ShowDictWindow(object? _, RoutedEventArgs e) {
        try { await new DictWindow().ShowDialog(this); } catch (Exception ex) {
            await ex.Alert("管理词库", this);
        } finally { _vm.RefreshState(); }
    }

    private async void ShowLogWindow(object? _, RoutedEventArgs e) {
        try { await new LogWindow().ShowDialog(this); } catch (Exception ex) {
            await ex.Alert("查看日志", this);
        }
    }
}
