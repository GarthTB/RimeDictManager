// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;
using ZLinq;
using static Services.DictManager;
using OpEx = InvalidOperationException;

public sealed partial class MainWindowVM: ObservableObject {
    #region 可用性

    [ObservableProperty,
     NotifyCanExecuteChangedFor(
         nameof(InsertCommand),
         nameof(RemoveCommand),
         nameof(ShortenCommand),
         nameof(ModifyCommand))]
    public partial bool DictReady { get; private set; }

    [ObservableProperty] public partial bool UseCodeBox { get; private set; }
    [ObservableProperty] public partial bool UseWeightBox { get; private set; }
    [ObservableProperty] public partial bool UseStemBox { get; private set; }
    [ObservableProperty] public partial bool ShowCodeCol { get; private set; }
    [ObservableProperty] public partial bool ShowWeightCol { get; private set; }
    [ObservableProperty] public partial bool ShowStemCol { get; private set; }
    public bool CanToggleEncoder => DictReady && Encoder.Ready;
    public static byte MinCodeLen => Encoder.Method.MinLen;
    public static byte MaxCodeLen => Encoder.Method.MaxLen;

    public void RefreshState() {
        DictReady = Ready;
        var tgtCols = TgtCols;
        UseCodeBox = DictReady && tgtCols?.Contains(DictCol.Code) == true;
        UseWeightBox = DictReady && tgtCols?.Contains(DictCol.Weight) == true;
        UseStemBox = DictReady && tgtCols?.Contains(DictCol.Stem) == true;
        var unionCols = UnionCols;
        ShowCodeCol = DictReady && unionCols.Contains(DictCol.Code);
        ShowWeightCol = DictReady && unionCols.Contains(DictCol.Weight);
        ShowStemCol = DictReady && unionCols.Contains(DictCol.Stem);
        UseEncoder = Encoder.Ready;
        OnPropertyChanged(nameof(CanToggleEncoder));
        OnPropertyChanged(nameof(MinCodeLen));
        OnPropertyChanged(nameof(MaxCodeLen));
        _ = UpdateAutoCodesAsync(true);
        _ = SearchAsync();
    }

    #endregion 可用性

