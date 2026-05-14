namespace RimeDictManager.VMs;

using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;
using static System.Windows.MessageBoxResult;
using static MsgBox;

internal sealed partial class LogVM: ObservableObject {
    public static string Logs => Logger.Dump();
    private static bool Any => Logger.Count > 0;

    [RelayCommand(CanExecute = nameof(Any))]
    private static void Save() {
        try {
            var sfd = new SaveFileDialog {
                Title = "将日志保存至...",
                FileName = $"RDM_{DateTime.Now:yyMMdd_HHmmss}.log",
                Filter = "日志文件|*.log|所有文件|*.*",
                OverwritePrompt = false
            };
            if (sfd.ShowDialog() != true) return;
            var path = sfd.FileName;
            var msg = $"确认追加此文件？\n'{path}'";
            if (File.Exists(path) && ShowConfirm("确认", msg) != Yes) return;

            Logger.Save(path);
            ShowInfo("成功", $"日志已写入'{path}'，共{Logger.Count}条");
        } catch (Exception ex) { ShowErr($"保存日志时：\n{ex}"); }
    }
}
