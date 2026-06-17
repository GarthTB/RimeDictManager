namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Services;
using ViewModels;
using ZLinq;

public sealed partial class MainWindow: Window {
    private readonly MainWindowVM _vm = new();
    private bool _closeConfirmed;

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

    protected override async void OnClosing(WindowClosingEventArgs e) {
        try {
            if (!_closeConfirmed
             && DictManager.AllDicts.AsValueEnumerable().Any(static x => x.Modified)) {
                e.Cancel = true;
                if (await MsgBox.Ask<bool>("有词库的变更未保存，是否丢弃？")) {
                    _closeConfirmed = true;
                    Close();
                }
            }
            base.OnClosing(e);
        } catch (Exception ex) { await ex.Alert("阻止退出", this); }
    }
}
