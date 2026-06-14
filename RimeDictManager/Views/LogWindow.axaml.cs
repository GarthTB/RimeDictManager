namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services.Utils;

public sealed partial class LogWindow: Window {
    private readonly FilePickerSaveOptions _saveOptions = new() {
        Title = "将日志保存至...", FileTypeChoices = [FileTypes.Log, FilePickerFileTypes.All]
    };

    public LogWindow() {
        InitializeComponent();
        var log = Log.All;
        if (BtnSave.IsEnabled = log.Count > 0) Logs.Text = string.Join('\n', log);
    }

    private async void SaveLog(object? _, RoutedEventArgs e) {
        try {
            _saveOptions.SuggestedFileName = $"RDM_{DateTime.Now:yyMMdd_HHmmss}";
            using var file = await StorageProvider.SaveFilePickerAsync(_saveOptions);
            if (file is null) return;
            await using (var stream = await file.OpenWriteAsync()) await Log.SaveAsync(stream);
            await MsgBox.Info($"保存成功，路径：{file.TryGetLocalPath() ?? file.Path.LocalPath}", this);
        } catch (Exception ex) { await ex.Alert("保存日志", this); }
    }
}
