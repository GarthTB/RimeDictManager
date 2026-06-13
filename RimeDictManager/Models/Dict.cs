namespace RimeDictManager.Models;

using System.Diagnostics;
using Services.Data;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class Dict {
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
            ref var idx = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
            if (exists)
                idx!.Add(i);
            else
                idx = [i];
            _entriesByCode.Insert(e.Code, i);
        }
    }

    public string Name { get; }
    public string Path { get; }
    public IReadOnlyList<Col> Cols { get; }
    public uint Cnt { get; private set; }
    public bool Mod { get; private set; }

    public void Insert(EntryLine e) {
        if (e.Num == 0) e = e with { Num = ++_num };

        _entries.Add(e);
        var i = _entries.Count - 1;
        ref var idx = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
        if (exists)
            idx!.Add(i);
        else
            idx = [i];
        _entriesByCode.Insert(e.Code, i);

        Cnt++;
        Mod = true;
    }

    public bool Remove(EntryLine e) {
        if (!_entriesByText.TryGetValue(e.Text, out var idx)
         || idx.FindIndex(x => _entries[x] == e) is not (>= 0 and var j))
            return false;

        var i = idx[j];
        idx[j] = idx[^1];
        idx.RemoveAt(idx.Count - 1);
        if (!_entriesByCode.Remove(e.Code, i) || (idx.Count == 0 && !_entriesByText.Remove(e.Text)))
            throw new UnreachableException("严重错误：请停用并报告异常A");

        _entries[i] = e with { Num = 0 }; // 标记死亡
        Cnt--;
        return Mod = true;
    }

    public bool ContainsCode(string? code) => _entriesByCode.HasValue(code);
    public bool IsCodePrefix(string? code) => _entriesByCode.HasChildValue(code);

    public void ForEachByCode(string? code, bool exact, Action<EntryLine> f) =>
        _entriesByCode.ForEachBy(code, exact, i => f(_entries[i]));

    public void ForEachByText(string text, Action<EntryLine> f) {
        if (!_entriesByText.TryGetValue(text, out var indexes)) return;
        foreach (var i in indexes) f(_entries[i]);
    }

    /// <summary> 保存词库（不迁移路径） </summary>
    /// <param name="path"> null则覆写 </param>
    /// <param name="reorder"> true：词条先按Code升序再按Num升序重排，空行丢弃，注释原序排在末尾；false：保持原有行，新词条按插入顺序排在末尾 </param>
    public async Task SaveAsync(string? path, bool reorder) {
        await using StreamWriter writer = new(path ?? Path);
        writer.NewLine = "\n";
        await writer.WriteLineAsync(_header);

        if (reorder) {
            foreach (var e in _entries.Where(static e => e.Num > 0)
                .OrderBy(static e => (e.Code, e.Num)))
                await writer.WriteLineAsync(e.Serialize(Cols));
            foreach (var r in _rawLines.Where(static r => r.Content is {}))
                await writer.WriteLineAsync(r.Content);
        } else {
            using var entries = _entries.Where(static e => e.Num > 0).GetEnumerator();
            using var rawLines = _rawLines.GetEnumerator();
            bool anyE = entries.MoveNext(), anyR = rawLines.MoveNext();
            while (anyE || anyR)
                if (anyE && (!anyR || entries.Current.Num <= rawLines.Current.Num)) {
                    await writer.WriteLineAsync(entries.Current.Serialize(Cols));
                    anyE = entries.MoveNext();
                } else {
                    await writer.WriteLineAsync(rawLines.Current.Content);
                    anyR = rawLines.MoveNext();
                }
        }

        Mod = false;
    }
}
