namespace RimeDictManager.Models;

using System.IO;
using static System.Runtime.InteropServices.CollectionsMarshal;
using static ArgumentException;

internal sealed class Dict {
    private readonly Dictionary<string, List<Line>> _dict = new(8192);
    private readonly List<string> _header = [];
    private readonly List<Line> _misc = [];
    private readonly string _path;
    private readonly CodeTrie _trie = new();

    public Dict(string path) {
        var isHeader = true;
        var num = 1u;
        foreach (var sLine in File.ReadLines(path))
            if (isHeader) {
                _header.Add(sLine);
                if (sLine == "...") isHeader = false;
            } else {
                var oLine = Line.FromString(num++, sLine);
                if (oLine.Word is [not '#', ..])
                    Insert(oLine);
                else
                    _misc.Add(oLine);
            }
        if (_header.Count == 0) throw new FormatException("词库缺失文件头");
        _path = path;
        Modified = false; // 纠正Insert副作用
    }

    public uint Count { get; private set; }
    public bool Modified { get; private set; }

    public void Insert(Line entry) {
        ThrowIfNullOrEmpty(entry.Word);

        _trie.Insert(entry);
        ref var list = ref GetValueRefOrAddDefault(_dict, entry.Word, out var exist);
        if (exist)
            list!.Add(entry);
        else
            list = [entry];

        Count++;
        Modified = true;
    }

    public void Remove(Line entry) {
        ThrowIfNullOrEmpty(entry.Word);

        _trie.Remove(entry); // 保证现有
        if (!_dict[entry.Word].Remove(entry))
            throw new InvalidOperationException("Trie和Dict不一致，请停用并报告异常");

        Count--;
        Modified = true;
    }

    public IReadOnlyList<Line> SearchByCode(string code, bool exact) => _trie.Search(code, exact);

    public IReadOnlyList<Line> SearchByWord(string word) =>
        _dict.TryGetValue(word, out var list)
            ? list.AsReadOnly()
            : [];

    /// <summary> 保存词库 </summary>
    /// <param name="path"> 路径：null时覆写 </param>
    /// <param name="sort"> true时条目按编码升序，编码相同则原序，注释原序放在末尾，空行删除；false时保留原序，新条目按编码升序放在末尾 </param>
    public void Save(string? path, bool sort) {
        var entries = _dict.Values.SelectMany(static x => x).ToArray();
        var ordered = sort
            ? entries.OrderBy(static e => e.Code)
                .ThenBy(static e => e.Num - 1) // 0回绕（新条目后置）
                .Concat(_misc.Where(static l => l.Word is ['#', ..])) // 注释原序
            : entries.Where(static e => e.Num > 0) // 原有条目
                .Concat(_misc)
                .OrderBy(static e => e.Num)
                .Concat(entries.Where(static e => e.Num == 0).OrderBy(static e => e.Code));
        File.WriteAllLines(path ?? _path, _header.Concat(ordered.Select(static e => $"{e}")));

        Modified = false;
    }
}
