namespace RimeDictManager.Views;

using System.Windows;
using VMs;

public sealed partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
        Closing += (_, e) => e.Cancel = !((MainVM)DataContext).EnsureSaved;
    }

    private void DropDict(object _, DragEventArgs e) {
        var vm = (MainVM)DataContext;
        if (vm.EnsureSaved && e.Data.GetData(DataFormats.FileDrop) is string[] and [var path])
            vm.LoadDict(path);
    }

    private void DropSingle(object _, DragEventArgs e) {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] and [var path])
            ((MainVM)DataContext).LoadSingle(path);
    }

    private void ShowLogs(object _, RoutedEventArgs e) => new LogWindow().ShowDialog();
}
