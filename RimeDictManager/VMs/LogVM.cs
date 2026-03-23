namespace RimeDictManager.VMs;

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
                Filter = "日志文件|*.log|所有文件|*.*"
            };
            if (sfd.ShowDialog() != true) return;
            Logger.Save(sfd.FileName);
            Show($"已将{LogView.Count - 1}条日志存至'{sfd.FileName}'", "成功", OK, Information);
        } catch (Exception ex) { Show($"保存日志时：\n{ex}", "异常", OK, Error); }
    }
}
