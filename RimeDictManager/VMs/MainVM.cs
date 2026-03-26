// ReSharper disable UnusedParameterInPartialMethod

namespace RimeDictManager.VMs;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Models;
using Services;
using static System.Windows.MessageBox;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;
using static StringComparison;
using IOE = InvalidOperationException;
using MBR = System.Windows.MessageBoxResult;

internal sealed partial class MainVM: ObservableObject {
    #region 词库文件

    private const string DictFileFilter = "RIME词库文件|*.dict.yaml|所有文件|*.*";
    [ObservableProperty] private partial Dict? Dict { get; set; }
    [ObservableProperty] private partial bool DictModified { get; set; }
    private bool DictExist => Dict is {};

    public bool UnmodOrDiscard =>
        !DictModified || Show("是否丢弃未保存的改动？", "警告", YesNo, Warning) == MBR.Yes;

    public void LoadDict(string path) {
        try {
            Dict = new(path, () => DictModified = Dict!.Modified);
            DictModified = false;
            Search();
            LogAndShowSuccess("已加载词库", $"路径'{path}'", $"总词条数'{Dict.Count}'");
        } catch (Exception ex) {
            Dict = null;
            DictModified = false;
            LogAndShowEx($"加载词库时：\n{ex}");
        }
    }

    [RelayCommand]
    private void Open() {
        if (!UnmodOrDiscard) return;
        var ofd = new OpenFileDialog { Title = "打开RIME词库", Filter = DictFileFilter };
        if (ofd.ShowDialog() == true) LoadDict(ofd.FileName);
    }

    [RelayCommand(CanExecute = nameof(DictModified))]
    private void Save(string? path) {
        const string notice = "是：词条先按编码升序再按原序（新词条后置），注释原序放在末尾，空行删除\n否：保留原序，新词条按编码升序放在末尾";
        try {
            var sort = Show(notice, "选择排序策略", YesNo, Question);
            if (sort is not (MBR.Yes or MBR.No)) return;
            Dict!.Save(path, sort == MBR.Yes);
            var msg = path is {}
                ? $"路径'{path}'"
                : "覆写原文件";
            LogAndShowSuccess("已保存词库", msg, $"总词条数'{Dict.Count}'");
        } catch (Exception ex) { LogAndShowEx($"保存词库时：\n{ex}"); }
    }

    [RelayCommand(CanExecute = nameof(DictExist))]
    private void SaveAs() {
        var sfd = new SaveFileDialog { Title = "将词库另存为...", Filter = DictFileFilter };
        if (sfd.ShowDialog() == true) Save(sfd.FileName);
    }

    #endregion 词库文件

    #region 编码器

    [ObservableProperty] public partial bool EncoderActive { get; set; }
    public static IReadOnlyList<string> EncoderMethods => Encoder.Methods;
    [ObservableProperty] public partial string? SelEncoderMethod { get; set; }
    [ObservableProperty] private partial Func<string, IEnumerable<string>>? Encode { get; set; }
    [ObservableProperty] public partial byte MinCodeLen { get; private set; }
    [ObservableProperty] public partial byte MaxCodeLen { get; private set; }
    [ObservableProperty] public partial byte CurCodeLen { get; set; }
    partial void OnCurCodeLenChanged(byte value) => UpdateAutoCodes(false);

    partial void OnEncoderActiveChanged(bool value) {
        SyncSearchText();
        if (!value || Encode is {} || SelEncoderMethod is null) return;
        PickCharsDict();
        EncoderActive = Encode is {};
    }

    partial void OnSelEncoderMethodChanged(string? value) {
        if (value is null) return;
        var ofd = new OpenFileDialog { Title = $"打开{SelEncoderMethod}码表", Filter = DictFileFilter };
        if (ofd.ShowDialog() == true)
            SetEncoder(ofd.FileName);
        else {
            Encode = null;
            EncoderActive = false;
        }
    }

