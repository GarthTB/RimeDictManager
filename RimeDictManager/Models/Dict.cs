namespace RimeDictManager.Models;

using Serde;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class Dict: IDictInfo {
    private readonly List<EntryLine> _entries;
    private readonly CodeTrie _entriesByCode;
    private readonly Dictionary<string, List<int>> _entriesByText;
    private readonly string _header;
    private readonly List<RawLine> _rawLines;
    private uint _num;

    public Dict(string path) {
        using StreamReader reader = new(path);
        _header = DictParser.ReadHeader(reader, path, out var name, out var cols, out _num);
        Name = name;
        Path = path;
        Cols = cols;

        _entries = new(4096);
        _rawLines = new(64);
        for (string? l; (l = reader.ReadLine()) is {}; _num++)
            if (LineCodec.Deserialize(l, _num, Cols, out var e, out var r))
                _entries.Add(e);
            else
                _rawLines.Add(r);
        Cnt = (uint)_entries.Count;

        _entriesByText = new(_entries.Count);
        _entriesByCode = new(4 * _entries.Count);
        for (var i = 0; i < _entries.Count; i++) {
            var e = _entries[i];
            ref var indexes = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
            if (exists)
                indexes!.Add(i);
            else
                indexes = [i];
            _entriesByCode.Insert(e.Code, i);
        }
    }

    public string Name { get; }
    public string Path { get; }
    public IReadOnlyList<Column> Cols { get; }
    public uint Cnt { get; private set; }
    public bool Modified { get; private set; }

    public bool ContainsCode(string code) => _entriesByCode[code]?.Count > 0;

    public bool IsOnlyCodePrefix(string code) =>
        _entriesByCode[code]?.Count == 1 && _entriesByCode.AnyDescendantValue(code);

    public EntryLine Insert(EntryLine e) {
        if (e.Num == 0) e = e with { Num = ++_num };
        _entries.Add(e);

        var i = _entries.Count - 1;
        ref var indexes = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
        if (exists)
            indexes!.Add(i);
        else
            indexes = [i];
        _entriesByCode.Insert(e.Code, i);

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

    public void ForEachByText(string text, Action<EntryLine> f) {
        if (!_entriesByText.TryGetValue(text, out var indexes)) return;
        foreach (var i in indexes) f(_entries[i]);
    }

    public void ForEachByCode(string code, Action<EntryLine> f) {
        if (_entriesByCode[code] is not {} indexes) return;
        foreach (var i in indexes) f(_entries[i]);
    }

    public void ForEachByCodePrefix(string code, Action<EntryLine> f) =>
        _entriesByCode.ForEachSubtreeValue(code, i => f(_entries[i]));

    /// <summary> 保存词库（不迁移路径） </summary>
    /// <param name="path"> null则覆写 </param>
    /// <param name="reorder"> true：词条先按Code升序再按Num升序重排，非词条行按原序排在末尾；false：保持原有行，新词条按插入顺序排在末尾 </param>
    public async Task SaveAsync(string? path, bool reorder) {
        await using StreamWriter writer = new(path ?? Path);
        writer.NewLine = "\n";
        await writer.WriteLineAsync(_header);

        if (reorder) {
            foreach (var e in _entries.Where(static e => e.Num > 0)
                .OrderBy(static e => (e.Code, e.Num)))
                await writer.WriteLineAsync(e.Serialize(Cols));
            foreach (var r in _rawLines) await writer.WriteLineAsync(r.Content);
        } else {
            using var entries = _entries.Where(static e => e.Num > 0).GetEnumerator();
            using var rawLines = _rawLines.GetEnumerator();
            var anyE = entries.MoveNext();
            var anyR = rawLines.MoveNext();
            while (anyE || anyR)
                if (anyE && (!anyR || entries.Current.Num <= rawLines.Current.Num)) {
                    await writer.WriteLineAsync(entries.Current.Serialize(Cols));
                    anyE = entries.MoveNext();
                } else {
                    await writer.WriteLineAsync(rawLines.Current.Content);
                    anyR = rawLines.MoveNext();
                }
        }

        Modified = false;
    }
}
