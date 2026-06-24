namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

public interface IDictInfo {
    string Path { get; }
    string Name { get; }
    IReadOnlyList<DictCol> Cols { get; }
    uint Cnt { get; }
    bool Modified { get; }
}

public sealed class Dict: IDictInfo {
    private readonly List<EntryLine> _entries;
    private readonly CodeTrie _entriesByCode;
    private readonly Dictionary<string, List<int>> _entriesByText;
    private uint _num;

    public Dict(
        string path,
        string header,
        string name,
        IReadOnlyList<DictCol> cols,
        IReadOnlyList<RawLine> rawLines,
        IReadOnlyList<EntryLine> entries,
        uint num) {
        _entries = new(entries);
        _num = num;
        Header = header;
        RawLines = rawLines;
        Path = path;
        Name = name;
        Cols = cols;
        Cnt = (uint)_entries.Count;
        _entriesByCode = new(4 * _entries.Count);
        _entriesByText = new(_entries.Count);
        for (var i = 0; i < _entries.Count; i++) {
            var e = _entries[i];
            _entriesByCode.Insert(e.Code, i);
            ref var indexes = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
            if (exists)
                indexes!.Add(i);
            else
                indexes = [i];
        }
    }

    public string Header { get; }
    public IReadOnlyList<RawLine> RawLines { get; }
    public IEnumerable<EntryLine> Entries => _entries.Where(static e => e.Num > 0);

    public string Path { get; }
    public string Name { get; }
    public IReadOnlyList<DictCol> Cols { get; }
    public uint Cnt { get; private set; }
    public bool Modified { get; private set; }

    public EntryLine Insert(EntryLine e) {
        if (e.Num == 0) e = e with { Num = ++_num };
        _entries.Add(e);

        var i = _entries.Count - 1;
        _entriesByCode.Insert(e.Code, i);
        ref var indexes = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
        if (exists)
            indexes!.Add(i);
        else
            indexes = [i];

        Cnt++;
        Modified = true;
        return e;
    }

    public bool Remove(EntryLine e) {
        if (!_entriesByText.TryGetValue(e.Text, out var indexes)
         || indexes.FindIndex(i => _entries[i] == e) is not (>= 0 and var j))
            return false;

        var i = indexes[j];
        indexes[j] = indexes[^1];
        indexes.RemoveAt(indexes.Count - 1);
        if (!_entriesByCode.Remove(e.Code, i)
         || (indexes.Count == 0 && !_entriesByText.Remove(e.Text)))
            throw new InvalidOperationException("请停用并报告：数据结构内部相悖");

        _entries[i] = e with { Num = 0 }; // 标记死亡
        Cnt--;
        return Modified = true;
    }

    public void NotifySaved() => Modified = false;

    public bool ContainsCode(string code) => _entriesByCode[code]?.Count > 0;

    public bool IsOnlyCodePrefix(string code) =>
        _entriesByCode[code]?.Count == 1 && _entriesByCode.AnyDescendantValue(code);

    public void ForEachByCode(string code, Action<EntryLine> f) {
        if (_entriesByCode[code] is not {} indexes) return;
        foreach (var i in indexes) f(_entries[i]);
    }

    public void ForEachByCodePrefix(string code, Action<EntryLine> f) =>
        _entriesByCode.ForEachSubtreeValue(code, i => f(_entries[i]));

    public void ForEachByText(string text, Action<EntryLine> f) {
        if (!_entriesByText.TryGetValue(text, out var indexes)) return;
        foreach (var i in indexes) f(_entries[i]);
    }
}