    public void SetEncoder(string path) {
        try {
            var (min, max, encode, count) = Encoder.Create(SelEncoderMethod!, path);
            if (count == 0) throw new InvalidDataException("码表为空");
            (MinCodeLen, MaxCodeLen, CurCodeLen, Encode) = (min, max, min, encode);
            UpdateAutoCodes(true);
            LogAndShowSuccess($"已加载{SelEncoderMethod}码表", $"路径'{path}'", $"覆盖字数'{count}'");
        } catch (Exception ex) {
            Encode = null;
            EncoderActive = false;
            LogAndShowEx($"加载{SelEncoderMethod}码表时：\n{ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(EncoderActive))]
    private void PickCharsDict() {
        var ofd = new OpenFileDialog { Title = $"打开{SelEncoderMethod}码表", Filter = DictFileFilter };
        if (ofd.ShowDialog() == true) SetEncoder(ofd.FileName);
    }

    private void UpdateAutoCodes(bool needEncode) {
        try {
            var oldSel = SelAutoCode;

            AutoCodes.Clear();
            if (needEncode) _fullAutoCodes = Encode!(Word).ToArray();
            var codes = MaxCodeLen == MinCodeLen || CurCodeLen == MaxCodeLen
                ? _fullAutoCodes
                : _fullAutoCodes.Select(s => s[..CurCodeLen]).Distinct();
            foreach (var code in codes.Order()) AutoCodes.Add(code);

            if (AutoCodes.Count > 0)
                SelAutoCode = Math.Min(oldSel?.Length ?? 0, CurCodeLen) is var len and > 0
                    ? AutoCodes.FirstOrDefault(s => s[..len] == oldSel![..len], AutoCodes[0])
                    : AutoCodes[0];
            AutoCodeColor = AutoCodes.Count > 1
                ? "Red"
                : "";
        } catch (Exception ex) { LogAndShowEx($"自动编码时：\n{ex}"); }
    }

    #endregion 编码器

    #region 词条属性

    [ObservableProperty] public partial string Word { get; set; } = "";
    [ObservableProperty] public partial string ManualCode { get; set; } = "";
    [ObservableProperty] public partial string Weight { get; set; } = "";
    partial void OnManualCodeChanged(string value) => SyncSearchText();

    private string[] _fullAutoCodes = [];
    public ObservableCollection<string> AutoCodes { get; } = [];
    [ObservableProperty] public partial string? SelAutoCode { get; set; }
    [ObservableProperty] public partial string AutoCodeColor { get; private set; } = "";
    partial void OnSelAutoCodeChanged(string? value) => SyncSearchText();

    partial void OnWordChanged(string value) {
        if (EncoderActive) UpdateAutoCodes(true);
        SyncSearchText();
    }

    private string? CurCode =>
        EncoderActive
            ? SelAutoCode
            : ManualCode;

    private Entry? CurEntry => Entry.TryNew(Word, CurCode, Weight); // 内部会Trim

    #endregion 词条属性

    #region 搜索

    [ObservableProperty] public partial bool IsSearchByCode { get; set; } = true;
    [ObservableProperty] public partial string SearchText { get; set; } = "";
    public ObservableCollection<MutEntry> SearchResults { get; } = [];
    [ObservableProperty] public partial MutEntry? SelSearchResult { get; set; }
    partial void OnIsSearchByCodeChanged(bool value) => SyncSearchText();
    partial void OnSearchTextChanged(string value) => Search();

    private void SyncSearchText() =>
        SearchText = IsSearchByCode
            ? CurCode ?? ""
            : Word;

    private void Search() {
        try {
            SearchResults.Clear();
            if (Dict is { Count: > 0 } && !(IsSearchByCode && SearchText.Length == 0)) {
                var entries = IsSearchByCode
                    ? Dict.SearchByCode(SearchText, false)
                    : Dict.SearchByWord(SearchText);
                foreach (var e in entries.OrderBy(static e => e.Code)) SearchResults.Add(new(e));
            }
            ModifyCommand.NotifyCanExecuteChanged();
        } catch (Exception ex) { LogAndShowEx($"搜索词条时：\n{ex}"); }
    }

    #endregion 搜索

    #region 词库操作

    private bool CanInsert => Dict is {} && CurEntry is {};

    [RelayCommand(CanExecute = nameof(CanInsert))]
    private void Insert() {
        try {
            var entry = CurEntry!;

            var related = (entry.Code is {} code
                    ? Dict!.SearchByWord(entry.Word).Union(Dict.SearchByCode(code, true))
                    : Dict!.SearchByWord(entry.Word)).Select(static e => $"{e}")
                .ToArray();
            if (related.Length > 0) {
                var msg = $"已有以下相关词条，仍要添加？\n{string.Join('\n', related)}";
                if (Show(msg, "确认", YesNo, Question) != MBR.Yes) return;
            }

            Dict.Insert(entry);
            Logger.Log("添加", entry);

            var searchText = SearchText;
            SyncSearchText();
            if (searchText != SearchText) return;
            SearchResults.Add(new(entry));
            ModifyCommand.NotifyCanExecuteChanged();
        } catch (Exception ex) { LogAndShowEx($"添加词条时：\n{ex}"); }
    }

    private bool CanRemove => Dict is {} && SelSearchResult is { Modified: false };

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove() {
        try {
            var entry = SelSearchResult!.Src;

            var msg = $"确认删除？\n{entry}";
            if (entry.Code is {} code && Dict!.IsCodePrefix(code))
                msg += $"\n删除后，编码'{code}'将空缺，有长码可被截短";
            if (Show(msg, "确认", YesNo, Question) != MBR.Yes) return;

            Dict!.Remove(entry);
            Logger.Log("删除", entry);
            SearchResults.Remove(SelSearchResult);
            ModifyCommand.NotifyCanExecuteChanged();
        } catch (Exception ex) { LogAndShowEx($"删除词条时：\n{ex}"); }
    }

    private bool CanShorten =>
        Dict is {}
     && EncoderActive
     && MaxCodeLen > MinCodeLen
     && IsSearchByCode
     && SearchText.Length >= MinCodeLen
     && SelSearchResult is { Modified: false } entry
     && entry.Code.Length > SearchText.Length;

    [RelayCommand(CanExecute = nameof(CanShorten))]
    private void Shorten() {
        try {
            var oldLong = SelSearchResult!;
            var newShort = Entry.TryNew(oldLong.Word, SearchText, oldLong.Weight);
            if (newShort is null) throw new IOE("短码无效");

            var oldShorts = SearchResults.Where(me => me.Code == SearchText).ToArray();
            if (oldShorts.Length > 1) throw new IOE("占用短码的词条不唯一");
            var oldShort = oldShorts.FirstOrDefault();
            if (oldShort?.Modified == true) throw new IOE("占用短码的词条有改动");
            var newLong = oldShort is {}
                ? Lengthen()
                : null;

            List<string> messages = new(5) { $"删除：'{oldLong.Src}'", $"改为：'{newShort}'" };
            if (oldShort is {}) messages.AddRange([$"删除：'{oldShort.Src}'", $"改为：'{newLong}'"]);
            if (oldLong.Code != newLong?.Code && Dict!.IsCodePrefix(oldLong.Code))
                messages.Add($"截短后，编码'{oldLong.Code}'将空缺，有更长编码可被截短");
            var msg = $"确认修改？\n{string.Join('\n', messages)}";
            if (Show(msg, "确认", YesNo, Question) != MBR.Yes) return;

            Dict!.Remove(oldLong.Src);
            Logger.Log("删除", oldLong.Src);
            SearchResults.Remove(oldLong);
            Dict.Insert(newShort);
            Logger.Log("改为", newShort);
            SearchResults.Add(new(newShort));
            if (oldShort is {}) {
                Dict.Remove(oldShort.Src);
                Logger.Log("删除", oldShort.Src);
                SearchResults.Remove(oldShort);
                Dict.Insert(newLong!);
                Logger.Log("改为", newLong);
                SearchResults.Add(new(newLong!));
            }
            ModifyCommand.NotifyCanExecuteChanged();

            Entry Lengthen() {
                var fullCodes = Encode!(oldShort.Word)
                    .Where(s => s.StartsWith(SearchText, Ordinal))
                    .ToArray();
                if (fullCodes.Length == 0) throw new IOE("找不到匹配的新长码");
                var newLongCode = fullCodes.Any(s => s.StartsWith(oldLong.Code, Ordinal))
                    ? oldLong.Code // 直接交换编码
                    : GetLongCode(fullCodes);
                return Entry.TryNew(oldShort.Word, newLongCode, oldShort.Weight)
                    ?? throw new IOE("新长码无效");
            }

            string GetLongCode(string[] fullCodes) {
                for (var len = SearchText.Length + 1; len <= MaxCodeLen; len++) {
                    var codes = fullCodes.Select(s => s[..len]).Distinct().ToArray();
                    if (codes.Length > 1) throw new IOE("新长码不唯一");
                    if (Dict!.SearchByCode(codes[0], true).Count == 0) return codes[0];
                }
                throw new IOE("没有空闲的新长码");
            }
        } catch (Exception ex) { LogAndShowEx($"截短编码时：\n{ex}"); }
    }

    private bool CanModify => Dict is {} && SearchResults.Count > 0;

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify() {
        try {
            var mods = SearchResults.Select(static me => (Old: me, New: me.TryNew()))
                .Where(static mod => mod.New is {})
                .ToArray();
            if (mods.Length == 0) throw new IOE("没有改动，什么都没做");

            List<string> messages = new(mods.Length * 3);
            foreach (var (o, n) in mods) {
                messages.AddRange([$"删除：'{o.Src}'", $"改为：'{n}'"]);
                if (o.Src.Code is {} code && code != n!.Code && Dict!.IsCodePrefix(code))
                    messages.Add($"修改后，编码'{code}'将空缺，有长码可被截短");
            }
            var msg = $"确认修改？\n{string.Join('\n', messages)}";
            if (Show(msg, "确认", YesNo, Question) != MBR.Yes) return;

            foreach (var (o, n) in mods) {
                Dict!.Remove(o.Src);
                Logger.Log("删除", o.Src);
                SearchResults.Remove(o);
                Dict.Insert(n!);
                Logger.Log("改为", n);
                SearchResults.Add(new(n!));
            }
            ModifyCommand.NotifyCanExecuteChanged();
        } catch (Exception ex) { LogAndShowEx($"修改词条时：\n{ex}"); }
    }

    #endregion 词库操作

    #region 集中响应

    private static void LogAndShowEx(string msg) {
        Logger.Log(msg, null);
        Show(msg, "异常", OK, Error);
    }

    private static void LogAndShowSuccess(params string[] msg) {
        Logger.Log(string.Join('，', msg), null);
        Show(string.Join('\n', msg), "成功", OK, Information);
    }

    private readonly Dictionary<string, IRelayCommand[]> _dependencies;

    public MainVM() =>
        _dependencies = new() {
            [nameof(Dict)]
                = [SaveAsCommand, InsertCommand, RemoveCommand, ShortenCommand, ModifyCommand],
            [nameof(DictModified)] = [SaveCommand],
            [nameof(Word)] = [InsertCommand],
            [nameof(SelAutoCode)] = [InsertCommand],
            [nameof(ManualCode)] = [InsertCommand],
            [nameof(Weight)] = [InsertCommand],
            [nameof(EncoderActive)] = [PickCharsDictCommand, InsertCommand, ShortenCommand],
            [nameof(Encode)] = [ShortenCommand],
            [nameof(IsSearchByCode)] = [ShortenCommand],
            [nameof(SearchText)] = [ShortenCommand],
            [nameof(SelSearchResult)] = [RemoveCommand, ShortenCommand]
        };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);
        if (e.PropertyName is not {} n || !_dependencies.TryGetValue(n, out var commands)) return;
        foreach (var command in commands) command.NotifyCanExecuteChanged();
    }

    #endregion 集中响应
}
