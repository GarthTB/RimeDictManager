namespace RimeDictManager.Services;

using Avalonia;
using Avalonia.Controls;
using Views;
using Desktop = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

public static class MsgBox {
    public static Task<T> Ask<T>(string msg, Window? owner = null) =>
        new MsgWindow(msg, true).ShowDialog<T>(owner ?? GetTopWindow());

    public static Task Err(string op, Exception ex, Window? owner = null) =>
        new MsgWindow($"{op}时：\n{ex}", false).ShowDialog(owner ?? GetTopWindow());

    public static Task Info(string msg, Window? owner = null) =>
        new MsgWindow(msg, false).ShowDialog(owner ?? GetTopWindow());

    private static Window GetTopWindow() {
        var w = ((Desktop)Application.Current!.ApplicationLifetime!).MainWindow!;
        while (w.OwnedWindows is [.., var last]) w = last;
        return w;
    }
}
