namespace RimeDictManager.Views;

using System.IO;
using System.Windows;
using ViewModels;

/// <summary> 主窗口的交互逻辑 </summary>
public sealed partial class MainWindow
{
    /// <summary> 构造函数 </summary>
    public MainWindow() => InitializeComponent();

    /// <summary> 打开日志窗口 </summary>
    private void OpenLog(object _, RoutedEventArgs e) => new LogWindow().ShowDialog();

    /// <summary> 加载RIME词库文件（.dict.yaml） </summary>
    private void LoadDict(object _, DragEventArgs e) {
        if (DataContext is MainViewModel vm
         && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths
         && File.Exists(paths[0])
         && !vm.KeepModification)
            vm.LoadDict(paths[0]);
    }
}
