namespace RimeDictManager.Services.Data;

using Models;
using ZLinq;
using static Models.Col;
using FmtEx = FormatException;

public static class LineCodec {
    public static bool Deserialize(
        string l,
        uint num,
        IReadOnlyList<Col> cols,
        out EntryLine e,
        out RawLine r) {
        if (string.IsNullOrWhiteSpace(l)) {
            e = default;
            r = new(num, null);
            return false;
        }
        if (l[0] == '#') {
            e = default;
            r = new(num, l);
            return false;
        }

        var cnt = cols.Count;
        var parts = l.Split('\t', cnt + 1);
        if (parts.Length > cnt) throw new FmtEx($"第{num}行词条超过{cnt}列");

        var vals = new string?[4];
        for (var i = 0; i < parts.Length; i++) vals[(int)cols[i]] = TrimOrNull(parts[i]);
        if (vals[(int)Text] is not {} t) throw new FmtEx($"第{num}行词条文本为空");

        e = new(num, t, vals[(int)Code], vals[(int)Weight], vals[(int)Stem]);
        r = default;
        return true;
    }

    public static string Serialize(this EntryLine e, IReadOnlyList<Col> cols) {
        var vals = cols.AsValueEnumerable()
            .Select(col =>
                col switch { Text => e.Text, Code => e.Code, Weight => e.Weight, _ => e.Stem })
            .ToArray();
        var last = Array.FindLastIndex(vals, static x => x is {});
        return string.Join('\t', vals.AsSpan(..(last + 1)));
    }

    public static bool TryNewEntry(
        uint num,
        string text,
        string? code,
        string? weight,
        string? stem,
        IReadOnlyList<Col> cols,
        out EntryLine e) {
        var t = TrimOrNull(text);
        if (t is null) goto Fail;
        var c = TrimOrNull(code);
        if (c is {} && !cols.Contains(Code)) goto Fail;
        var w = TrimOrNull(weight);
        if (w is {} && !cols.Contains(Weight)) goto Fail;
        var s = TrimOrNull(stem);
        if (s is {} && !cols.Contains(Stem)) goto Fail;

        e = new(num, t, c, w, s);
        return true;

    Fail:
        e = default;
        return false;
    }

    private static string? TrimOrNull(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : s.Trim();
}
