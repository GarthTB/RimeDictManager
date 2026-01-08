// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Encoders;
using Microsoft.Win32;
using Models;
using Services;
using static VmHelper;

/// <summary> 主窗口的视图模型 </summary>
internal sealed partial class MainViewModel: ObservableObject
{
    #region 打开词库

    /// <summary> RIME词库 </summary>
    [ObservableProperty,
     NotifyCanExecuteChangedFor(
         nameof(SaveCommand),
         nameof(SaveAsCommand),
         nameof(InsertCommand),
         nameof(RemoveCommand),
         nameof(ShortenCommand),
         nameof(ModifyCommand))]
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
                SaveCommand.NotifyCanExecuteChanged();
                var msg1 = $"已保存词库到\"{path}\"";
                var msg2 = $"共有{Dict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}");
            });

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
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private string _word = "";

    /// <summary> 手动编码 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private string _manualCode = "";

    /// <summary> 权重 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private string _weight = "";

    /// <summary> 自动编码结果颜色 </summary>
    [ObservableProperty]
    private string _autoCodeColor = "Black";

    /// <summary> 自动编码结果 </summary>
    public ObservableCollection<string> AutoCodes { get; } = [];

    /// <summary> 自动编码结果索引 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private int _autoCodeIdx = -1;

    private string? SelectedAutoCode =>
        AutoCodeIdx >= 0 && AutoCodeIdx < AutoCodes.Count
            ? AutoCodes[AutoCodeIdx]
            : null;

    private string? CurCode =>
        UseEncoder
            ? SelectedAutoCode
            : ManualCode.Trim();

    /// <summary> 用当前各属性构造的新条目 </summary>
    private Line CurEntry => new(null, Word, CurCode, Weight, null);

    #endregion 条目属性

    #region 自动编码

    /// <summary> 是否使用自动编码 </summary>
    [ObservableProperty,
     NotifyCanExecuteChangedFor(
         nameof(ChangeEncoderCommand),
         nameof(InsertCommand),
         nameof(ShortenCommand))]
    private bool _useEncoder, _notUseEncoder = true;

    /// <summary> 可用编码方案名 </summary>
    public IReadOnlyList<string> EncoderNames { get; } = EncoderFactory.Names;

    /// <summary> 编码方案名索引 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ChangeEncoderCommand))]
    private int _encoderNameIdx = -1;

    private string? SelectedEncoderName =>
        EncoderNameIdx >= 0 && EncoderNameIdx < EncoderNames.Count
            ? EncoderNames[EncoderNameIdx]
            : null;

    /// <summary> 编码器 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    private IEncoder? _encoder;

    partial void OnEncoderChanged(IEncoder? value) {
        (MinLen, MaxLen) = value?.LenRange ?? (4, 4);
        CurLen = MinLen;
        Encode();
    }

    private bool CanChangeEncoder => UseEncoder && SelectedEncoderName is {};

    /// <summary> 更换当前方案的单字词库并更新编码器 </summary>
    [RelayCommand(CanExecute = nameof(CanChangeEncoder))]
    private void ChangeEncoder() =>
        TryOrShowEx(
            "更换单字",
            () => {
                OpenFileDialog dialog = new() {
                    Title = $"打开\"{SelectedEncoderName}\"的RIME单字词库文件",
                    Filter = "RIME词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                    LoadEncoder(dialog.FileName);
            });

    /// <summary> 加载单字词库并构造编码器 </summary>
    /// <param name="path"> 单字词库路径 </param>
    public void LoadEncoder(string path) =>
        TryOrShowEx(
            "加载编码器",
            () => {
                var encoder = EncoderFactory.Create(SelectedEncoderName!, path);
                if (encoder.Chars == 0)
                    throw new InvalidOperationException("单字词库无效");
                Encoder = encoder;
                var msg1 = $"已启用\"{SelectedEncoderName}\"的编码器";
                var msg2 = $"使用单字词库\"{path}\"";
                var msg3 = $"覆盖{Encoder.Chars}个单字";
                AuditLogger.Log($"{msg1}，{msg2}，{msg3}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}\n{msg3}");
            });

    /// <summary> 自动编码的码长 </summary>
    [ObservableProperty]
    private byte _maxLen = 4, _minLen = 4, _curLen = 4;

    /// <summary> 执行自动编码 </summary>
    private static void Encode() => throw new NotImplementedException();

    #endregion 自动编码

    #region 搜索

    /// <summary> 搜索模式：0按编码前缀搜索，1按词组精准搜索 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    private byte _searchMode;

    /// <summary> 搜索内容 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    private string _searchText = "";

    /// <summary> 搜索结果 </summary>
    public ObservableCollection<MutEntry> SearchResults { get; } = [];

    /// <summary> 搜索结果索引 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveCommand), nameof(ShortenCommand))]
    private int _searchResultIdx = -1;

    private MutEntry? SelectedSearchResult =>
        SearchResultIdx >= 0 && SearchResultIdx < SearchResults.Count
            ? SearchResults[SearchResultIdx]
            : null;

    /// <summary> 搜索并填充SearchResults </summary>
    private static void Search() => throw new NotImplementedException();

    #endregion 搜索

    #region 词库操作

    private bool CanInsert => Dict is {} && CurEntry.IsEntry == true;

    /// <summary> 将各属性添加为一个新条目 </summary>
    [RelayCommand(CanExecute = nameof(CanInsert))]
    private static void Insert() =>
        TryOrShowEx("添加条目", static () => throw new NotImplementedException());

    private bool CanRemove => Dict is {} && SelectedSearchResult is {};

    /// <summary> 将表格里选中的条目删除 </summary>
    [RelayCommand(CanExecute = nameof(CanRemove))]
    private static void Remove() =>
        TryOrShowEx("删除条目", static () => throw new NotImplementedException());

    private bool CanShorten =>
        Dict is {}
     && UseEncoder
     && MaxLen != MinLen
     && SearchMode == 0
     && SelectedSearchResult is { Modified: false }
     && SelectedSearchResult.Code.Length > SearchText.Length;

    /// <summary> 将表格里选中条目的编码截短为搜索框中的编码；若有一个条目占用该编码，则自动加长其编码到最短的空闲位置 </summary>
    /// <remarks> 需开启自动编码和编码前缀搜索；仅用于变长编码方案 </remarks>
    [RelayCommand(CanExecute = nameof(CanShorten))]
    private static void Shorten() =>
        TryOrShowEx("截短编码", static () => throw new NotImplementedException());

    private bool CanModify => Dict is {} && SearchResults.Count > 0;

    /// <summary> 应用在表格中的改动 </summary>
    [RelayCommand(CanExecute = nameof(CanModify))]
    private static void Modify() =>
        TryOrShowEx("应用修改", static () => throw new NotImplementedException());

    #endregion 词库操作
}
