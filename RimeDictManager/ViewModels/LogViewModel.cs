namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;
using static Helper;

/// <summary> 日志窗口的视图模型 </summary>
internal sealed partial class LogViewModel: ObservableObject
{
    /// <summary> 日志文本 </summary>
    [ObservableProperty]
    private string _logText = string.Join('\n', Logger.Entries);

    /// <summary> 日志是否有内容 </summary>
    private static bool HasLogs => Logger.Entries.Count > 1;

    /// <summary> 将日志保存为文件 </summary>
    [RelayCommand(CanExecute = nameof(HasLogs))]
    private static void Save() =>
        TryOrShowEx(
            "保存日志",
            static () => {
                SaveFileDialog dialog = new() {
                    Title = "将日志保存到...",
                    DefaultExt = ".log",
                    FileName = $"RDM_{DateTime.Now:yyMMdd-HHmmss}.log",
                    Filter = "日志文件 (*.log)|*.log|所有文件 (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true)
                    return;

                Logger.Save(dialog.FileName);
                var count = Logger.Entries.Count - 1;
                ShowInfo("成功", $"已将{count}条日志保存到\"{dialog.FileName}\"");
            });
}
