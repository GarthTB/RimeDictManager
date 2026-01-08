namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;

/// <summary> 主窗口的视图模型 </summary>
internal sealed partial class MainViewModel: ObservableObject
{
    #region 词库打开、保存

    /// <summary> RIME词库 </summary>
    private RimeDict? _dict;

    /// <summary> 词库改动未保存且选择保留时为true </summary>
    public bool KeepModification =>
        _dict?.Modified == true && !VmHelper.ShowConfirm("警告", "词库改动未保存，是否丢弃？");

    /// <summary> 打开RIME词库文件（.dict.yaml） </summary>
    [RelayCommand]
    private void Open() =>
        VmHelper.TryOrShowEx(
            "打开词库",
            () => {
                if (KeepModification)
                    return;

                OpenFileDialog dialog = new() {
                    Title = "打开RIME词库文件",
                    Filter = "RIME词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                    LoadDict(dialog.FileName);
            });

    /// <summary> 加载RIME词库文件（.dict.yaml） </summary>
    /// <param name="path"> 词库路径 </param>
    public void LoadDict(string path) =>
        VmHelper.TryOrShowEx(
            "加载词库",
            () => {
                _dict = new(path);
                Search(); // 触发搜索

                var msg1 = $"加载词库\"{path}\"";
                var msg2 = $"共有{_dict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                VmHelper.ShowInfo("成功", $"已{msg1}\n{msg2}");
            });

    private static void Search() => throw new NotImplementedException();

    #endregion 词库打开、保存
}
