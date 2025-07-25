using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RimeDictManager.Models;
using RimeDictManager.Services;
using RimeDictManager.Services.Encoding;
using RimeDictManager.Services.Logging;
using RimeDictManager.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace RimeDictManager.ViewModels;

/// <summary> 主窗口的ViewModel </summary>
internal partial class MainViewModel : ObservableObject
{
    #region 服务

    /// <summary> Rime词库管理器 </summary>
    [ObservableProperty]
    private DictManager? _dictManager;

    partial void OnDictManagerChanged(DictManager? value) => Search();

    /// <summary> 编码器接口 </summary>
    [ObservableProperty]
    private IEncoder? _encoder;

    partial void OnEncoderChanging(IEncoder? value)
    {
        if (value is { CharCount: > 0 })
            return;
        Encoder = null;
        throw new InvalidOperationException(
            "词库文件不含任何有效单字！未激活编码器。");
    }

    partial void OnEncoderChanged(IEncoder? value)
    {
        if (value is IVarLenEncoder encoder)
            (MinCodeLength, MaxCodeLength) = encoder.CodeLengthRange;
        Encode();
        OnPropertyChanged(nameof(UsingVarLenEncoder));
    }

    /// <returns> 是否正在使用变长编码器 </returns>
    public bool UsingVarLenEncoder
        => AutoEncode && Encoder is IVarLenEncoder;

    /// <summary> 只写静态日志单例 </summary>
    private static readonly ILogWriter _logWriter = Logger.Writer;

    #endregion

    #region 属性：运行模式

    /// <returns> 是否正在使用自动编码 </returns>
    [ObservableProperty]
    private bool _autoEncode = false;

    partial void OnAutoEncodeChanged(bool value)
    {
        Encode();
        OnPropertyChanged(nameof(UsingVarLenEncoder));
    }

    /// <returns> 是否正在使用手动编码（不用于判断） </returns>
    [ObservableProperty]
    private bool _manualEncode = true;

    #endregion

    #region 属性：手动输入的文本

    /// <returns> 词组文本 </returns>
    [ObservableProperty]
    private string _word = "";

    partial void OnWordChanged(string value)
    {
        if (SearchMode == 0
            && !string.IsNullOrWhiteSpace(value))
            SearchText = value;
        Encode();
    }

    /// <returns> 手动编码文本 </returns>
    [ObservableProperty]
    private string _manualCode = "";

    partial void OnManualCodeChanged(string value)
    {
        if (SearchMode == 1
            && !string.IsNullOrWhiteSpace(value))
            SearchText = value;
    }

    /// <returns> 权重文本 </returns>
    [ObservableProperty]
    private string _weight = "";

    /// <returns> 造词码文本 </returns>
    [ObservableProperty]
    private string _stem = "";

    /// <returns> 当前待添加的词条 </returns>
    private Entry? CurrentEntry => new(
        Word.Trim(),
        AutoEncode ? SelectedAutoCode ?? "" : ManualCode.Trim(),
        Weight.Trim(),
        Stem.Trim());

    #endregion

    #region 属性：自动编码相关

    /// <summary> 所有可用的编码器名称 </summary>
    public static ObservableCollection<string> EncoderNames { get; }
        = new(EncoderFactory.EncoderNames);

    /// <returns> 选中的编码器索引 </returns>
    [ObservableProperty]
    private int _selectedEncoderNameIndex = -1;

    partial void OnSelectedEncoderNameIndexChanged(int value) => LoadEncoder();

    /// <returns> 选中的编码器名称 </returns>
    private string? SelectedEncoderName
        => SelectedEncoderNameIndex >= 0
        && SelectedEncoderNameIndex < EncoderNames.Count
        ? EncoderNames[SelectedEncoderNameIndex]
        : null;

    /// <returns> 当前码长 </returns>
    [ObservableProperty]
    private byte _currentCodeLength = 4;

    partial void OnCurrentCodeLengthChanged(byte value) => GenPresentAutoCodes();

    /// <returns> 最大码长 </returns>
    [ObservableProperty]
    private byte _maxCodeLength = 4;

    /// <returns> 最小码长 </returns>
    [ObservableProperty]
    private byte _minCodeLength = 4;

    /// <summary> 原始自动编码结果（全码） </summary>
    private string[] _autoFullCodes = [];

    /// <summary> 界面显示的自动编码结果 </summary>
    [ObservableProperty]
    public ObservableCollection<string> _presentAutoCodes = [];

