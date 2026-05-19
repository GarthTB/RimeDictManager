// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.VMs;

using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Models;
using static System.Windows.MessageBoxResult;
using static MsgBox;
using static Services.LineCodec;
using static Services.Logger;
using NeverEx = System.Diagnostics.UnreachableException;
using OpEx = InvalidOperationException;

internal sealed partial class MainVM: ObservableObject {
    private const string DictFileFilter = "RIME 词库|*.dict.yaml|所有文件|*.*";

    #region 词库文件

    [ObservableProperty] private partial Dict? Dict { get; set; }
    private bool DictModified => Dict is { Modified: true };
    public bool EnsureSaved => !DictModified || ShowConfirm("警告", "丢弃未保存的改动？") == Yes;

    [RelayCommand]
    private void Open() {
        if (!EnsureSaved) return;
        var ofd = new OpenFileDialog { Title = "打开 RIME 词库", Filter = DictFileFilter };
        if (ofd.ShowDialog() == true) LoadDict(ofd.FileName);
    }

    public void LoadDict(string path) {
        try {
            Dict = new(path, SaveCommand.NotifyCanExecuteChanged);
            Search();
            LogAndShowSuccess("词库已载入", $"路径：{path}", $"总词条数：{Dict.Count}");
        } catch (Exception ex) {
            Dict = null;
            LogAndShowErr($"加载词库时：\n{ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(DictModified))]
    private void Save() {
        const string overwritePrompt = "是：覆写原词库\n否：另存副本",
            reorderPrompt = "是：词条先按编码升序，再按原行号升序重排，空行丢弃，注释原序排在末尾\n否：保持原有行，新词条按编码升序排在末尾";

        var overwrite = ShowConfirm("选择策略", overwritePrompt);
        if (overwrite is not (Yes or No)) return;
        string? path = null;
        if (overwrite == No) {
            var sfd = new SaveFileDialog { Title = "将词库另存至...", Filter = DictFileFilter };
            if (sfd.ShowDialog() != true) return;
            path = sfd.FileName;
        }
        var reorder = ShowConfirm("选择策略", reorderPrompt);
        if (reorder is not (Yes or No)) return;

        try {
            Dict!.Save(path, reorder == Yes);
            var msg = path is {}
                ? $"路径：{path}"
                : "覆写原文件";
            LogAndShowSuccess("词库已保存", msg, $"总词条数：{Dict.Count}");
        } catch (Exception ex) { LogAndShowErr($"保存词库时：\n{ex}"); }
    }

    #endregion 词库文件

    #region 编码器

    public static IReadOnlyList<string> EncodeMethods => Encoder.Methods;

    public bool EncoderEnabled {
        get;
        set {
            if (field == value) return;
            if (value && Encoder is null && SelEncodeMethod is {}) {
                OpenSingle();
                if (Encoder is null) {
                    OnPropertyChanged();
                    return;
                }
            }
            SetProperty(ref field, value);
            UpdatePendingEntry();
        }
    }

    public string? SelEncodeMethod {
        get;
        set {
            if (field == value) return;
            if (value is {}) {
                var title = $"打开'{value}'方案的单字码表";
                var ofd = new OpenFileDialog { Title = title, Filter = DictFileFilter };
                if (ofd.ShowDialog() == true)
                    LoadSingle(ofd.FileName, value);
                else
                    EncoderEnabled = false;
            }
            SetProperty(ref field, value);
        }
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CodeLenMin), nameof(CodeLenMax))]
    private partial Encoder? Encoder { get; set; }

    private bool EncoderReady => Encoder is {};
    public byte CodeLenMin => Encoder?.CodeLenMin ?? 0;
    public byte CodeLenMax => Encoder?.CodeLenMax ?? 0;
    [ObservableProperty] public partial byte CodeLen { get; set; }
    partial void OnCodeLenChanged(byte value) => UpdateAutoCodes(false);

    [RelayCommand(CanExecute = nameof(EncoderReady))]
    private void OpenSingle() {
        var title = $"打开'{SelEncodeMethod}'方案的单字码表";
        var ofd = new OpenFileDialog { Title = title, Filter = DictFileFilter };
        if (ofd.ShowDialog() == true) LoadSingle(ofd.FileName);
    }

    public void LoadSingle(string path, string? newMethod = null) {
        try {
            var method = SelEncodeMethod ?? newMethod ?? throw new NeverEx("程序内部不一致，请停用并报告异常B");
            Encoder = new(method, path);
            CodeLen = Encoder.CodeLenMin;
            UpdateAutoCodes(true);
            var msg = $"'{method}'方案的单字码表已载入";
            LogAndShowSuccess(msg, $"路径：{path}", $"覆盖字数：{Encoder.CharCount}");
        } catch (Exception ex) {
            Encoder = null;
            EncoderEnabled = false;
            LogAndShowErr($"加载单字码表时：\n{ex}");
        }
    }

    #endregion 编码器

    #region 词条字段

    [ObservableProperty] public partial string PendingText { get; set; } = "";
    [ObservableProperty] public partial string ManualCode { get; set; } = "";
    [ObservableProperty] public partial string PendingWeight { get; set; } = "";
    [ObservableProperty] public partial string PendingStem { get; set; } = "";
    [ObservableProperty] private partial Entry? PendingEntry { get; set; }

    private readonly List<string> _fullAutoCodes = new(64);
    public ObservableCollection<string> AutoCodes { get; } = [];
    [ObservableProperty] public partial string? SelAutoCode { get; set; }
    [ObservableProperty] public partial string AutoCodeColor { get; private set; } = "";

    partial void OnPendingTextChanged(string value) {
        if (Encoder is {}) UpdateAutoCodes(true);
        UpdatePendingEntry();
    }

    partial void OnManualCodeChanged(string value) => UpdatePendingEntry();
    partial void OnSelAutoCodeChanged(string? value) => UpdatePendingEntry();
    partial void OnPendingWeightChanged(string value) => UpdatePendingEntry();
    partial void OnPendingStemChanged(string value) => UpdatePendingEntry();
    partial void OnPendingEntryChanged(Entry? value) => SyncSearchText();

    private void UpdatePendingEntry() =>
        PendingEntry = EncoderEnabled && EncoderReady
            ? TryNewEntry(PendingText, SelAutoCode, PendingWeight, PendingStem)
            : TryNewEntry(PendingText, ManualCode, PendingWeight, PendingStem);

    private void UpdateAutoCodes(bool needEncode) {
        try {
            if (needEncode) {
                _fullAutoCodes.Clear();
                _fullAutoCodes.AddRange(Encoder!.Encode(PendingText));
            }
            var codes = CodeLen < CodeLenMax
                ? _fullAutoCodes.Select(s => s[..CodeLen]).Distinct()
                : _fullAutoCodes;

            var oldSel = SelAutoCode;
            AutoCodes.Clear();
            foreach (var code in codes.Order()) AutoCodes.Add(code);

            if (AutoCodes.Count > 0)
                SelAutoCode = oldSel is {} o && Math.Min(o.Length, CodeLen) is var len
                    ? AutoCodes.FirstOrDefault(s => s[..len] == o[..len], AutoCodes[0])
                    : AutoCodes[0];
        } catch (Exception ex) {
            _fullAutoCodes.Clear();
            AutoCodes.Clear();
            LogAndShowErr($"自动编码时：\n{ex}");
        }

        AutoCodeColor = AutoCodes.Count > 1
            ? "Red"
            : "";
    }

    #endregion 词条字段

    #region 搜索

    [ObservableProperty] public partial bool IsSearchByCode { get; set; } = true;
    [ObservableProperty] public partial string SearchText { get; set; } = "";
    public ObservableCollection<MutEntry> SearchResults { get; } = [];
    [ObservableProperty] public partial MutEntry? SelSearchResult { get; set; }
    partial void OnIsSearchByCodeChanged(bool value) => SyncSearchText();
    partial void OnSearchTextChanged(string value) => Search();

    private void SyncSearchText() =>
        SearchText = PendingEntry is {} e
            ? IsSearchByCode
                ? e.Code ?? ""
                : e.Text
            : "";

    private void Search() {
        SearchResults.Clear();
        try {
            if (Dict is not { Count: > 0 }) return;
            if (!IsSearchByCode)
                Dict.ForEachByText(SearchText, e => SearchResults.Add(new(e)));
            else if (!string.IsNullOrEmpty(SearchText)) // 禁止前缀匹配整个Trie
                Dict.ForEachByCode(SearchText, false, e => SearchResults.Add(new(e)));
        } catch (Exception ex) { LogAndShowErr($"搜索词条时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion 搜索

    #region 操作

    private bool CanInsert => Dict is {} && PendingEntry is {};

    [RelayCommand(CanExecute = nameof(CanInsert))]
    private void Insert() {
        try {
            var entry = PendingEntry!.Value;

            List<Entry> related = [];
            Dict!.ForEachByText(entry.Text, related.Add);
            if (entry.Code is {} code) Dict!.ForEachByCode(code, true, related.Add);
            if (related.Count > 0) {
                var msg = string.Join('\n', related.Select(Serialize));
                if (ShowConfirm("确认", $"已有以下相关词条，仍要添加？\n{msg}") != Yes) return;
            }

            Dict.Insert(entry);
            Log("添加", entry);

            var tmp = SearchText;
            SyncSearchText();
            if (tmp != SearchText) return;
            SearchResults.Add(new(entry));
        } catch (Exception ex) { LogAndShowErr($"添加词条时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRemove => Dict is {} && SelSearchResult is {};

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove() {
        try {
            var entry = SelSearchResult!.Src;

            var msg = $"确认删除？\n{Serialize(entry)}";
            if (entry.Code is {} code && Dict!.IsCodePrefix(code))
                msg += $"\n删除后，编码'{code}'将空缺，有长码可被截短";
            if (ShowConfirm("确认", msg) != Yes) return;

            if (!Dict!.Remove(entry)) throw new OpEx("找不到要删除的词条");
            Log("删除", entry);

            SearchResults.Remove(SelSearchResult);
        } catch (Exception ex) { LogAndShowErr($"删除词条时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanShorten =>
        Dict is {}
     && Encoder is { CodeLenMin: var min, CodeLenMax: var max }
     && min < max
     && IsSearchByCode
     && SearchText.Length >= min
     && SelSearchResult is {} e
     && e.Code.Length > SearchText.Length;

    [RelayCommand(CanExecute = nameof(CanShorten))]
    private void Shorten() {
        try {
            var oldLong = SelSearchResult!;
            var newShort = TryNewEntry(oldLong.Text, SearchText, oldLong.Weight, oldLong.Stem)
                        ?? throw new OpEx("新短码无效");

            var oldShorts = SearchResults.Where(me => me.Code == SearchText).ToArray();
            if (oldShorts.Length > 1) throw new OpEx("占用短码的词条不唯一");
            (MutEntry, Entry)? oldShortNewLong = oldShorts.FirstOrDefault() is {} os1
                ? (os1, Lengthen(os1, oldLong.Code))
                : null;

            List<string> msg = new(5) {
                $"删除：'{Serialize(oldLong.Src)}'", $"改为：'{Serialize(newShort)}'"
            };
            if (oldShortNewLong is var (os2, nl2))
                msg.AddRange([$"删除：'{Serialize(os2.Src)}'", $"改为：'{Serialize(nl2)}'"]);
            if ((oldShortNewLong is not var (_, nl3) || oldLong.Code != nl3.Code)
             && Dict!.IsCodePrefix(oldLong.Code))
                msg.Add($"截短后，编码'{oldLong.Code}'将空缺，有更长编码可被截短");
            if (ShowConfirm("确认", $"确认修改？\n{string.Join('\n', msg)}") != Yes) return;

            Replace(oldLong, newShort);
            if (oldShortNewLong is var (os4, nl4)) Replace(os4, nl4);
        } catch (Exception ex) { LogAndShowErr($"截短编码时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }

        Entry Lengthen(MutEntry oldShort, string oldLongCode) {
            var fullCodes = Encoder!.Encode(oldShort.Text)
                .Where(s => s.AsSpan().StartsWith(SearchText))
                .ToArray();
            if (fullCodes.Length == 0) throw new OpEx("找不到匹配的新长码");
            var newLongCode = fullCodes.Any(s => s.AsSpan().StartsWith(oldLongCode))
                ? oldLongCode // 直接交换编码
                : Determine(fullCodes);
            return TryNewEntry(oldShort.Text, newLongCode, oldShort.Weight, oldShort.Stem)
                ?? throw new OpEx("新长码无效");
        }

        string Determine(string[] fullCodes) {
            for (var len = SearchText.Length + 1; len <= CodeLenMax; len++) {
                var codes = fullCodes.Select(s => s[..len]).Distinct().ToArray();
                if (codes.Length > 1) throw new OpEx("新长码不唯一");
                if (!Dict!.ContainsCode(codes[0])) return codes[0];
            }
            throw new OpEx("没有空闲的新长码");
        }
    }

    private bool CanModify => Dict is {} && SearchResults.Count > 0;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify() {
        try {
            List<(MutEntry Bef, Entry Aft)> mods = new(SearchResults.Count);
            foreach (var me in SearchResults)
                if (me.TryNewIfModified() is {} aft)
                    mods.Add((me, aft));
            if (mods.Count == 0) throw new OpEx("没有改动，什么都没做");

            List<string> msg = new(mods.Count * 3);
            foreach (var (bef, aft) in mods) {
                msg.AddRange([$"删除：'{Serialize(bef.Src)}'", $"改为：'{Serialize(aft)}'"]);
                if (bef.Src.Code is {} code
                 && code != aft.Code
                 && mods.All(mod => mod.Aft.Code != code)
                 && Dict!.IsCodePrefix(code))
                    msg.Add($"修改后，编码'{code}'将空缺，有更长编码可被截短");
            }
            if (ShowConfirm("确认", $"确认修改？\n{string.Join('\n', msg)}") != Yes) return;

            foreach (var (bef, aft) in mods) Replace(bef, aft);
        } catch (Exception ex) { LogAndShowErr($"应用修改时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private void Replace(MutEntry o, Entry n) {
        Dict!.Remove(o.Src);
        Log("删除", o.Src);
        if (!SearchResults.Remove(o)) throw new OpEx("找不到要删除的词条");
        Dict.Insert(n);
        Log("改为", n);
        SearchResults.Add(new(n));
    }

    #endregion 操作

    #region 响应

    private static void LogAndShowSuccess(params string[] msg) {
        Log(string.Join('，', msg), null);
        ShowInfo("成功", string.Join('\n', msg));
    }

    private static void LogAndShowErr(string msg) {
        Log(msg, null);
        ShowErr(msg);
    }

    private readonly Dictionary<string, IRelayCommand[]> _dependencies;

    public MainVM() =>
        _dependencies = new() {
            [nameof(Dict)]
                = [SaveCommand, InsertCommand, RemoveCommand, ShortenCommand, ModifyCommand],
            [nameof(Encoder)] = [OpenSingleCommand, ShortenCommand],
            [nameof(PendingEntry)] = [InsertCommand],
            [nameof(IsSearchByCode)] = [ShortenCommand],
            [nameof(SearchText)] = [ShortenCommand],
            [nameof(SelSearchResult)] = [RemoveCommand, ShortenCommand]
        };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);
        if (e.PropertyName is not {} n || !_dependencies.TryGetValue(n, out var commands)) return;
        foreach (var cmd in commands) cmd.NotifyCanExecuteChanged();
    }

    #endregion 响应
}
