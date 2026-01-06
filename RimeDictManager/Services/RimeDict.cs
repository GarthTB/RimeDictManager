namespace RimeDictManager.Services;

using System.IO;
using Models;

/// <summary> 词库管理器 </summary>
internal class RimeDict
{
    /// <summary> 前缀树：提供按编码的快速前缀搜索 </summary>
    private readonly EntryTrie _entriesByCode = new();

    /// <summary> 字典：提供按词的快速精确搜索 </summary>
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
            } else {
                var lineObj = Line.FromString(idx, rawLine);
                _entriesByCode.Insert(lineObj); // 保证无重且Word非null
                if (_entriesByWord.TryGetValue(lineObj.Word!, out var list))
                    list.Add(lineObj);
                else
                    _entriesByWord[lineObj.Word!] = [lineObj];
                Count++;
            }
            idx++;
        }

        if (_header.Count < 2)
            throw new FormatException("词库缺失文件头");
        _srcPath = dictPath;
    }

    /// <summary> 是否已修改（需要保存） </summary>
    public bool Modified { get; private set; } = false;

    /// <summary> 条目总数 </summary>
    public uint Count { get; }

    /// <summary> 插入条目 </summary>
    /// <param name="entry"> 待插入的条目 </param>
    public void Insert(Line entry) {}

    /// <summary> 删除条目 </summary>
    /// <param name="entry"> 待删除的条目 </param>
    public void Remove(Line entry) {}

    /// <summary> 按词精确搜索 </summary>
    /// <param name="word"> </param>
    /// <returns> </returns>
    public IReadOnlyList<Line> SearchByWord(string word) =>
        _entriesByWord.TryGetValue(word, out var list)
            ? list
            : [];
}