    #region 词条

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(InsertCommand))]
    public partial string PendingText { get; set; } = "";

    [ObservableProperty] public partial string ManualCode { get; set; } = "";
    [ObservableProperty] public partial string PendingWeight { get; set; } = "";
    [ObservableProperty] public partial string PendingStem { get; set; } = "";

    private string PendingCode =>
        Encoder.Ready && UseEncoder
            ? SelAutoCode ?? ""
            : ManualCode;

    partial void OnPendingTextChanged(string value) {
        SyncSearchText();
        _ = UpdateAutoCodesAsync(true);
    }

    partial void OnManualCodeChanged(string value) => SyncSearchText();

    #endregion 词条

    #region 自动编码

    [ObservableProperty] public partial bool UseEncoder { get; set; }
    [ObservableProperty] public partial byte CurCodeLen { get; set; }
    private readonly List<string> _fullCodes = new(64);
    public ObservableCollection<string> AutoCodes { get; } = [];
    [ObservableProperty] public partial string? SelAutoCode { get; set; }
    public bool MultiAutoCodes => AutoCodes.Count > 1;

    partial void OnUseEncoderChanged(bool value) {
        SyncSearchText();
        _ = UpdateAutoCodesAsync(true);
    }

    partial void OnCurCodeLenChanged(byte value) => _ = UpdateAutoCodesAsync(false);
    partial void OnSelAutoCodeChanged(string? value) => SyncSearchText();

    private async Task UpdateAutoCodesAsync(bool needEncode) {
        try {
            if (!Encoder.Ready || !UseEncoder) return;

            if (needEncode) {
                _fullCodes.Clear();
                _fullCodes.AddRange(Encoder.Encode(PendingText));
            }
            var prev = SelAutoCode;
            AutoCodes.Clear();
            if (CurCodeLen < MaxCodeLen) {
                var shortCodes = _fullCodes.AsValueEnumerable().Select(s => s[..CurCodeLen]);
                foreach (var code in shortCodes.Distinct().Order()) AutoCodes.Add(code);
            } else
                foreach (var code in _fullCodes.AsValueEnumerable().Order())
                    AutoCodes.Add(code);

            if (AutoCodes is [var first, ..])
                SelAutoCode = prev is {} p && Math.Min(p.Length, CurCodeLen) is var len
                    ? AutoCodes.FirstOrDefault(s => s[..len] == p[..len], first)
                    : first;
        } catch (Exception ex) {
            _fullCodes.Clear();
            AutoCodes.Clear();
            await ex.AlertAsync("自动编码");
        } finally { OnPropertyChanged(nameof(MultiAutoCodes)); }
    }

    #endregion 自动编码

    #region 搜索

    public static Dictionary<SearchMode, string> SearchModes =>
        Enum.GetValues<SearchMode>().ToDictionary(static x => x, static x => x.ToString());

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    public partial SearchMode? SelSearchMode { get; set; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    public partial string SearchText { get; set; } = "";

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial ObservableCollection<EntryVM>? SearchResults { get; private set; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveCommand), nameof(ShortenCommand))]
    public partial EntryVM? SelSearchResult { get; set; }

    partial void OnSelSearchModeChanged(SearchMode? value) => SyncSearchText();
    partial void OnSearchTextChanged(string value) => _ = SearchAsync();

    private void SyncSearchText() {
        if (SelSearchMode == SearchMode.编码前缀)
            SearchText = PendingCode;
        else if (SelSearchMode == SearchMode.文本精确) SearchText = PendingText;
    }

    private async Task SearchAsync() {
        try {
            if (string.IsNullOrWhiteSpace(SearchText))
                SearchResults = null;
            else if (DictReady && SelSearchMode is {} mode)
                SearchResults = new(Search(SearchText, mode).Select(static x => new EntryVM(x)));
        } catch (Exception ex) {
            SearchResults = null;
            await ex.AlertAsync("搜索");
        } finally { ModifyCommand.NotifyCanExecuteChanged(); }
    }

    #endregion 搜索

    #region 操作

    private bool CanInsert => DictReady && !string.IsNullOrWhiteSpace(PendingText);

    [RelayCommand(CanExecute = nameof(CanInsert))]
    private async Task InsertAsync() {
        try {
            var e = await InsertEntryAsync(PendingText, PendingCode, PendingWeight, PendingStem);
            if (e is not {} v) return;
            var prev = SearchText;
            SyncSearchText();
            if (prev == SearchText) SearchResults?.Add(new(v));
        } catch (Exception ex) { await ex.AlertAsync("添加词条"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRemove => DictReady && SelSearchResult is {};

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task RemoveAsync() {
        try {
            var e = SelSearchResult ?? throw new OpEx("UI 错误");
            var done = await RemoveEntryAsync(e.Src);
            if (done && SearchResults?.Remove(e) == false) throw new OpEx("UI 删除词条失败");
        } catch (Exception ex) { await ex.AlertAsync("删除词条"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanShorten =>
        DictReady
     && Encoder.Ready
     && MinCodeLen < MaxCodeLen
     && SelSearchMode == SearchMode.编码前缀
     && SearchText.Length >= MinCodeLen
     && SelSearchResult?.Code.Length > SearchText.Length;

    [RelayCommand(CanExecute = nameof(CanShorten))]
    private async Task ShortenAsync() {
        try {
            var e = SelSearchResult ?? throw new OpEx("UI 错误");
            if (await ShortenEntryAsync(e.Src, SearchText)) _ = SearchAsync();
        } catch (Exception ex) { await ex.AlertAsync("截短编码"); }
    }

    private bool CanModify => DictReady && SearchResults?.Count > 0;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task ModifyAsync() {
        try {
            var entries = SearchResults ?? throw new OpEx("UI 错误");
            List<(DictEntry Src, EntryLine Tgt)> mods = new(entries.Count);
            foreach (var e in entries)
                if (e.TryNewIfModified(out var tgt))
                    mods.Add((e.Src, tgt));
            if (mods.Count == 0) throw new OpEx("没有改动，什么都没做");
            if (await ModifyEntriesAsync(mods)) _ = SearchAsync();
        } catch (Exception ex) { await ex.AlertAsync("应用修改"); }
    }

    #endregion 操作
}
