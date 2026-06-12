namespace RimeDictManager.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using ViewModels;

public sealed partial class MainWindow: Window {
    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowVM();
    }

    private void Test(object? _, RoutedEventArgs e) => new DictWindow().ShowDialog(this);
}
