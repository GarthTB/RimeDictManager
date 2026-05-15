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
            if (sfd.ShowDialog() == true) path = sfd.FileName;
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
                var title = $"打开'{SelEncodeMethod}'方案的单字码表";
                var ofd = new OpenFileDialog { Title = title, Filter = DictFileFilter };
                if (ofd.ShowDialog() == true)
                    LoadSingle(ofd.FileName);
                else {
                    OnPropertyChanged();
                    return;
                }
            }
            SetProperty(ref field, value);
        }
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CodeLenMin), nameof(CodeLenMax))]
    private partial Encoder? Encoder { get; set; }

    public byte CodeLenMin => Encoder?.CodeLenMin ?? 0;
    public byte CodeLenMax => Encoder?.CodeLenMax ?? 0;
    [ObservableProperty] public partial byte CodeLen { get; set; }
    partial void OnCodeLenChanged(byte value) => UpdateAutoCodes(false);

    [RelayCommand(CanExecute = nameof(EncoderEnabled))]
    private void OpenSingle() {
        var title = $"打开'{SelEncodeMethod}'方案的单字码表";
        var ofd = new OpenFileDialog { Title = title, Filter = DictFileFilter };
        if (ofd.ShowDialog() == true) LoadSingle(ofd.FileName);
    }

    public void LoadSingle(string path) {
        try {
            Encoder = new(SelEncodeMethod!, path);
            CodeLen = Encoder.CodeLenMin;
            UpdateAutoCodes(true);
            var msg = $"'{SelEncodeMethod}'方案的单字码表已载入";
            LogAndShowSuccess(msg, $"路径：{path}", $"覆盖字数：{Encoder.CharCount}");
        } catch (Exception ex) {
            Encoder = null;
            EncoderEnabled = false;
            LogAndShowErr($"加载单字码表时：\n{ex}");
        }
    }

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
                SelAutoCode = oldSel is {} && Math.Min(oldSel.Length, CodeLen) is var len
                    ? AutoCodes.FirstOrDefault(s => s[..len] == oldSel[..len], AutoCodes[0])
                    : AutoCodes[0];
        } catch (Exception ex) {
            _fullAutoCodes.Clear();
            AutoCodes.Clear();
            LogAndShowErr($"自动编码时：\n{ex}");
        } finally {
            AutoCodeColor = AutoCodes.Count > 1
                ? "Red"
                : "";
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
        PendingEntry = EncoderEnabled
            ? TryNewEntry(PendingText, SelAutoCode, PendingWeight, PendingStem)
            : TryNewEntry(PendingText, ManualCode, PendingWeight, PendingStem);

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
                Dict.ForEachByText(SearchText, AddResult);
            else if (!string.IsNullOrEmpty(SearchText)) // 禁止前缀匹配整个Trie
                Dict.ForEachByCode(SearchText, false, AddResult);
        } catch (Exception ex) { LogAndShowErr($"搜索词条时：\n{ex}"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool AddResult(Entry e) {
        SearchResults.Add(new(e));
        return true;
    }

    #endregion 搜索

    #region 操作

    private bool CanInsert => Dict is {} && PendingEntry is {};

    [RelayCommand(CanExecute = nameof(CanInsert))]
    private void Insert() => throw new NotImplementedException();

    private bool CanRemove => Dict is {} && SelSearchResult is {};

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove() => throw new NotImplementedException();

    private bool CanShorten =>
        Dict is {}
     && Encoder is { CodeLenMin: var min, CodeLenMax: var max }
     && min < max
     && IsSearchByCode
     && SearchText.Length >= min
     && SelSearchResult is {} e
     && e.Code.Length > SearchText.Length;

    [RelayCommand(CanExecute = nameof(CanShorten))]
    private void Shorten() => throw new NotImplementedException();

    private bool CanModify => Dict is {} && SearchResults.Count > 0;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify() => throw new NotImplementedException();

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
            [nameof(EncoderEnabled)] = [OpenSingleCommand],
            [nameof(Encoder)] = [ShortenCommand],
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
