namespace RimeDictManager.Services;

using Models;
using FmtEx = FormatException;

internal static class LineCodec {
    private static Col[] _cols = [];

    public static void SetCols(List<string?> metaLines) {
        var i = metaLines.FindIndex(static l => l.AsSpan().TrimStart().StartsWith("columns:"));
        if (i < 0) {
            _cols = [Col.Text, Col.Code, Col.Weight, Col.Stem];
            return;
        }

        List<Col> cols = new(5);
        var indent = metaLines[i]!.Length - metaLines[i].AsSpan().TrimStart().Length;
        while (++i < metaLines.Count) {
            var line = metaLines[i].AsSpan();

            var item = line.TrimStart();
            if (item.IsEmpty || item[0] == '#') continue;
            if (line.Length - item.Length <= indent) break;
            if (item[0] != '-') throw new FmtEx("列定义项缺失'-'");
            if (cols.Count >= 4) throw new FmtEx("词库超过4列");

            var name = (item.IndexOf(" #") is > 1 and var j
                ? item[1..j]
                : item[1..]).Trim();
            if (!Enum.TryParse(name, true, out Col col)) throw new FmtEx($"列名无效：'{name}'");
            if (cols.Contains(col)) throw new FmtEx($"列名重复：'{name}'");

            cols.Add(col);
        }
        if (cols.Count == 0) throw new FmtEx("列定义为空");
        if (!cols.Contains(Col.Text)) throw new FmtEx("未定义文本列");

        _cols = [..cols];
    }

    public static Entry Deserialize(uint num, string line) {
        var parts = line.Split('\t', _cols.Length + 1);
        if (parts.Length > _cols.Length) throw new FmtEx($"词条超过{_cols.Length}列");

        var vals = new string?[4];
        for (var i = 0; i < parts.Length; i++) vals[(int)_cols[i]] = TrimOrNull(parts[i]);

        return vals[(int)Col.Text] is {} t
            ? new(num, t, vals[(int)Col.Code], vals[(int)Col.Weight], vals[(int)Col.Stem])
            : throw new FmtEx("词条文本为空");
    }

    public static Entry? TryNewEntry(string text, string? code, string? weight, string? stem) {
        if (TrimOrNull(text) is not {} t) return null;
        var c = TrimOrNull(code);
        if (c is {} && !_cols.Contains(Col.Code)) return null;
        var w = TrimOrNull(weight);
        if (w is {} && !_cols.Contains(Col.Weight)) return null;
        var s = TrimOrNull(stem);
        if (s is {} && !_cols.Contains(Col.Stem)) return null;
        return new(0, t, c, w, s);
    }

    public static string Serialize(Entry e) {
        var vals = _cols.Select(col => col switch {
                Col.Text => e.Text, Col.Code => e.Code, Col.Weight => e.Weight, _ => e.Stem
            })
            .ToArray();
        var last = Array.FindLastIndex(vals, static s => s is {});
        return string.Join('\t', vals.AsSpan(..(last + 1)));
    }

    private static string? TrimOrNull(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : s.Trim();

    private enum Col: byte { Text, Code, Weight, Stem }
}