    /// <returns> 选中的自动编码索引 </returns>
    [ObservableProperty]
    private int _selectedAutoCodeIndex = -1;

    partial void OnSelectedAutoCodeIndexChanged(int value)
    {
        if (SearchMode == 1
            && !string.IsNullOrWhiteSpace(SelectedAutoCode))
            SearchText = SelectedAutoCode;
    }

    /// <returns> 选中的自动编码值 </returns>
    private string? SelectedAutoCode
        => SelectedAutoCodeIndex >= 0
        && SelectedAutoCodeIndex < PresentAutoCodes.Count
        ? PresentAutoCodes[SelectedAutoCodeIndex]
        : null;

    /// <summary> 自动编码文本的颜色 </summary>
    [ObservableProperty]
    private Brush _autoCodeColor = Brushes.Black;

    #endregion

    #region 属性：搜索相关

    /// <returns>
    /// 搜索模式：0按词组精准搜索，1按编码前缀搜索
    /// </returns>
    [ObservableProperty]
    private byte _searchMode = 1;

    partial void OnSearchModeChanged(byte value) => Search();

    /// <returns> 搜索的文本 </returns>
    [ObservableProperty]
    private string _searchText = "";

    partial void OnSearchTextChanged(string value) => Search();

    /// <summary> 界面显示的搜索结果 </summary>
    [ObservableProperty]
    public ObservableCollection<MutableEntry> _searchResults = [];

    /// <returns> 选中的搜索结果索引 </returns>
    [ObservableProperty]
    private int _selectedSearchResultIndex = -1;

    /// <returns> 选中的搜索结果值 </returns>
    private MutableEntry? SelectedSearchResult
        => SelectedSearchResultIndex >= 0
        && SelectedSearchResultIndex < SearchResults.Count
        ? SearchResults[SelectedSearchResultIndex]
        : null;

    #endregion

    #region 事件：自动编码、搜索、激活编码器

    /// <summary> 自动编码 </summary>
    private void Encode() => Try.Do("自动编码", () =>
    {
        _autoFullCodes = AutoEncode && Encoder is not null
            ? [.. Encoder.Encode(Word)]
            : [];
        GenPresentAutoCodes();
    });

    /// <summary> 刷新界面显示的自动编码结果 </summary>
    private void GenPresentAutoCodes()
    {
        var oldSelected = SelectedAutoCode;

        PresentAutoCodes = Encoder is IVarLenEncoder encoder
            && CurrentCodeLength < MaxCodeLength
            ? new(encoder.ShortenCodes(_autoFullCodes, CurrentCodeLength).Order())
            : new(_autoFullCodes.Order());

        SelectedAutoCodeIndex = -1; // 刷新选中编码，以触发SearchText的更新
        SelectedAutoCodeIndex = string.IsNullOrWhiteSpace(oldSelected)
            ? 0
            : PresentAutoCodes.Select((code, index) => (code, index))
                .FirstOrDefault(pair
                => pair.code.StartsWith(oldSelected)
                || oldSelected.StartsWith(pair.code)).index;

        AutoCodeColor = PresentAutoCodes.Count > 1
            ? Brushes.IndianRed
            : Brushes.Black;
    }

    /// <summary> 搜索并取消选择 </summary>
    private void Search() => Try.Do("搜索词条", () =>
    {
        var entries = DictManager is { Count: > 0 }
            && !string.IsNullOrWhiteSpace(SearchText)
            ? SearchMode == 0
                ? DictManager.SearchWord(SearchText)
                : DictManager.SearchCode(SearchText, false)
            : [];
        var ordered = entries.OrderBy(static entry => entry.OrderKey);

        SearchResults.Clear();
        foreach (var entry in ordered)
            SearchResults.Add(new(entry));

        ModifyCommand.NotifyCanExecuteChanged();
        SelectedSearchResultIndex = -1; // 取消选择
    });

    /// <summary> 激活编码器，SelectedEncoderName不会为null </summary>
    private void LoadEncoder() => Try.Do("载入单字词库并激活编码器", () =>
    {
        OpenFileDialog dialog = new()
        {
            Title = $"打开 {SelectedEncoderName!} 的单字词库文件以激活编码器",
            Filter = "Rime词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*",
        };
        if (dialog.ShowDialog() != true)
            return;

        Encoder = EncoderFactory.CreateEncoder(
            SelectedEncoderName!, dialog.FileName);
        var info1 = $"成功激活 {SelectedEncoderName} 编码器";
        var info2 = $"使用单字词库 {dialog.FileName}";
        var info3 = $"覆盖 {Encoder.CharCount} 个单字";
        _logWriter.Log($"{info1}，{info2}，{info3}");
        MsgBox.Info("成功", $"{info1}\n{info2}\n{info3}");
    });

