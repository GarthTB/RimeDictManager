namespace RimeDictManager.ViewModels;

using System.Collections.Frozen;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services.Core;
using Services.Data;

public sealed partial class MainWindowVM: ObservableObject {
    #region 可用性

    [ObservableProperty] public partial bool DictReady { get; private set; }
    [ObservableProperty] public partial bool HasCode { get; private set; }
    [ObservableProperty] public partial bool HasWeight { get; private set; }
    [ObservableProperty] public partial bool HasStem { get; private set; }
    [ObservableProperty] public partial bool EncoderToggleEnabled { get; private set; }

    public void RefreshState() {
        DictReady = DictManager.Ready;
        var cols = DictManager.IntersectCols;
        HasCode = DictReady && cols.Contains(Col.Code);
        HasWeight = DictReady && cols.Contains(Col.Weight);
        HasStem = DictReady && cols.Contains(Col.Stem);
        EncoderToggleEnabled = DictReady && Encoder.Ready;
    }

    #endregion 可用性

    #region 词条

    [ObservableProperty] public partial string PendingText { get; set; } = "";
    [ObservableProperty] public partial string ManualCode { get; set; } = "";
    [ObservableProperty] public partial string PendingWeight { get; set; } = "";
    [ObservableProperty] public partial string PendingStem { get; set; } = "";

    #endregion 词条

    #region 自动编码

    [ObservableProperty] public partial bool UseEncoder { get; set; }
    public static byte MinCodeLen => Encoder.Method.MinLen;
    public static byte MaxCodeLen => Encoder.Method.MaxLen;
    [ObservableProperty] public partial byte CurCodeLen { get; set; }
    private readonly List<string> _fullAutoCodes = new(64);
    public ObservableCollection<string> AutoCodes { get; } = [];
    [ObservableProperty] public partial string? SelAutoCode { get; set; }

    #endregion 自动编码

    #region 搜索

    public static IReadOnlyDictionary<DictManager.SearchMode, string> SearchModes { get; } = Enum
        .GetValues<DictManager.SearchMode>()
        .ToFrozenDictionary(static x => x, static x => x.ToString());

    [ObservableProperty]
    public partial DictManager.SearchMode SelSearchMode { get; set; } = DictManager.SearchMode.编码前缀;

    [ObservableProperty] public partial string SearchText { get; set; } = "";
    public ObservableCollection<EntryInfo> SearchResults { get; } = [];
    [ObservableProperty] public partial EntryInfo? SelSearchResult { get; set; }

    #endregion 搜索

    #region 操作

    [RelayCommand] private Task Insert() => throw new NotImplementedException();
    [RelayCommand] private Task Remove() => throw new NotImplementedException();
    [RelayCommand] private Task Shorten() => throw new NotImplementedException();
    [RelayCommand] private Task Modify() => throw new NotImplementedException();

    #endregion 操作
}
