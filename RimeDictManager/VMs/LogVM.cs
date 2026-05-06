namespace RimeDictManager.VMs;

using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;
using static System.Windows.MessageBox;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;
using static Services.Logger;

internal sealed partial class LogVM: ObservableObject {
    public static string LogText => string.Join('\n', LogView);
    private static bool HasLogs => LogView.Count > 1;

    [RelayCommand(CanExecute = nameof(HasLogs))]
    private static void Save() {
        try {
            var sfd = new SaveFileDialog {
                Title = "将日志保存为...",
                FileName = $"RDM_{DateTime.Now:yyMMdd-HHmmss}.log",
                Filter = "日志文件|*.log|所有文件|*.*",
                OverwritePrompt = false
            };
            if (sfd.ShowDialog() != true) return;
            var path = sfd.FileName;
            if (File.Exists(path)) {
                var msg = $"确认追加此文件？\n'{path}'";
                if (Show(msg, "确认", YesNo, Question) != MessageBoxResult.Yes) return;
            }
            Logger.Save(path);
            Show($"已将{LogView.Count - 1}条日志写入'{path}'", "成功", OK, Information);
        } catch (Exception ex) { Show($"保存日志时：\n{ex}", "异常", OK, Error); }
    }
}
