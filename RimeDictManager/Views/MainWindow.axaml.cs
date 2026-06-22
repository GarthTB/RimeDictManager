namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using Services;
using ViewModels;
using ZLinq;

public sealed partial class MainWindow: Window {
    private readonly MainWindowVM _vm = new();
    private bool _closeConfirmed;

    public MainWindow() {
        InitializeComponent();
        DataContext = _vm;
        Title = Meta.Version is {} v
            ? $"{Meta.Name} - {v}"
            : Meta.Name;
        Loaded += async (_, _) => {
            if (UrlActivation.ConsumeDir() is {} dir) await ShowDictWindow(dir);
        };
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void ShowDictWindow(object? _, RoutedEventArgs e) => await ShowDictWindow(null);

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task ShowDictWindow(string? dir) {
        try {
            await new DictWindow(dir).ShowDialog(this);
            _vm.RefreshState();
        } catch (Exception ex) { await ex.Alert("管理词库并刷新状态", this); }
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
