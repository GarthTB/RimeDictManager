namespace RimeDictManager.Views;

using System.ComponentModel;
using System.Windows;
using ViewModels;

/// <summary> 主窗口的交互逻辑 </summary>
public sealed partial class MainWindow
{
    /// <summary> 构造函数 </summary>
    public MainWindow() => InitializeComponent();

    /// <summary> 拖放词库路径 </summary>
    private void DropDict(object _, DragEventArgs e) {
        if (DataContext is MainViewModel { KeepModification: false } vm
         && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths)
            vm.LoadDict(paths[0]);
    }

    /// <summary> 拖放单字路径 </summary>
    private void DropCharsDict(object _, DragEventArgs e) {
        if (DataContext is MainViewModel { KeepModification: false } vm
         && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths)
            vm.LoadEncoder(paths[0]);
    }

    /// <summary> 打开日志窗口 </summary>
    private void OpenLog(object _, RoutedEventArgs e) => new LogWindow().ShowDialog();

    /// <summary> 在关闭时警告词库改动未保存 </summary>
    private void WarnModificationAtClosing(object _, CancelEventArgs e) =>
        e.Cancel = DataContext is MainViewModel { KeepModification: true };
}
