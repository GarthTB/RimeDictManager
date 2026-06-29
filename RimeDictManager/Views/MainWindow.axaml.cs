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
        Title = $"{Meta.Name} - {Meta.Version}";
        Loaded += async (_, _) => await AutoLoadDir();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task AutoLoadDir() {
        try {
            var dir = UrlActivation.ConsumeDir();
            if (dir is null) return;
            if (!Directory.Exists(dir)) throw new InvalidOperationException("指定的目录不存在");
            await new DictWindow(dir).ShowDialog(this);
            _vm.RefreshState();
        } catch (Exception ex) { await ex.AlertAsync("自动加载目录", this); }
    }

    private async void ShowDictWindow(object? _, RoutedEventArgs e) {
        try {
            await new DictWindow().ShowDialog(this);
            _vm.RefreshState();
        } catch (Exception ex) { await ex.AlertAsync("管理词库并刷新状态", this); }
    }

    private async void ShowLogWindow(object? _, RoutedEventArgs e) {
        try { await new LogWindow().ShowDialog(this); } catch (Exception ex) {
            await ex.AlertAsync("查看日志", this);
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e) {
        try {
            if (!_closeConfirmed
             && DictManager.AllDicts.AsValueEnumerable().Any(static x => x.Modified)) {
                e.Cancel = true;
                if (await MsgBox.AskAsync<bool>("有词库的变更未保存，是否丢弃？")) {
                    _closeConfirmed = true;
                    Close();
                    return;
                }
            }
            base.OnClosing(e);
        } catch (Exception ex) { await ex.AlertAsync("阻止退出", this); }
    }
}
