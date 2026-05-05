namespace RimeDictManager.Models;

using System.IO;
using static System.Runtime.InteropServices.CollectionsMarshal;

internal sealed class Dict {
    private readonly CodeTrie _codeTrie = new();
    private readonly Dictionary<string, List<Entry>> _dict = new(8192);
    private readonly List<string> _header = [];
    private readonly List<Line> _misc = [];
    private readonly Action<bool> _onModifiedChanged;
    private readonly string _path;

    public Dict(string path, Action<bool> onModifiedChanged) {
        var isHeader = true;
        var num = 1u;
        foreach (var line in File.ReadLines(path)) {
            if (isHeader) {
                _header.Add(line);
                if (line == "...") isHeader = false;
            } else if (line.Length == 0)
                _misc.Add(new(num, null));
            else if (line[0] == '#')
                _misc.Add(new(num, line));
            else
                InsertCore(Entry.FromString(num, line));
            num++;
        }
        _onModifiedChanged = onModifiedChanged;
        _path = path;
        if (_header.LastOrDefault() != "...") throw new FormatException("词库缺失文件头");
    }

    public uint Count { get; private set; }

    private bool Modified {
        set {
            if (field != value) _onModifiedChanged(field = value);
        }
    }

    public void Insert(Entry entry) {
        InsertCore(entry);
        Modified = true;
    }

    public void Remove(Entry entry) {
        _codeTrie.Remove(entry); // 找不到会抛异常
        if (!_dict[entry.Word].Remove(entry))
            throw new InvalidOperationException("Trie和Dict不一致，请停用并报告异常");
        Count--;
        Modified = true;
    }

    private void InsertCore(Entry entry) {
        _codeTrie.Insert(entry);
        ref var list = ref GetValueRefOrAddDefault(_dict, entry.Word, out var exist);
        if (exist)
            list!.Add(entry);
        else
            list = [entry];
        Count++;
    }

    public IReadOnlyList<Entry> SearchByCode(string code, bool exact) =>
        _codeTrie.Search(code, exact);

    public IReadOnlyList<Entry> SearchByWord(string word) =>
        _dict.TryGetValue(word, out var list)
            ? list
            : [];

    public bool IsCodePrefix(string code) =>
        code.Length > 0 && _codeTrie.Search(code, false).Any(e => e.Code!.Length > code.Length);

    /// <summary> 保存词库 </summary>
    /// <param name="path"> 路径：null时覆写 </param>
    /// <param name="sort"> true时词条先按编码升序再按原序（新词条后置），注释原序放在末尾，空行删除；false保留原序，新词条按编码升序放在末尾 </param>
    public void Save(string? path, bool sort) {
        var entries = _dict.Values.SelectMany(static x => x).ToArray();
        var sorted = sort
            ? entries.OrderBy(static e => (e.Code, e.Num - 1)) // 0回绕（新词条后置）
                .Concat(_misc.Where(static l => l.Raw is {})) // 注释原序
            : entries.Where(static e => e.Num > 0) // 原有词条
                .Concat(_misc)
                .OrderBy(static l => l.Num)
                .Concat(entries.Where(static e => e.Num == 0).OrderBy(static e => e.Code));

        using StreamWriter sw = new(path ?? _path);
        sw.NewLine = "\n";
        foreach (var line in _header.Concat(sorted.Select(static l => $"{l}"))) sw.WriteLine(line);

        Modified = false;
    }
}
