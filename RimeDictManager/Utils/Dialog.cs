namespace RimeDictManager.Utils;

using Avalonia.Controls;
using Views;

public static class Dialog {
    public static Task<bool> Ask(Window owner, string msg) =>
        new MsgBox(msg, true).ShowDialog<bool>(owner);

    public static Task Inform(Window owner, string msg) => new MsgBox(msg, false).ShowDialog(owner);
}
