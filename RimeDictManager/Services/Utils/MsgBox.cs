namespace RimeDictManager.Services.Utils;

using Avalonia;
using Avalonia.Controls;
using Views;
using Desktop = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

public static class MsgBox {
    public static Task Info(string msg, Window? owner = null) =>
        new MsgWindow(msg, false).ShowDialog(owner ?? GetTopWindow());

    public static Task<T> Ask<T>(string msg, Window? owner = null) =>
        new MsgWindow(msg, true).ShowDialog<T>(owner ?? GetTopWindow());

    public static Task Alert(this Exception ex, string op, Window? owner = null) {
        Log.Err(ex, op);
        return new MsgWindow($"{op}时：\n{ex}", false).ShowDialog(owner ?? GetTopWindow());
    }

    private static Window GetTopWindow() {
        var w = ((Desktop)Application.Current!.ApplicationLifetime!).MainWindow!;
        while (w.OwnedWindows is [.., var last]) w = last;
        return w;
    }
}
