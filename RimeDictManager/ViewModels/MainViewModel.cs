// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Services;
using static VmHelper;

/// <summary> 主窗口的视图模型 </summary>
internal sealed partial class MainViewModel: ObservableObject
{
    #region 打开词库

    /// <summary> RIME词库 </summary>
    [ObservableProperty]
    private RimeDict? _dict;

    /// <summary> 词库改动未保存且选择保留时为true </summary>
    public bool KeepModification => Dict?.Modified == true && !ShowConfirm("警告", "词库改动未保存，是否丢弃？");

    /// <summary> 打开RIME词库文件（.dict.yaml） </summary>
    [RelayCommand]
    private void Open() =>
        TryOrShowEx(
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
        TryOrShowEx(
            "加载词库",
            () => {
                Dict = new(path);
                var msg1 = $"已加载词库\"{path}\"";
                var msg2 = $"共有{Dict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}");
            });

    #endregion 打开词库

    #region 保存词库

    /// <summary> 是否可以保存词库 </summary>
    private bool CanSave => Dict?.Modified == true;

    /// <summary> 保存修改并覆写原词库文件 </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save(string? path) =>
        TryOrShowEx(
            "保存词库",
            () => {
                var sort = ShowConfirm(
                    "选择排序策略",
                    "请选择排序策略：\n是：条目按编码升序，编码相同则原序，注释原序放在末尾，空行删除\n否：保留原序，新条目按编码升序放在末尾");
                Dict!.Save(path, sort);
                OnPropertyChanged(nameof(Dict));
                var msg1 = $"已保存词库到\"{path}\"";
                var msg2 = $"共有{Dict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}");
            });

    /// <summary> 是否可以另存词库 </summary>
    private bool CanSaveAs => Dict is {};

    /// <summary> 保存修改并另存为RIME词库文件（.dict.yaml） </summary>
    [RelayCommand(CanExecute = nameof(CanSaveAs))]
    private void SaveAs() =>
        TryOrShowEx(
            "另存词库",
            () => {
                SaveFileDialog dialog = new() {
                    Title = "将词库保存到...",
                    DefaultExt = ".dict.yaml",
                    Filter = "RIME词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                    Save(dialog.FileName);
            });

    #endregion 保存词库

    #region 属性变更响应

    partial void OnDictChanged(RimeDict? value) => Search();

    /// <summary> 搜索并填充SearchResults </summary>
    private static void Search() => throw new NotImplementedException();

    #endregion 属性变更响应
}
