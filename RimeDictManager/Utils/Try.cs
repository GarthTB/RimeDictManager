namespace RimeDictManager.Utils;

/// <summary> 用于简化Try-Catch块的工具类 </summary>
internal static class Try
{
    /// <summary> 执行一个Action，若有异常则弹窗提示 </summary>
    public static void Do(string actionName, Action action)
    {
        try { action(); }
        catch (Exception ex)
        { MsgBox.Error($"{actionName} 出错：\n{FormatException(ex)}"); }
    }

    /// <summary> 等待一个异步Func，若有异常则弹窗提示 </summary>
    public static async Task DoAsync(string actionName, Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex)
        { MsgBox.Error($"{actionName} 出错：\n{FormatException(ex)}"); }
    }

    /// <summary> 将异常信息及栈跟踪格式化为字符串 </summary>
    private static string FormatException(Exception ex)
        => string.IsNullOrWhiteSpace(ex.Message)
        ? "未知错误"
        : string.IsNullOrWhiteSpace(ex.StackTrace)
        ? ex.Message
        : $"{ex.Message}\n\n栈跟踪：\n\n{ex.StackTrace}";
}
