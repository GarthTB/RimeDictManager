namespace RimeDictManager.Models;

using System.IO;
using Services;
using static System.Runtime.InteropServices.CollectionsMarshal;
using FmtEx = FormatException;

internal sealed class Dict {
    private readonly CodeTrie _codeTrie;
    private readonly List<Entry> _entries;
    private readonly List<string?> _header;
    private readonly Action<bool> _onModifiedChanged;
    private readonly string _path;
    private readonly List<RawLine> _rawLines;
    private readonly Dictionary<string, List<int>> _textDict;

    public Dict(string path, Action<bool> onModifiedChanged) {
        using StreamReader sr = new(_path = path);
        string? l;

        _header = new(64);
        for (var pos = 0; (l = sr.ReadLine()) is {}; _header.Add(l)) {
            if (_header.Count - pos > 1023) throw new FmtEx("词库文件头过长，疑似未闭合");
            if (string.IsNullOrWhiteSpace(l))
                l = null;
            else if (l == "---")
                pos = _header.Count + 1;
            else if (l == "...") {
                LineCodec.SetCols(_header[pos..]);
                break;
            }
        }
        if (sr.EndOfStream) throw new FmtEx("词库文件头缺失或未闭合");

        _rawLines = new(64);
        _entries = new(16384);
        for (var num = (uint)_header.Count + 2; (l = sr.ReadLine()) is {}; num++)
            if (string.IsNullOrWhiteSpace(l))
                _rawLines.Add(new(num, null));
            else if (l[0] == '#')
                _rawLines.Add(new(num, l));
            else
                _entries.Add(LineCodec.Deserialize(num, l));
        if (_entries.Count == 0) throw new FmtEx("词库为空");
        Count = _entries.Count;

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

    public int Count { get; private set; }

    private bool Modified {
        set {
            if (field != value) _onModifiedChanged(field = value);
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
        throw new NotImplementedException();

        Count--;
        Modified = true;
    }

    public void ForEachByWord(string word, Func<Entry, bool> doWhile) =>
        throw new NotImplementedException();

    public void ForEachByCode(string? code, bool exact, Func<Entry, bool> doWhile) =>
        throw new NotImplementedException();

    public void Save(string? path, bool reorder) {
        throw new NotImplementedException();

        using StreamWriter sw = new(path ?? _path);
        sw.NewLine = "\n";

        Modified = false;
    }
}
