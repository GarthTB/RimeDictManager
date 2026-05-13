namespace RimeDictManager.Models;

using System.IO;
using static System.Runtime.InteropServices.CollectionsMarshal;
using static Services.LineCodec;
using FmtEx = FormatException;
using NeverEx = System.Diagnostics.UnreachableException;

internal sealed class Dict {
    private readonly CodeTrie _codeTrie;
    private readonly List<Entry> _entries;
    private readonly List<string?> _header;
    private readonly Action _onModifiedChanged;
    private readonly string _path;
    private readonly List<RawLine> _rawLines;
    private readonly Dictionary<string, List<int>> _textDict;

    public Dict(string path, Action onModifiedChanged) {
        using StreamReader reader = new(_path = path);
        string? l;

        _header = new(64);
        for (var pos = 0; (l = reader.ReadLine()) is {}; _header.Add(l)) {
            if (_header.Count - pos > 1023) throw new FmtEx("词库文件头过长，疑似缺失");
            if (string.IsNullOrWhiteSpace(l))
                l = null;
            else if (l == "---")
                pos = _header.Count + 1;
            else if (l == "...") {
                SetCols(_header[pos..]);
                break;
            }
        }
        if (reader.EndOfStream) throw new FmtEx("词库文件头缺失或未闭合");

        _rawLines = new(64);
        _entries = new(16384);
        for (var num = (uint)_header.Count + 2; (l = reader.ReadLine()) is {}; num++)
            if (string.IsNullOrWhiteSpace(l))
                _rawLines.Add(new(num, null));
            else if (l[0] == '#')
                _rawLines.Add(new(num, l));
            else
                _entries.Add(Deserialize(num, l));
        if (_entries.Count == 0) throw new FmtEx("词库为空");
        Count = (uint)_entries.Count;

        _textDict = new(16384);
        _codeTrie = new(65536);
        for (var i = 0; i < _entries.Count; i++) {
            var e = _entries[i];
            ref var indexes = ref GetValueRefOrAddDefault(_textDict, e.Text, out var exists);
            if (exists)
                indexes!.Add(i);
            else
                indexes = [i];
            _codeTrie.Insert(e.Code, i);
        }

        _onModifiedChanged = onModifiedChanged;
    }

    public uint Count { get; private set; }

    public bool Modified {
        get;
        private set {
            if (field == value) return;
            field = value;
            _onModifiedChanged();
        }
    }

    public void Insert(Entry e) {
        _entries.Add(e);
        var i = _entries.Count - 1;
        ref var indexes = ref GetValueRefOrAddDefault(_textDict, e.Text, out var exists);
        if (exists)
            indexes!.Add(i);
        else
            indexes = [i];
        _codeTrie.Insert(e.Code, i);

        Count++;
        Modified = true;
    }

    public bool Remove(Entry e) {
        if (!_textDict.TryGetValue(e.Text, out var indexes)) return false;
        var j = indexes.FindIndex(i => _entries[i] == e);
        if (j < 0) return false;

        var i = indexes[j];
        indexes[j] = indexes[^1];
        indexes.RemoveAt(indexes.Count - 1);

        if (!_codeTrie.Remove(e.Code, i)) throw new NeverEx("程序内部不一致，请停用并报告异常");
        if (indexes.Count == 0) _textDict.Remove(e.Text);

        _entries[i] = e with { Text = "" };
        Count--;
        return Modified = true;
    }

    public void ForEachByText(string text, Func<Entry, bool> doWhile) {
        if (!_textDict.TryGetValue(text, out var indexes)) return;
        foreach (var i in indexes)
            if (!doWhile(_entries[i]))
                break;
    }

    public void ForEachByCode(string? code, bool exact, Func<Entry, bool> doWhile) =>
        _codeTrie.ForEachByKey(code, exact, i => doWhile(_entries[i]));

    /// <summary> 保存词库（不迁移路径） </summary>
    /// <param name="path"> null则覆写 </param>
    /// <param name="reorder"> true：词条先按Code升序，再按Num升序重排，空行丢弃，注释原序排在末尾；false：保持原有行，新词条按Code升序排在末尾 </param>
    public void Save(string? path, bool reorder) {
        using StreamWriter writer = new(path ?? _path);
        writer.NewLine = "\n";
        foreach (var l in _header) writer.WriteLine(l);
        writer.WriteLine("...");

        if (reorder) {
            foreach (var e in _entries.Where(static e => e.Text.Length > 0)
                .OrderBy(static e => (e.Code, e.Num)))
                writer.WriteLine(Serialize(e));
            foreach (var l in _rawLines.Where(static l => l.Content is {}))
                writer.WriteLine(l.Content);
        } else {
            var oldEntries = _entries.Where(static e => e is { Num: > 0, Text.Length: > 0 })
                .Select(static e => (e.Num, (string?)Serialize(e)));
            var comments = _rawLines.Select(static l => (l.Num, l.Content));
            foreach (var (_, l) in oldEntries.Concat(comments).OrderBy(static x => x.Num))
                writer.WriteLine(l);
            foreach (var e in _entries.Where(static e => e is { Num: 0, Text.Length: > 0 })
                .OrderBy(static e => e.Code))
                writer.WriteLine(Serialize(e));
        }

        Modified = false;
    }
}
