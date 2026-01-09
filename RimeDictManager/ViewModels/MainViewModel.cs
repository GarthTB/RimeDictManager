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

    private RimeDict? _rimeDict;

    /// <summary> 词库改动未保存且选择保留时为true </summary>
    public bool KeepModification =>
        _rimeDict?.Modified == true && !ShowConfirm("警告", "词库改动未保存，是否丢弃？");

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
                _rimeDict = new(path);
                SaveCommand.NotifyCanExecuteChanged();
                SaveAsCommand.NotifyCanExecuteChanged();
                InsertCommand.NotifyCanExecuteChanged();
                RemoveCommand.NotifyCanExecuteChanged();
                ShortenCommand.NotifyCanExecuteChanged();
                ModifyCommand.NotifyCanExecuteChanged();
                UpdateSearchResults();

                var msg1 = $"已加载词库\"{path}\"";
                var msg2 = $"共有{_rimeDict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}");
            });

    #endregion 打开词库

    #region 保存词库

    private bool CanSave => _rimeDict?.Modified == true;

    /// <summary> 保存修改并覆写原词库文件 </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save(string? path) =>
        TryOrShowEx(
            "保存词库",
            () => {
                var sort = ShowConfirm(
                    "选择排序策略",
                    "请选择排序策略：\n是：条目按编码升序，编码相同则原序，注释原序放在末尾，空行删除\n否：保留原序，新条目按编码升序放在末尾");
                _rimeDict!.Save(path, sort);
                SaveCommand.NotifyCanExecuteChanged();

                var msg1 = $"已保存词库到\"{path}\"";
                var msg2 = $"共有{_rimeDict.Count}个条目";
                AuditLogger.Log($"{msg1}，{msg2}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}");
            });

    private bool CanSaveAs => _rimeDict is {};

    /// <summary> 保存修改并另存为RIME词库文件（.dict.yaml） </summary>
    [RelayCommand(CanExecute = nameof(CanSaveAs))]
    private void SaveAs() =>
        TryOrShowEx(
            "另存词库",
            () => {
                SaveFileDialog dialog = new() {
                    Title = "将词库另存到...",
                    DefaultExt = ".dict.yaml",
                    Filter = "RIME词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                    Save(dialog.FileName);
            });

    #endregion 保存词库

    #region 条目属性

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private string _word = "", _manualCode = "", _weight = "", _autoCodeColor = "Black";

    partial void OnWordChanged(string value) {
        UpdateAutoCodes(true);
        AutoSearch();
    }

    partial void OnManualCodeChanged(string value) => AutoSearch();

    [ObservableProperty,
     NotifyCanExecuteChangedFor(
         nameof(SetEncoderCommand),
         nameof(InsertCommand),
         nameof(ShortenCommand))]
    private bool _useEncoder, _notUseEncoder = true;

    partial void OnUseEncoderChanged(bool value) => AutoSearch();

    public IReadOnlyCollection<string> EncoderNames { get; } = EncoderFactory.Names;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SetEncoderCommand))]
    private string? _selectedEncoderName;

    partial void OnSelectedEncoderNameChanged(string? value) => SetEncoder();

    private string[] _fullAutoCodes = [];
    public ObservableCollection<string> AutoCodes { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    private string? _selectedAutoCode;

    partial void OnSelectedAutoCodeChanged(string? value) => AutoSearch();

    private string? CurCode =>
        UseEncoder
            ? SelectedAutoCode
            : ManualCode.Trim() is { Length: > 0 } code
                ? code
                : null;

    private string? CurWeight =>
        Weight.Trim() is { Length: > 0 } weight
            ? weight
            : null;

    /// <summary> 用当前各属性构造的新条目 </summary>
    private Line CurEntry => new(null, Word.Trim(), CurCode, CurWeight, null);

    #endregion 条目属性

    #region 自动编码

    private IEncoder? _encoder;

    private bool CanSetEncoder => UseEncoder && SelectedEncoderName is {};

    /// <summary> 选取当前方案的单字词库并更新编码器 </summary>
    [RelayCommand(CanExecute = nameof(CanSetEncoder))]
    private void SetEncoder() =>
        TryOrShowEx(
            "选取单字词库",
            () => {
                OpenFileDialog dialog = new() {
                    Title = $"打开\"{SelectedEncoderName}\"的RIME单字词库文件",
                    Filter = "RIME词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                    CreateEncoder(dialog.FileName);
            });

    /// <summary> 加载单字词库并构造编码器 </summary>
    /// <param name="path"> 单字词库路径 </param>
    public void CreateEncoder(string path) =>
        TryOrShowEx(
            "加载编码器",
            () => {
                var encoder = EncoderFactory.Create(SelectedEncoderName!, path);
                if (encoder.Chars == 0)
                    throw new InvalidOperationException("单字词库无效");

                _encoder = encoder;
                (MinLen, MaxLen) = encoder.LenRange;
                CurLen = MinLen;
                ShortenCommand.NotifyCanExecuteChanged();
                UpdateAutoCodes(true);

                var msg1 = $"已启用\"{SelectedEncoderName}\"的编码器";
                var msg2 = $"使用单字词库\"{path}\"";
                var msg3 = $"覆盖{_encoder.Chars}个单字";
                AuditLogger.Log($"{msg1}，{msg2}，{msg3}", null);
                ShowInfo("成功", $"{msg1}\n{msg2}\n{msg3}");
            });

    [ObservableProperty] private byte _maxLen = 4, _minLen = 4, _curLen = 4;
    partial void OnCurLenChanged(byte value) => UpdateAutoCodes(false);

    /// <summary> 自动编码，填充FullAutoCodes与AutoCodes </summary>
    private void UpdateAutoCodes(bool changeFullCode) =>
        TryOrShowEx(
            "自动编码",
            () => {
                var oldSelected = SelectedAutoCode ?? "";
                AutoCodes.Clear();
                if (changeFullCode)
                    _fullAutoCodes = _encoder?.Encode(Word.Trim()).ToArray() ?? [];
                if (_fullAutoCodes.Length == 0)
                    return;

                var codes = MaxLen == MinLen || CurLen == MaxLen
                    ? _fullAutoCodes
                    : _fullAutoCodes.Select(code => code[..CurLen]).Distinct();
                foreach (var code in codes.Order())
                    AutoCodes.Add(code);

                SelectedAutoCode = AutoCodes.FirstOrDefault(code =>
                                       code.StartsWith(oldSelected, StringComparison.Ordinal)
                                    || oldSelected.StartsWith(code, StringComparison.Ordinal))
                                ?? AutoCodes[0];
                AutoCodeColor = AutoCodes.Count > 1
                    ? "Crimson"
                    : "Black";
            });

    #endregion 自动编码

    #region 搜索

    /// <summary> 搜索模式：0按编码前缀搜索，1按词组精准搜索 </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    private byte _searchMode;

    partial void OnSearchModeChanged(byte value) => UpdateSearchResults();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    private string _searchText = "";

    partial void OnSearchTextChanged(string value) => UpdateSearchResults();

    public ObservableCollection<MutEntry> SearchResults { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveCommand), nameof(ShortenCommand))]
    private MutEntry? _selectedSearchResult;

    /// <summary> 自动搜索当前字词或编码 </summary>
    private void AutoSearch() {
        if (SearchMode == 1)
            SearchText = Word.Trim();
        else if (CurCode is {} code)
            SearchText = code;
    }

    /// <summary> 搜索条目，填充SearchResults </summary>
    private void UpdateSearchResults() =>
        TryOrShowEx(
            "搜索条目",
            () => {
                SearchResults.Clear();
                var entries = _rimeDict is { Count: > 0 }
                    ? SearchMode == 0
                        ? _rimeDict.SearchByCode(SearchText, false)
                        : _rimeDict.SearchByWord(SearchText)
                    : [];
                foreach (var entry in entries.OrderBy(static e => e.Code))
                    SearchResults.Add(new(entry));
                ModifyCommand.NotifyCanExecuteChanged();
            });

    #endregion 搜索

    #region 词库操作

    private bool CanInsert => _rimeDict is {} && CurEntry.Type == 2;

    /// <summary> 将各属性添加为一个新条目 </summary>
    [RelayCommand(CanExecute = nameof(CanInsert))]
    private void Insert() =>
        TryOrShowEx(
            "添加条目",
            () => {
                var curEntry = CurEntry;

                var related = (curEntry.Code is {} // 非null则有码
                        ? _rimeDict!.SearchByWord(curEntry.Word!)
                            .Union(_rimeDict.SearchByCode(curEntry.Code, true))
                        : _rimeDict!.SearchByWord(curEntry.Word!)).Select(static e => $"\"{e}\"")
                    .ToArray();
                if (related.Length > 0
                 && !ShowConfirm("警告", $"词库已有以下条目，是否仍要添加？\n{string.Join('\n', related)}"))
                    return;

                _rimeDict.Insert(curEntry);
                SaveCommand.NotifyCanExecuteChanged();
                AuditLogger.Log("添加", curEntry);
                if ((SearchMode == 0 && SearchText == (curEntry.Code ?? ""))
                 || (SearchMode == 1 && SearchText == curEntry.Word))
                    SearchResults.Add(new(curEntry));
                else
                    SearchText = SearchMode == 0
                        ? curEntry.Code ?? ""
                        : curEntry.Word!;
                ModifyCommand.NotifyCanExecuteChanged();
            });

    private bool CanRemove => _rimeDict is {} && SelectedSearchResult is {};

    /// <summary> 将表格里选中的条目删除 </summary>
    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove() =>
        TryOrShowEx(
            "删除条目",
            () => {
                var toRemove = SelectedSearchResult!.Src;
                if (!ShowConfirm("确认", $"确认删除\n\"{toRemove}\"？"))
                    return;
                _rimeDict!.Remove(toRemove);
                SaveCommand.NotifyCanExecuteChanged();
                AuditLogger.Log("删除", toRemove);
                SearchResults.Remove(SelectedSearchResult);
            });

    private bool CanShorten =>
        _rimeDict is {}
     && UseEncoder
     && MaxLen != MinLen
     && SearchMode == 0
     && SearchText.Length >= MinLen
     && SelectedSearchResult is {}
     && SelectedSearchResult.Src.Code?.Length > SearchText.Length;

    /// <summary> 将表格里选中条目的编码截短为搜索框中的编码；若有一个条目占用该编码，则自动加长其编码到最短的空闲位置 </summary>
    /// <remarks> 需开启自动编码和编码前缀搜索；仅用于变长编码方案 </remarks>
    [RelayCommand(CanExecute = nameof(CanShorten))]
    private void Shorten() =>
        TryOrShowEx(
            "截短编码",
            () => {
                var toShorten = SelectedSearchResult!;
                var shortened = toShorten.Src with { Code = SearchText };
                if (shortened.Type != 2)
                    throw new InvalidOperationException("编码截短后条目无效");

                var toLengthens = SearchResults.Where(me => me.Src.Code == SearchText).ToArray();
                if (toLengthens.Length > 1)
                    throw new InvalidOperationException("有多个条目占用短编码");
                var toLengthen = toLengthens.ElementAtOrDefault(0);
                var lengthened = toLengthen is {}
                    ? GetLengthened()
                    : null;

                var msg1 = $"\"{toShorten.Src}\"\t=>\t\"{shortened}\"";
                var msg2 = $"\"{toLengthen?.Src}\"\t=>\t\"{lengthened}\"";
                if (toLengthen is null
                    ? !ShowConfirm("确认", $"确认以下修改？\n{msg1}")
                    : !ShowConfirm("确认", $"确认以下修改？\n{msg1}\n{msg2}"))
                    return;
                if (SearchResults.Any(me =>
                        me.Src.Code!.Length > toShorten.Src.Code!.Length
                     && me.Src.Code.StartsWith(toShorten.Src.Code, StringComparison.Ordinal))
                 && !ShowConfirm("提示", "选中条目的编码将会空缺，是否仍要截短？"))
                    return;

                _rimeDict!.Remove(toShorten.Src);
                AuditLogger.Log("截短前", toShorten.Src);
                SearchResults.Remove(toShorten);
                _rimeDict.Insert(shortened);
                AuditLogger.Log("截短后", shortened);
                SearchResults.Add(new(shortened));
                if (toLengthen is {}) {
                    _rimeDict.Remove(toLengthen.Src);
                    AuditLogger.Log("加长前", toLengthen.Src);
                    SearchResults.Remove(toLengthen);
                    _rimeDict.Insert(lengthened!);
                    AuditLogger.Log("加长后", lengthened!);
                    SearchResults.Add(new(lengthened!));
                }
                SaveCommand.NotifyCanExecuteChanged();

                Line GetLengthened() {
                    var fullCodes = _encoder!.Encode(toLengthen.Src.Word!)
                        .Where(code => code.StartsWith(SearchText, StringComparison.Ordinal))
                        .ToArray();
                    if (fullCodes.Length == 0)
                        throw new InvalidOperationException("占位条目的长编码不匹配");
                    var longCode = fullCodes.Select(code => code[..toShorten.Src.Code!.Length])
                        .Contains(toShorten.Src.Code)
                        ? toShorten.Src.Code // 直接交换编码
                        : GetLongCode(fullCodes);
                    return toLengthen.Src with { Code = longCode } is { Type: 2 } result
                        ? result
                        : throw new InvalidOperationException("占位条目加长后无效");
                }

                string GetLongCode(string[] fullCodes) {
                    for (var len = SearchText.Length + 1; len <= MaxLen; len++) {
                        var codes = fullCodes.Select(code => code[..len]).Distinct().ToArray();
                        if (codes.Length > 1)
                            throw new InvalidOperationException("占位条目的长编码不唯一");
                        if (_rimeDict!.SearchByCode(codes[0], true).Count == 0)
                            return codes[0];
                    }
                    throw new InvalidOperationException("占位条目没有空闲的长编码");
                }
            });

    private bool CanModify => _rimeDict is {} && SearchResults.Count > 0;

    /// <summary> 应用在表格中的改动 </summary>
    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify() =>
        TryOrShowEx(
            "应用修改",
            () => {
                List<(MutEntry Old, Line New)> mods = new(SearchResults.Count);
                foreach (var me in SearchResults)
                    if (me.ToNewEntry(out var entry))
                        mods.Add((me, entry!));
                if (mods.Count == 0)
                    throw new InvalidOperationException("没有有效的改动，什么都没做");

                var msg = mods.Select(static mod => $"\"{mod.Old}\"\t=>\t\"{mod.New}\"");
                if (!ShowConfirm("确认", $"确认以下修改？\n{string.Join('\n', msg)}"))
                    return;

                foreach (var mod in mods) {
                    _rimeDict!.Remove(mod.Old.Src);
                    AuditLogger.Log("改前", mod.Old.Src);
                    SearchResults.Remove(mod.Old);
                    _rimeDict.Insert(mod.New);
                    AuditLogger.Log("改后", mod.New);
                    SearchResults.Add(new(mod.New));
                }
                SaveCommand.NotifyCanExecuteChanged();
            });

    #endregion 词库操作
}