    #endregion

    #region 命令：词库文件相关

    /// <summary> 打开一个Rime词库文件（.dict.yaml） </summary>
    [RelayCommand]
    private void Open() => Try.Do("打开词库", () =>
    {
        if (DictManager is { IsModified: true }
            && !MsgBox.Confirm("警告", "有未保存的修改，是否丢弃？"))
            return;

        OpenFileDialog dialog = new()
        {
            Title = "打开Rime词库文件",
            Filter = "Rime词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*",
        };
        if (dialog.ShowDialog() != true)
            return;

        DictManager = new(dialog.FileName);
        var info1 = $"成功打开 {dialog.FileName}";
        var info2 = $"共有 {DictManager.Count} 个唯一词条";
        _logWriter.Log($"{info1}，{info2}");
        MsgBox.Info("成功", $"{info1}\n{info2}");
    });

    /// <returns> 词库是否已修改 </returns>
    private bool CanSave
        => DictManager is { IsModified: true };

    /// <summary>
    /// 保存对原词库文件的修改（覆盖原文件）
    /// 可用性保证词库非null
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
        => await Try.DoAsync("保存词库", async () =>
        {
            await DictManager!.SortAndSaveAsync();
            OnPropertyChanged(nameof(DictManager));
            var info1 = "成功保存并覆盖原文件";
            var info2 = $"共有 {DictManager.Count} 个唯一词条";
            _logWriter.Log($"{info1}，{info2}");
            MsgBox.Info("成功", $"{info1}\n{info2}");
        });

    /// <returns> 词库是否已加载 </returns>
    private bool CanSaveAs
        => DictManager is not null;

