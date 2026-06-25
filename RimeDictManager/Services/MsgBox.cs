namespace RimeDictManager.Services;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Views;

public static class MsgBox {
    public static Task SuccessAsync(string msg, Window? owner = null) =>
        new MsgWindow("成功", msg, false).ShowDialog(owner ?? GetTopWindow());

    public static Task<T> AskAsync<T>(string msg, Window? owner = null) =>
        new MsgWindow("确认", msg, true).ShowDialog<T>(owner ?? GetTopWindow());

    public static Task AlertAsync(this Exception ex, string op, Window? owner = null) {
        var msg = $"{op}时出错：{ex.Message}";
        Log.Err(msg);
        return new MsgWindow("错误", $"{msg}\n\n详情：\n\n{ex}", false).ShowDialog(
            owner ?? GetTopWindow());
    }

    private static Window GetTopWindow() {
        if (Application.Current?.ApplicationLifetime is
            not IClassicDesktopStyleApplicationLifetime { MainWindow: {} w })
            throw new InvalidOperationException("无法获取主窗口");
        while (w.OwnedWindows is [.., var last]) w = last;
        return w;
    }
}
