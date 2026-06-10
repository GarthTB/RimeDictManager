namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;

public sealed partial class LogWindow: Window {
    private readonly FilePickerSaveOptions _saveOptions = new() {
        Title = "将日志保存至...",
        SuggestedFileName = $"RDM_{DateTime.Now:yyMMdd_HHmmss}",
        FileTypeChoices = [FileTypes.Log, FilePickerFileTypes.All]
    };

    public LogWindow() {
        InitializeComponent();
        var text = Log.ReadAll();
        Logs.Text = text.Length == 0
            ? "暂未记录任何操作"
            : text;
        BtnSave.IsEnabled = text.Length > 0;
    }

    private async void SaveLog(object? _, RoutedEventArgs e) {
        try {
            using var file = await StorageProvider.SaveFilePickerAsync(_saveOptions);
            if (file is null) return;
            await using (var stream = await file.OpenWriteAsync()) await Log.SaveAsync(stream);
            await MsgBox.Info($"日志已写入'{file.Name}'", this);
        } catch (Exception ex) { await MsgBox.Ex("保存日志", ex, this); }
    }
}
