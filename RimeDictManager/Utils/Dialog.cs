namespace RimeDictManager.Utils;

using Avalonia.Controls;
using Views;

public static class Dialog {
    public static Task<bool> Ask(Window owner, string msg) =>
        new MsgBox(msg, true).ShowDialog<bool>(owner);

    public static Task Ex(Window owner, string op, Exception ex) =>
        new MsgBox($"{op}时：\n{ex}", false).ShowDialog(owner);

    public static Task Info(Window owner, string msg) => new MsgBox(msg, false).ShowDialog(owner);
}
