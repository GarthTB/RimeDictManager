namespace RimeDictManager.Views;

using System.Windows;
using VMs;

public sealed partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
        Closing += (_, e) => { e.Cancel = DataContext is MainVM { UnmodOrDiscard: false }; };
    }

    private void DropDict(object _, DragEventArgs e) {
        if (DataContext is MainVM { UnmodOrDiscard: true } vm
         && e.Data.GetData(DataFormats.FileDrop) is string[] and [var path])
            vm.LoadDict(path);
    }

    private void DropCharsDict(object _, DragEventArgs e) {
        if (DataContext is MainVM vm
         && e.Data.GetData(DataFormats.FileDrop) is string[] and [var path])
            vm.SetEncoder(path);
    }

    private void ShowLogWindow(object _, RoutedEventArgs e) => new LogWindow().ShowDialog();
}
