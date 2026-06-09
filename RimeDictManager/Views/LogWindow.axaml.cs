namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;
using Utils;

public sealed partial class LogWindow: Window {
    public LogWindow() {
        InitializeComponent();
        Logs.Text = Log.ReadAll();
        BtnSave.IsEnabled = Logs.Text.Length > 0;
    }

    private async void SaveLog(object? _, RoutedEventArgs e) {
        try {
            using var file = await StorageProvider.SaveFilePickerAsync(
                new() {
                    Title = "将日志保存至...",
                    SuggestedFileName = $"RDM_{DateTime.Now:yyMMdd_HHmmss}",
                    FileTypeChoices = [FileTypes.Log, FilePickerFileTypes.All]
                });
            if (file is null) return;
            await using var stream = await file.OpenWriteAsync();
            await Log.SaveAsync(stream);
            await Dialog.Inform(this, $"日志已写入'{file.Name}'");
        } catch (Exception ex) { await Dialog.Inform(this, $"保存日志时：\n{ex}"); }
    }
}
