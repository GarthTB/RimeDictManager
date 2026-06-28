namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;
using ViewModels;
using OpEx = InvalidOperationException;

public sealed partial class DictWindow: Window {
    private const string OverwritePrompt = "是：覆写原词库\n否：另存副本（如果下一次选择覆写，覆写原文件而不是副本）",
        ReorderPrompt = "是：词条先按编码升序再按原序重排，非词条行按原序排在末尾\n否：保持原有行，新词条按插入顺序排在末尾";

    private readonly FolderPickerOpenOptions _openDirOptions = new() {
        Title = "选取 RIME 词库目录...", AllowMultiple = false
    };

    private readonly FilePickerOpenOptions _openFileOptions = new() {
        AllowMultiple = true,
        SuggestedFileType = FileTypes.RimeDict,
        FileTypeFilter = [FileTypes.RimeDict, FilePickerFileTypes.All]
    };

    private readonly FilePickerSaveOptions _saveOptions = new() {
        Title = "将词库另存到...", FileTypeChoices = [FileTypes.RimeDict, FilePickerFileTypes.All]
    };

    private readonly DictWindowVM _vm = new();

    public DictWindow() {
        InitializeComponent();
        DataContext = _vm;
        Title = $"{Meta.Name} - 词库";
        Closed += (_, _) => Encoder.Prepare(_vm.SelInputMethod);
    }

    public DictWindow(string dir): this() =>
        Loaded += async (_, _) => {
            var folder = await StorageProvider.TryGetFolderFromPathAsync(dir);
            _openFileOptions.SuggestedStartLocation = folder;
            _openDirOptions.SuggestedStartLocation = folder;
            await LoadDirAsync(dir);
        };

    private async Task LoadDirAsync(string dir) {
        try {
            var (dict, single) = await _vm.LoadDirAsync(dir);
            if (dict + single == 0) throw new OpEx("指定的目录中没有词库和单字码表，什么都没做");
            await MsgBox.SuccessAsync($"成功加载 {dict} 个词库、{single} 个单字码表。目录：{dir}", this);
        } catch (Exception ex) { await ex.AlertAsync("加载目录", this); }
    }

    private async void AddDir(object? _, RoutedEventArgs e) {
        try {
            var folders = await StorageProvider.OpenFolderPickerAsync(_openDirOptions);
            if (folders.Count == 0) return;

            var folder = folders[0];
            var dir = folder.TryGetLocalPath() ?? folder.Path.LocalPath;
            await LoadDirAsync(dir);
        } catch (Exception ex) { await ex.AlertAsync("选取目录", this); }
    }

    private async void AddDict(object? _, RoutedEventArgs e) {
        try {
            _openFileOptions.Title = "选取 RIME 词库...";
            var files = await StorageProvider.OpenFilePickerAsync(_openFileOptions);
            if (files.Count == 0) return;

            foreach (var file in files) {
                var path = file.TryGetLocalPath() ?? file.Path.LocalPath;
                file.Dispose();
                await _vm.AddDictAsync(path);
            }
        } catch (Exception ex) { await ex.AlertAsync("添加词库", this); }
    }

    private async void SaveDict(object? _, RoutedEventArgs e) {
        try {
            var overwrite = await MsgBox.AskAsync<bool?>(OverwritePrompt, this);
            if (overwrite is null) return;

            var dict = _vm.SelDict ?? throw new OpEx("UI 错误");
            string? path = null;
            if (overwrite == false) {
                _saveOptions.SuggestedFileName = dict.Src.Name;
                using var file = await StorageProvider.SaveFilePickerAsync(_saveOptions);
                if (file is null) return;

                path = file.TryGetLocalPath() ?? file.Path.LocalPath;
            }

            var reorder = await MsgBox.AskAsync<bool?>(ReorderPrompt, this);
            if (reorder is null) return;

            await _vm.SaveAsync(dict, path, reorder == true);
            await MsgBox.SuccessAsync($"成功保存到：{path ?? dict.Src.Path}", this);
        } catch (Exception ex) { await ex.AlertAsync("保存词库", this); }
    }

    private async void AddSingleDict(object? _, RoutedEventArgs e) {
        try {
            _openFileOptions.Title = "选取 RIME 单字码表...";
            var files = await StorageProvider.OpenFilePickerAsync(_openFileOptions);
            if (files.Count == 0) return;

            foreach (var file in files) {
                var path = file.TryGetLocalPath() ?? file.Path.LocalPath;
                file.Dispose();
                await _vm.AddSingleDictAsync(path);
            }
        } catch (Exception ex) { await ex.AlertAsync("添加单字码表", this); }
    }
}
