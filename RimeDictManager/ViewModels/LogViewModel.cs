namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;

/// <summary> 日志窗口的视图模型 </summary>
internal sealed partial class LogViewModel: ObservableObject
{
    /// <summary> 日志文本 </summary>
    [ObservableProperty]
    private string _logText = string.Join('\n', AuditLogger.Entries);

    /// <summary> 日志是否有内容 </summary>
    private static bool HasLogs => AuditLogger.Entries.Count > 1;

    /// <summary> 将日志保存为文件 </summary>
    [RelayCommand(CanExecute = nameof(HasLogs))]
    private static void Save() =>
        VmHelper.TryOrShowEx(
            "保存日志",
            static () => {
                SaveFileDialog dialog = new() {
                    Title = "将日志保存到...",
                    DefaultExt = "log",
                    FileName = $"RDM_{DateTime.Now:yyMMdd-HHmmss}.log",
                    Filter = "日志文件 (*.log)|*.log|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() != true)
                    return;

                AuditLogger.Save(dialog.FileName);
                VmHelper.ShowInfo("成功", $"已将{AuditLogger.Entries.Count}条日志保存到 {dialog.FileName}");
            });
}
