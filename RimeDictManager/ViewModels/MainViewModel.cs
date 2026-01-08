// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Encoders;
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

    partial void OnDictChanged(RimeDict? value) => Search();

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

    #region 条目属性

    /// <summary> 字词 </summary>
    [ObservableProperty]
    private string _word = "";

    /// <summary> 手动编码 </summary>
    [ObservableProperty]
    private string _manualCode = "";

    /// <summary> 权重 </summary>
    [ObservableProperty]
    private string _weight = "";

    /// <summary> 自动编码结果 </summary>
    public ObservableCollection<string> AutoCodes { get; } = [];

    /// <summary> 自动编码结果颜色 </summary>
    [ObservableProperty]
    private string _autoCodeColor = "Black";

    /// <summary> 自动编码结果索引 </summary>
    [ObservableProperty]
    private int _autoCodeIdx = -1;

    #endregion 条目属性

    #region 自动编码

    /// <summary> 是否使用自动编码 </summary>
    [ObservableProperty]
    private bool _useEncoder, _notUseEncoder = true;

    /// <summary> 可用编码方案名 </summary>
    public IReadOnlyList<string> EncoderNames { get; } = EncoderFactory.Names;

    /// <summary> 编码方案名索引 </summary>
    [ObservableProperty]
    private int _encoderNameIdx = -1;

    /// <summary> 编码器 </summary>
    [ObservableProperty]
    private IEncoder? _encoder;

    /// <summary> 更换当前方案的单字词库并更新编码器 </summary>
    [RelayCommand]
    private static void ChangeEncoder() =>
        TryOrShowEx("更换单字", static () => throw new NotImplementedException());

    /// <summary> 自动编码的码长 </summary>
    [ObservableProperty]
    private byte _maxLen = 4, _minLen = 4, _curLen = 4;

    #endregion 自动编码

    #region 搜索

    /// <summary> 搜索模式：0按编码前缀搜索，1按词组精准搜索 </summary>
    [ObservableProperty]
    private byte _searchMode;

    /// <summary> 搜索内容 </summary>
    [ObservableProperty]
    private string _searchText = "";

    /// <summary> 搜索结果 </summary>
    public ObservableCollection<MutEntry> SearchResults { get; } = [];

    /// <summary> 搜索结果索引 </summary>
    [ObservableProperty]
    private int _searchResultIdx = -1;

    /// <summary> 搜索并填充SearchResults </summary>
    private static void Search() => throw new NotImplementedException();

    #endregion 搜索

    #region 词库操作

    /// <summary> 将各属性添加为一个新条目 </summary>
    [RelayCommand]
    private static void Insert() =>
        TryOrShowEx("添加条目", static () => throw new NotImplementedException());

    /// <summary> 将表格里选中的条目删除 </summary>
    [RelayCommand]
    private static void Remove() =>
        TryOrShowEx("删除条目", static () => throw new NotImplementedException());

    /// <summary> 将表格里选中条目的编码截短为搜索框中的编码；若有一个条目占用该编码，则自动加长其编码到最短的空闲位置 </summary>
    /// <remarks> 需开启自动编码和编码前缀搜索；仅用于变长编码方案 </remarks>
    [RelayCommand]
    private static void Shorten() =>
        TryOrShowEx("截短编码", static () => throw new NotImplementedException());

    /// <summary> 应用在表格中的改动 </summary>
    [RelayCommand]
    private static void Modify() =>
        TryOrShowEx("应用修改", static () => throw new NotImplementedException());

    #endregion 词库操作
}
