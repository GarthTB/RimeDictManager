namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;
using static Services.DictManager;

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
        UseCodeBox = DictReady && tgtCols?.Contains(Column.Code) == true;
        UseWeightBox = DictReady && tgtCols?.Contains(Column.Weight) == true;
        UseStemBox = DictReady && tgtCols?.Contains(Column.Stem) == true;
        var unionCols = UnionCols;
        ShowCodeCol = DictReady && unionCols.Contains(Column.Code);
        ShowWeightCol = DictReady && unionCols.Contains(Column.Weight);
        ShowStemCol = DictReady && unionCols.Contains(Column.Stem);
        OnPropertyChanged(nameof(CanToggleEncoder));
        OnPropertyChanged(nameof(MinCodeLen));
        OnPropertyChanged(nameof(MaxCodeLen));
        ShortenCommand.NotifyCanExecuteChanged();
    }

    #endregion 可用性

    #region 词条

    [ObservableProperty] public partial string PendingText { get; set; } = "";
    [ObservableProperty] public partial string ManualCode { get; set; } = "";
    [ObservableProperty] public partial string PendingWeight { get; set; } = "";
    [ObservableProperty] public partial string PendingStem { get; set; } = "";

    private string? PendingCode =>
        Encoder.Ready && UseEncoder
            ? SelAutoCode
            : ManualCode;

    #endregion 词条

    #region 自动编码

    [ObservableProperty] public partial bool UseEncoder { get; set; }
    [ObservableProperty] public partial byte CurCodeLen { get; set; }
    private readonly List<string> _fullAutoCodes = new(64);
    public ObservableCollection<string> AutoCodes { get; } = [];
    [ObservableProperty] public partial string? SelAutoCode { get; set; }

    #endregion 自动编码

    #region 搜索

    public static Dictionary<SearchMode, string> SearchModes { get; } = Enum.GetValues<SearchMode>()
        .ToDictionary(static x => x, static x => x.ToString());

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    public partial SearchMode SelSearchMode { get; set; } = SearchMode.编码前缀;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ShortenCommand))]
    public partial string SearchText { get; set; } = "";

    public ObservableCollection<EntryVM> SearchResults { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveCommand), nameof(ShortenCommand))]
    public partial EntryVM? SelSearchResult { get; set; }

    partial void OnSearchTextChanged(string value) => _ = SearchAsync();

    private void SyncSearchText() =>
        SearchText = SelSearchMode switch {
            SearchMode.编码前缀 => PendingCode ?? "",
            SearchMode.文本精确 => PendingText,
            _ => throw new UnreachableException()
        };

    private async Task SearchAsync() {
        try {
            SearchResults.Clear();
            Search(SearchText, SelSearchMode, e => SearchResults.Add(new(e)));
        } catch (Exception ex) { await ex.Alert("搜索"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion 搜索

    #region 操作

    private bool CanInsert => DictReady && PendingText.Length > 0;

    [RelayCommand(CanExecute = nameof(CanInsert))]
    private async Task InsertAsync() {
        try {
            var e = await InsertEntryAsync(PendingText, PendingCode, PendingWeight, PendingStem);
            if (e is null) return;
            var tmp = SearchText;
            SyncSearchText();
            if (tmp != SearchText) return;
            SearchResults.Add(new(e.Value));
        } catch (Exception ex) { await ex.Alert("添加词条"); } finally {
            ModifyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRemove => DictReady && SelSearchResult is {};

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task RemoveAsync() {
        try {
            var e = SelSearchResult!;
            if (await RemoveEntryAsync(e.Src)) SearchResults.Remove(e);
        } catch (Exception ex) { await ex.Alert("删除词条"); } finally {
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
            if (await ShortenEntryAsync(SelSearchResult!.Src, SearchText)) _ = SearchAsync();
        } catch (Exception ex) { await ex.Alert("截短编码"); }
    }

    private bool CanModify => DictReady && SearchResults.Count > 0;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task ModifyAsync() {
        try {
            List<(DictEntry Src, EntryLine Tgt)> mods = new(SearchResults.Count);
            foreach (var e in SearchResults)
                if (e.TryNewIfModified(out var tgt))
                    mods.Add((e.Src, tgt));
            if (mods.Count == 0) throw new InvalidOperationException("没有改动，什么都没做");
            if (await ModifyEntriesAsync(mods)) _ = SearchAsync();
        } catch (Exception ex) { await ex.Alert("应用修改"); }
    }

    #endregion 操作
}
