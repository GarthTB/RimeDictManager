namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;
using ViewModels;

public sealed partial class DictWindow: Window {
    private const string OverwritePrompt = "是：覆写原词库\n否：另存副本（如果下一次选择覆写，覆写原文件而不是副本）",
        ReorderPrompt = "是：词条先按编码升序再按原序重排，空行丢弃，注释原序排在末尾\n否：保持原有行，新词条按插入顺序排在末尾";

    private readonly FilePickerOpenOptions _openOptions = new() {
        AllowMultiple = true,
        SuggestedFileType = FileTypes.RimeDict,
        FileTypeFilter = [FileTypes.RimeDict, FilePickerFileTypes.All]
    };

    private readonly FilePickerSaveOptions _saveOptions = new() {
        Title = "将词库另存至...", FileTypeChoices = [FileTypes.RimeDict, FilePickerFileTypes.All]
    };

    private readonly DictWindowVM _vm = new();

    public DictWindow() {
        InitializeComponent();
        DataContext = _vm;
        Closed += (_, _) => Encoder.Prepare(_vm.SelInputMethod);
    }

    private async void AddDict(object? _, RoutedEventArgs e) {
        try {
            _openOptions.Title = "打开 RIME 词库...";
            var files = await StorageProvider.OpenFilePickerAsync(_openOptions);
            if (files.Count == 0) return;
            foreach (var file in files) {
                _vm.AddDict(file.TryGetLocalPath() ?? file.Path.LocalPath);
                file.Dispose();
            }
        } catch (Exception ex) { await ex.Alert("添加词库", this); }
    }

    private async void SaveDict(object? _, RoutedEventArgs e) {
        try {
            var overwrite = await MsgBox.Ask<bool?>(OverwritePrompt, this);
            if (overwrite is null) return;
            var dict = _vm.SelDict!;
            string? path = null;
            if (overwrite == false) {
                _saveOptions.SuggestedFileName = dict.Src.Name;
                using var file = await StorageProvider.SaveFilePickerAsync(_saveOptions);
                if (file is null) return;
                path = file.TryGetLocalPath() ?? file.Path.LocalPath;
            }
            var reorder = await MsgBox.Ask<bool?>(ReorderPrompt, this);
            if (reorder is null) return;

            await _vm.SaveDict(dict, path, reorder == true);
            await MsgBox.Info($"保存成功，路径：{path ?? dict.Src.Path}", this);
        } catch (Exception ex) { await ex.Alert("保存词库", this); }
    }

    private async void AddSingleDict(object? _, RoutedEventArgs e) {
        try {
            _openOptions.Title = "打开 RIME 单字码表...";
            var files = await StorageProvider.OpenFilePickerAsync(_openOptions);
            if (files.Count == 0) return;
            foreach (var file in files) {
                _vm.AddSingleDict(file.TryGetLocalPath() ?? file.Path.LocalPath);
                file.Dispose();
            }
        } catch (Exception ex) { await ex.Alert("添加单字码表", this); }
    }
}
