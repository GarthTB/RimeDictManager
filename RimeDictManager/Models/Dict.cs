namespace RimeDictManager.Models;

using System.Diagnostics;
using System.Text;
using Utils;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class Dict {
    private readonly IReadOnlyList<Col> _cols;
    private readonly List<EntryLine> _entries;
    private readonly CodeTrie _entriesByCode;
    private readonly Dictionary<string, List<int>> _entriesByText;
    private readonly string _header, _name, _path;
    private readonly List<RawLine> _rawLines;

    public Dict(string path) {
        using StreamReader reader = new(_path = path);
        var num = 1u;

        var headerIdx = 0;
        StringBuilder header = new(1024);
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (header.Length > 16384) throw new FormatException("词库文件头过长，疑似缺失或未闭合");
            if (string.IsNullOrWhiteSpace(l))
                header.Append('\n');
            else if (l == "---")
                headerIdx = header.Append(l).Append('\n').Length;
            else if (l == "...") {
                var yaml = header.ToString(headerIdx, header.Length - headerIdx);
                Codec.ParseHeader(yaml, path, out _name, out _cols);
                header.Append(l);
                num++;
                break;
            } else
                header.Append(l).Append('\n');
        }
        if (_name is null || _cols is null) throw new FormatException("词库文件头缺失或未闭合");
        _header = header.ToString();

        _entries = new(4096);
        _rawLines = new(64);
        for (string? l; (l = reader.ReadLine()) is {}; num++)
            if (string.IsNullOrWhiteSpace(l))
                _rawLines.Add(new(num, null));
            else if (l[0] == '#')
                _rawLines.Add(new(num, l));
            else
                _entries.Add(Codec.Deserialize(num, l, _cols));
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

    public uint Cnt { get; private set; }
    public bool Mod { get; private set; }

    public void Insert(EntryLine e) {
        _entries.Add(e);
        var i = _entries.Count - 1;
        ref var indexes = ref GetValueRefOrAddDefault(_entriesByText, e.Text, out var exists);
        if (exists)
            indexes!.Add(i);
        else
            indexes = [i];
        _entriesByCode.Insert(e.Code, i);

        Cnt++;
        Mod = true;
    }

    public bool Remove(EntryLine e) {
        if (!_entriesByText.TryGetValue(e.Text, out var indexes)
         || indexes.FindIndex(x => _entries[x] == e) is not (>= 0 and var j))
            return false;

        var i = indexes[j];
        indexes[j] = indexes[^1];
        indexes.RemoveAt(indexes.Count - 1);
        if (!_entriesByCode.Remove(e.Code, i)) throw new UnreachableException("致命错误，请停用并报告异常A");
        if (indexes.Count == 0) _entriesByText.Remove(e.Text);

        _entries[i] = e with { Text = "" }; // 标记死亡
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
    /// <param name="reorder"> true：词条先按Code升序，再按Num升序重排，空行丢弃，注释原序排在末尾；false：保持原有行，新词条按Code升序排在末尾 </param>
    public void Save(string? path, bool reorder) {
        using StreamWriter writer = new(path ?? _path);
        writer.NewLine = "\n";
        writer.WriteLine(_header);

        if (reorder) {
            foreach (var e in _entries.Where(static e => e.Text.Length > 0)
                .OrderBy(static e => (e.Code, e.Num - 1))) // 新词条后置
                writer.WriteLine(e.Serialize(_cols));
            foreach (var l in _rawLines.Where(static l => l.Content is {}))
                writer.WriteLine(l.Content);
        } else {
            using var en = _entries.Where(static e => e is { Num: > 0, Text.Length: > 0 })
                .GetEnumerator();
            using var rn = _rawLines.GetEnumerator();
            var hasE = en.MoveNext();
            var hasR = rn.MoveNext();
            while (hasE || hasR)
                if (hasE && (!hasR || en.Current.Num <= rn.Current.Num)) {
                    writer.WriteLine(en.Current.Serialize(_cols));
                    hasE = en.MoveNext();
                } else {
                    writer.WriteLine(rn.Current.Content);
                    hasR = rn.MoveNext();
                }
            foreach (var e in _entries.Where(static e => e is { Num: 0, Text.Length: > 0 })
                .OrderBy(static e => e.Code))
                writer.WriteLine(e.Serialize(_cols));
        }

        Mod = false;
    }
}
