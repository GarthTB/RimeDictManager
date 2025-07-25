using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RimeDictManager.Services.Logging;
using RimeDictManager.Utils;

namespace RimeDictManager.ViewModels;

/// <summary> 日志窗口的ViewModel </summary>
internal partial class LogViewModel : ObservableObject
{
    /// <summary> 只读静态日志单例 </summary>
    private readonly ILogReader _logReader = Logger.Reader;

    /// <summary> 日志文本（直接从单例获取） </summary>
    [ObservableProperty]
    private string _logText =
        string.Join('\n', Logger.Reader.Logs);

    /// <returns> 日志是否有内容 </returns>
    private bool HasLogs => _logReader.Logs.Count > 1;

    /// <summary> 将修改日志保存为一个文本文件 </summary>
    [RelayCommand(CanExecute = nameof(HasLogs))]
    private async Task Save()
        => await Try.DoAsync("保存日志", async () =>
        {
            SaveFileDialog dialog = new()
            {
                Title = "将日志保存为...",
                FileName = $"RimeDictManager_{DateTime.Now:yyMMdd-HHmmss}.log",
                Filter = "日志文件 (*.log)|*.log|所有文件 (*.*)|*.*",
                DefaultExt = "log"
            };
            if (dialog.ShowDialog() != true)
                return;

            await _logReader.SaveAsync(dialog.FileName);
            var info1 = $"成功保存至 {dialog.FileName}";
            var info2 = $"共有 {_logReader.Logs.Count - 1} 条日志";
            MsgBox.Info("成功", $"{info1}\n{info2}");
        });
}
