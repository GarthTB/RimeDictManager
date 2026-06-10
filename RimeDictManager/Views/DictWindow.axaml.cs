namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using ViewModels;

public sealed partial class DictWindow: Window {
    private readonly FilePickerOpenOptions _openOptions = new() {
        Title = "打开 RIME 词库...",
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
            var files = await StorageProvider.OpenFilePickerAsync(_openOptions);
            if (files.Count < 1) return;
            var vm = (DictWindowVM)DataContext!;
            foreach (var file in files) {
                vm.Add(file.TryGetLocalPath() ?? file.Path.LocalPath);
                file.Dispose();
            }
        } catch (Exception ex) { await MsgBox.Ex("添加词库", ex, this); }
    }
}
