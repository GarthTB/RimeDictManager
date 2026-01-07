namespace RimeDictManager.Services;

using System.IO;
using Models;

/// <summary> 词库管理器 </summary>
internal class RimeDict
{
    /// <summary> 前缀树：提供按编码的快速前缀搜索 </summary>
    private readonly EntryTrie _entriesByCode = new();

    /// <summary> 字典：提供按字词的快速精确搜索 </summary>
    private readonly Dictionary<string, List<Line>> _entriesByWord = new(8105);

    /// <summary> 文件头 </summary>
    private readonly List<string> _header = [];

    /// <summary> 词库路径 </summary>
    private readonly string _srcPath;

    /// <summary> 载入RIME词库文件（.dict.yaml） </summary>
    /// <param name="dictPath"> 词库路径 </param>
    /// <remarks> 格式标准见 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
    public RimeDict(string dictPath) {
        var headerClosed = false;
        var idx = 0u;
        foreach (var rawLine in File.ReadLines(dictPath)) {
            if (!headerClosed) {
                _header.Add(rawLine);
                if (rawLine == "...")
                    headerClosed = true;
            } else
                Insert(Line.FromString(idx, rawLine));
            idx++;
        }
        Modified = false; // 纠正Insert副作用

        if (_header.Count < 2)
            throw new FormatException("词库缺失文件头");
        _srcPath = dictPath;
    }

    /// <summary> 条目总数 </summary>
    public uint Count { get; private set; }

    /// <summary> 是否已修改（需要保存） </summary>
    public bool Modified { get; private set; }

    /// <summary> 插入条目 </summary>
    /// <param name="entry"> 待插入的条目 </param>
    public void Insert(Line entry) {
        _entriesByCode.Insert(entry); // 保证无重且Word非null
        if (_entriesByWord.TryGetValue(entry.Word!, out var list))
            list.Add(entry);
        else
            _entriesByWord[entry.Word!] = [entry];

        Count++;
        Modified = true;
    }

    /// <summary> 删除条目 </summary>
    /// <param name="entry"> 待删除的条目 </param>
    public void Remove(Line entry) {
        _entriesByCode.Remove(entry); // 保证现有
        if (!_entriesByWord[entry.Word!].Remove(entry))
            throw new InvalidOperationException("trie和dict不一致，请报告异常");

        Count--;
        Modified = true;
    }

    /// <summary> 按编码搜索条目 </summary>
    /// <param name="code"> 编码 </param>
    /// <param name="exact"> true时精确搜索，false时前缀搜索 </param>
    /// <returns> 无重、无序的条目 </returns>
    public IReadOnlyList<Line> SearchByCode(string code, bool exact) =>
        _entriesByCode.Search(code, exact);

    /// <summary> 按字词精确搜索条目 </summary>
    /// <param name="word"> 字词 </param>
    /// <returns> 无重、无序的条目 </returns>
    public IReadOnlyList<Line> SearchByWord(string word) =>
        _entriesByWord.TryGetValue(word, out var list)
            ? list
            : [];

    /// <summary> 保存词库 </summary>
    /// <param name="path"> 保存路径：null时覆写 </param>
    /// <param name="sort"> true时条目按编码升序，编码相同则原序，注释原序放在末尾，空行删除； false时保留原序，新条目按Code升序放在末尾 </param>
    public void Save(string? path, bool sort) {
        var entries = _entriesByWord.Values.SelectMany(static list => list).ToArray();
        var orderedEntries = sort
            ? entries.Where(static e => e.Word is { Length: > 0 })
                .OrderBy(static e => e.Code)
                .ThenBy(static e => e.Idx ?? uint.MaxValue)
                .Concat(entries.Where(static e => e.Comment is {}).OrderBy(static e => e.Idx))
            : entries.Where(static e => e.Idx is {})
                .OrderBy(static e => e.Idx)
                .Concat(entries.Where(static e => e.Idx is null).OrderBy(static e => e.Code));

        var lines = _header.Concat(orderedEntries.Select(static e => $"{e}"));
        File.WriteAllLines(path ?? _srcPath, lines);
        Modified = false;
    }
}
