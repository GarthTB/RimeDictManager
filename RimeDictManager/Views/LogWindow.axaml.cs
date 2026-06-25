namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;

public sealed partial class LogWindow: Window {
    private readonly FilePickerSaveOptions _saveOptions = new() {
        Title = "将日志保存至...", FileTypeChoices = [FileTypes.Log, FilePickerFileTypes.All]
    };

    public LogWindow() {
        InitializeComponent();
        Title = $"{Meta.Name} - 日志";
        var log = Log.All;
        BtnSave.IsEnabled = log.Count > 0;
        Logs.Text = log.Count > 0
            ? string.Join('\n', log)
            : "尚无操作";
    }

    private async void SaveLog(object? _, RoutedEventArgs e) {
        try {
            _saveOptions.SuggestedFileName = $"RDM_{DateTime.Now:yyMMdd_HHmmss}";
            using var file = await StorageProvider.SaveFilePickerAsync(_saveOptions);
            if (file is null) return;

            await using (var stream = await file.OpenWriteAsync()) await Log.SaveAsync(stream);
            var path = file.TryGetLocalPath() ?? file.Path.LocalPath;
            await MsgBox.SuccessAsync($"保存成功，路径：{path}", this);
        } catch (Exception ex) { await ex.AlertAsync("保存日志", this); }
    }
}
