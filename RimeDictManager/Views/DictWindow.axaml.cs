namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Services;
using ViewModels;

public sealed partial class DictWindow: Window {
    private readonly FilePickerOpenOptions _openOptions = new() {
        AllowMultiple = true,
        SuggestedFileType = FileTypes.RimeDict,
        FileTypeFilter = [FileTypes.RimeDict, FilePickerFileTypes.All]
    };

    public DictWindow() {
        InitializeComponent();
        DataContext = new DictWindowVM();
    }

    private async void AddDict(object? _, RoutedEventArgs e) {
        try {
            _openOptions.Title = "打开 RIME 词库...";
            var files = await StorageProvider.OpenFilePickerAsync(_openOptions);
            if (files.Count == 0) return;
            var vm = (DictWindowVM)DataContext!;
            foreach (var file in files) {
                vm.AddDict(file.TryGetLocalPath() ?? file.Path.LocalPath);
                file.Dispose();
            }
        } catch (Exception ex) { await MsgBox.Ex("添加词库", ex, this); }
    }

    private async void AddSingleDict(object? _, RoutedEventArgs e) {
        try {
            _openOptions.Title = "打开 RIME 单字码表...";
            var files = await StorageProvider.OpenFilePickerAsync(_openOptions);
            if (files.Count == 0) return;
            var vm = (DictWindowVM)DataContext!;
            foreach (var file in files) {
                vm.AddSingleDict(file.TryGetLocalPath() ?? file.Path.LocalPath);
                file.Dispose();
            }
        } catch (Exception ex) { await MsgBox.Ex("添加单字码表", ex, this); }
    }
}
