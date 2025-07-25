namespace RimeDictManager.Views;

/// <summary> MainWindow.xaml的交互逻辑 </summary>
public partial class MainWindow : System.Windows.Window
{
    public MainWindow() => InitializeComponent();

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        => e.Cancel = DataContext is ViewModels.MainViewModel { DictManager.IsModified: true }
        && !Utils.MsgBox.Confirm("警告", "有未保存的修改，是否直接退出？");
}
