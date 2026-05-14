namespace RimeDictManager.VMs;

using static System.Windows.MessageBox;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;
using Result = System.Windows.MessageBoxResult;

internal static class MsgBox {
    public static void ShowInfo(string title, string msg) => Show(msg, title, OK, Information);
    public static void ShowErr(string msg) => Show(msg, "异常", OK, Error);

    public static Result ShowConfirm(string title, string msg) =>
        Show(msg, title, YesNoCancel, Question);
}
