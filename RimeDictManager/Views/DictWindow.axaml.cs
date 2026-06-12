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

    private void AddDict(object? _, RoutedEventArgs e) => throw new NotImplementedException();
    private void AddSingleDict(object? _, RoutedEventArgs e) => throw new NotImplementedException();
}
