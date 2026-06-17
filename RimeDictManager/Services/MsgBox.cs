namespace RimeDictManager.Services;

using Avalonia;
using Avalonia.Controls;
using Views;
using Desktop = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

public static class MsgBox {
    public static Task Info(string msg, Window? owner = null) =>
        new MsgWindow("提示", msg, false).ShowDialog(owner ?? GetTopWindow());

    public static Task<T> Ask<T>(string msg, Window? owner = null) =>
        new MsgWindow("确认", msg, true).ShowDialog<T>(owner ?? GetTopWindow());

    public static Task Alert(this Exception ex, string op, Window? owner = null) {
        var msg = $"{op}时出错：{ex.Message}";
        Log.Err(msg);
        return new MsgWindow("错误", $"{msg}\n\n详情：\n\n{ex}", false).ShowDialog(
            owner ?? GetTopWindow());
    }

    private static Window GetTopWindow() {
        var w = ((Desktop)Application.Current!.ApplicationLifetime!).MainWindow!;
        while (w.OwnedWindows is [.., var last]) w = last;
        return w;
    }
}
