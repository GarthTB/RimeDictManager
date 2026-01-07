namespace RimeDictManager.ViewModels;

using System.Windows;

/// <summary> 视图模型工具 </summary>
internal static class VmHelper
{
    /// <summary> 弹窗提示信息 </summary>
    /// <param name="title"> 弹窗标题 </param>
    /// <param name="msg"> 提示信息 </param>
    public static void ShowInfo(string title, string msg) =>
        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);

    /// <summary> 弹窗请求确认 </summary>
    /// <param name="title"> 弹窗标题 </param>
    /// <param name="msg"> 提示信息 </param>
    /// <returns> 用户按Yes为true </returns>
    public static bool ShowConfirm(string title, string msg) =>
        MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
     == MessageBoxResult.Yes;

    /// <summary> 尝试执行动作，静默处理打断，其他异常弹窗提示 </summary>
    /// <param name="name"> 动作名 </param>
    /// <param name="action"> 动作 </param>
    public static void TryOrShowEx(string name, Action action) {
        try {
            action();
        } catch (Exception ex) {
            var msg = string.IsNullOrWhiteSpace(ex.Message)
                ? "无描述的异常"
                : $"{name}异常：{ex.Message}";
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                msg += $"\n栈追踪：\n{ex.StackTrace}";
            MessageBox.Show(msg, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
