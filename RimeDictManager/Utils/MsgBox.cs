using System.Windows;

namespace RimeDictManager.Utils;

/// <summary> 用于显示消息弹窗的工具类 </summary>
internal static class MsgBox
{
    /// <summary> 显示问号弹窗，并返回用户是否确认 </summary>
    public static bool Confirm(string title, string text)
        => MessageBox.Show(
            text,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;

    /// <summary> 显示错误弹窗 </summary>
    public static void Error(string text)
        => _ = MessageBox.Show(
            text,
            "错误",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

    /// <summary> 显示信息弹窗 </summary>
    public static void Info(string title, string text)
        => _ = MessageBox.Show(
            text,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
}
