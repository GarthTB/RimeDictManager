namespace RimeDictManager.Utils;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Views;

public static class Dialog {
    private static Window Owner =>
        ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Windows
        .FirstOrDefault(static w => w.IsActive)!;

    public static Task<bool> Ask(string msg) => new MsgBox(msg, true).ShowDialog<bool>(Owner);
    public static Task Show(string msg) => new MsgBox(msg, false).ShowDialog(Owner);
}