    /// <summary>
    /// 将修改后的词库另存为一个Rime词库文件（.dict.yaml）
    /// 可用性保证词库非null
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveAs))]
    private async Task SaveAs()
        => await Try.DoAsync("另存词库", async () =>
        {
            SaveFileDialog dialog = new()
            {
                Title = "将词库另存为...",
                Filter = "Rime词库文件 (*.dict.yaml)|*.dict.yaml|所有文件 (*.*)|*.*",
                DefaultExt = ".dict.yaml"
            };
            if (dialog.ShowDialog() != true)
                return;

            await DictManager!.SortAndSaveAsync(dialog.FileName);
            OnPropertyChanged(nameof(DictManager));
            var info1 = $"成功另存至 {dialog.FileName}";
            var info2 = $"共有 {DictManager.Count} 个唯一词条";
            _logWriter.Log($"{info1}，{info2}");
            MsgBox.Info("成功", $"{info1}\n{info2}");
        });

    /// <returns> 是否启用自动编码并选中了编码器名称 </returns>
    private bool CanReload
        => AutoEncode && !string.IsNullOrWhiteSpace(SelectedEncoderName);

    /// <summary> 重新载入当前编码方案的单字词库并更新编码器 </summary>
    [RelayCommand(CanExecute = nameof(CanReload))]
    private void Reload() => LoadEncoder();

    #endregion

    #region 核心方法：添加和删除

    /// <summary> 插入词条的核心逻辑 </summary>
    /// <returns> 是否原无该词条且插入成功 </returns>
    private bool InsertCore(Entry entry)
    {
        if (DictManager?.Insert(entry) != true)
            return false;
        OnPropertyChanged(nameof(DictManager));
        _logWriter.Log("添加", entry);
        return true;
    }

    /// <summary> 删除词条的核心逻辑 </summary>
    /// <returns> 是否原有该词条且删除成功 </returns>
    private bool RemoveCore(Entry entry)
    {
        if (DictManager?.Remove(entry) != true)
            return false;
        OnPropertyChanged(nameof(DictManager));
        _logWriter.Log("删除", entry);
        return true;
    }

    /// <summary> 在搜索结果中插入词条 </summary>
    private void InsertSearchResult(Entry entry)
    {
        switch (SearchMode)
        {
            case 0 when SearchText == entry.Word:
            case 1 when entry.Code.StartsWith(SearchText):
                var results = SearchResults.Append(new(entry))
                    .OrderBy(static me => me.OriginalEntry.OrderKey);
                SearchResults.Clear();
                foreach (var result in results)
                    SearchResults.Add(result);
                break;
        }
    }

    #endregion

    #region 命令：词库修改相关

    /// <returns> 是否已打开词库且当前词条有效 </returns>
    private bool CanInsert
        => DictManager is not null
        && (CurrentEntry?.IsValid ?? false);

    /// <summary>
    /// 将各信息添加为一个新词条。
    /// 可用性保证词库非null，且当前词条有效。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanInsert))]
    private void Insert() => Try.Do("添加词条", () =>
    {
        var info = string.Join('\n',
            DictManager!.SearchSimilarEntries(CurrentEntry!)
            .OrderBy(static entry => entry.OrderKey)
            .Select(static entry => entry.ToString())); // 不含全同词条
        if (info.Length > 0
            && !MsgBox.Confirm("警告",
            $"词库中存在词组或编码相同的词条：\n{info}\n是否仍要添加？"))
            return;

        if (InsertCore(CurrentEntry!))
        {
            InsertSearchResult(CurrentEntry!);
            Word = ManualCode = Weight = Stem = ""; // 不会触发搜索
        }
        else MsgBox.Info("提示", "词条原已存在。未添加。");
    });

    /// <returns> 是否存在选中的搜索结果词条 </returns>
    private bool CanRemove
        => SelectedSearchResult is not null;

    /// <summary>
    /// 将选中的词条删除。
    /// 可用性保证选中词条非null。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove() => Try.Do("删除词条", () =>
    {
        if (SelectedSearchResult!.CurrentEntry
            != SelectedSearchResult.OriginalEntry)
            throw new InvalidOperationException(
                "不能直接删除手动修改过的词条！");

        var entry = SelectedSearchResult.OriginalEntry;
        if (!MsgBox.Confirm("确认", $"是否删除以下词条？\n{entry}"))
            return;

        if (!RemoveCore(entry))
            MsgBox.Info("提示", "词条原不存在！未删除。");
        else if (SearchResults.Remove(SelectedSearchResult))
            SelectedSearchResultIndex = -1; // 防止重复操作
        else throw new InvalidOperationException(
            "词库和搜索结果不一致，请勿继续操作！");
    });

    /// <returns>
    /// 是否正在使用有效的变长编码器，
    /// 且允许截短选中词条的编码
    /// </returns>
    private bool CanShorten
        => UsingVarLenEncoder
        && SearchMode == 1
        && (SelectedSearchResult?.OriginalEntry.Code.Length > SearchText.Length);

    /// <summary>
    /// 将选中词条的编码截短为搜索框中的编码，
    /// 若目标处已有词条，则尝试自动加长其编码。
    /// 可用性保证正在使用有效的变长编码器，
    /// 且允许截短选中词条的编码。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShorten))]
    private void Shorten() => Try.Do("截短编码", () =>
    {
        if (SelectedSearchResult!.CurrentEntry
            != SelectedSearchResult.OriginalEntry)
            throw new InvalidOperationException(
                "不能直接截短手动修改过的词条！");

        var encoder = (IVarLenEncoder)Encoder!;

        // 后缀1代表截短选中词条的编码，2代表加长占位词条的编码
        var entry1 = SelectedSearchResult.OriginalEntry;
        var codes1 = encoder.Encode(entry1.Word);
        var targetCode1 = SearchText;
        if (!encoder.ShortenCodes(codes1, targetCode1.Length)
            .Contains(targetCode1))
            throw new InvalidOperationException(
                $"无法为词组 “{entry1.Word}” 重新自动编码，\n"
              + "或者搜索框中的编码不是其合法的短编码！");

        var newEntry1 = entry1 with { Code = targetCode1 };
        if (!newEntry1.IsValid)
            throw new InvalidOperationException(
                $"词组 “{entry1.Word}” 的编码截短后词条无效！");

        var obstacles = SearchResults.Where(me
            => me.OriginalEntry.Code == targetCode1).ToArray();
        if (obstacles.Length > 1)
            throw new InvalidOperationException(
                $"目标短编码 “{targetCode1}” 被不止一个词条占用！");

        if (obstacles.Length == 1)
        {
            var entry2 = obstacles.Single().OriginalEntry;
            string[] codes2 = [.. encoder.Encode(entry2.Word)];
            if (codes2.Length == 0)
                throw new InvalidOperationException(
                $"无法为占位词组 “{entry2.Word}” 重新自动编码！");

            var targetCode2 = encoder.ShortenCodes(codes2, entry1.Code.Length)
                .Contains(entry1.Code)
                ? entry1.Code // 直接交换编码
                : EncodingUtils.GetLengthenedCode(
                    entry2.Code, codes2, DictManager!, encoder)
                ?? throw new InvalidOperationException(
                    $"无法为占位词组 “{entry2.Word}” 找到唯一且空闲的长编码！");

            var newEntry2 = entry2 with { Code = targetCode2 };
            if (!newEntry2.IsValid)
                throw new InvalidOperationException(
                    $"占位词组 “{entry2.Word}” 的编码加长后词条无效！");

            var info1 = $"将 “{entry1.Word}” 的编码截短为 “{targetCode1}”";
            var info2 = $"将 “{entry2.Word}” 的编码加长为 “{targetCode2}”";
            if (!MsgBox.Confirm("确认", $"是否进行以下操作？\n{info1}\n{info2}"))
                return;

            if (!RemoveCore(entry2) || !InsertCore(newEntry2))
                throw new InvalidOperationException(
                    "未能加长占位词条的编码！请查看日志。");
        }

        if (SearchResults.Any(me
            => me.OriginalEntry.Code != entry1.Code
            && me.OriginalEntry.Code.StartsWith(entry1.Code)))
            MsgBox.Info("提示", $"截短后，“{entry1.Code}” 可能成为编码空位");

        if (!RemoveCore(entry1) || !InsertCore(newEntry1))
            throw new InvalidOperationException(
                "未能截短选中词条的编码！请查看日志。");

        Search(); // 刷新搜索结果并取消选择
    });

    /// <returns> 原始搜索结果是否不为空 </returns>
    private bool CanModify
        => SearchResults.Count > 0;

    /// <summary> 应用DataGrid中的手动修改 </summary>
    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify() => Try.Do("应用手动修改", () =>
    {
        var modifiedEntries = SearchResults.Where(static me
            => me.OriginalEntry != me.CurrentEntry
            && me.CurrentEntry.IsValid).ToArray();
        if (modifiedEntries.Length == 0)
        {
            MsgBox.Info("提示", "没有任何有效的手动修改。什么都没做。");
            return;
        }

        var info = string.Join('\n',
            modifiedEntries.Select(static me
            => $"原有\t{me.OriginalEntry}\n改为\t{me.CurrentEntry}"));
        if (!MsgBox.Confirm("确认", $"是否应用以下手动修改？\n{info}"))
            return;

        var count = modifiedEntries.Count(me
            => RemoveCore(me.OriginalEntry)
            && InsertCore(me.CurrentEntry));
        if (count == modifiedEntries.Length)
            MsgBox.Info("提示", "全部修改成功。");
        else MsgBox.Info("提示", "未能全部修改成功。请查看日志。");

        Search(); // 刷新搜索结果并取消选择
    });

    /// <summary> 打开日志窗口并查看修改日志 </summary>
    [RelayCommand]
    private static void OpenLog()
        => Try.Do("打开日志窗口", ()
            => _ = new Views.LogWindow().ShowDialog());

    #endregion

    #region 命令可用性变更通知

    /// <summary> 集中处理命令可用性的变更通知 </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e); // 原始通知

        if (!string.IsNullOrWhiteSpace(e.PropertyName)
            && _commandDependencies.TryGetValue(e.PropertyName, out var commands))
            foreach (var command in commands)
                command.NotifyCanExecuteChanged();
    }

    /// <summary> 命令可用性的依赖关系 </summary>
    private readonly Dictionary<string, IRelayCommand[]> _commandDependencies;

    /// <summary> 构造时初始化命令可用性的依赖关系 </summary>
    public MainViewModel() => _commandDependencies = new()
    {
        [nameof(DictManager)] = [SaveAsCommand, SaveCommand, InsertCommand],
        [nameof(Encoder)] = [ShortenCommand],
        [nameof(AutoEncode)] = [ReloadCommand, InsertCommand, ShortenCommand],
        [nameof(Word)] = [InsertCommand],
        [nameof(ManualCode)] = [InsertCommand],
        [nameof(Weight)] = [InsertCommand],
        [nameof(Stem)] = [InsertCommand],
        [nameof(SelectedEncoderNameIndex)] = [ReloadCommand],
        [nameof(PresentAutoCodes)] = [InsertCommand],
        [nameof(SelectedAutoCodeIndex)] = [InsertCommand],
        [nameof(SearchMode)] = [ShortenCommand],
        [nameof(SearchText)] = [ShortenCommand],
        [nameof(SearchResults)] = [RemoveCommand, ShortenCommand],
        [nameof(SelectedSearchResultIndex)] = [RemoveCommand, ShortenCommand]
    };

    #endregion
}
