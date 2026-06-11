namespace RimeDictManager.Utils;

using System.Diagnostics.CodeAnalysis;
using Models;
using static Col;
using Cols = IReadOnlyList<Col>;
using FmtEx = FormatException;

public static class LineCodec {
    public static bool Deserialize(
        string l,
        uint num,
        Cols cols,
        [NotNullWhen(true)] out EntryLine? e,
        [NotNullWhen(false)] out RawLine? r) {
        if (string.IsNullOrWhiteSpace(l)) {
            e = null;
            r = new(num, null);
            return false;
        }
        if (l[0] == '#') {
            e = null;
            r = new(num, l);
            return false;
        }

        var cnt = cols.Count;
        var parts = l.Split('\t', cnt + 1);
        if (parts.Length > cnt) throw new FmtEx($"第{num}行词条超过{cnt}列");

        var vals = new string?[cnt];
        for (var i = 0; i < parts.Length; i++) vals[(int)cols[i]] = TrimOrNull(parts[i]);
        if (vals[(int)Text] is not {} t) throw new FmtEx($"第{num}行词条文本为空");

        e = new(num, t, vals[(int)Code], vals[(int)Weight], vals[(int)Stem]);
        r = null;
        return true;
    }

    public static string Serialize(this EntryLine e, Cols cols) {
        var vals = cols.Select(col => col switch {
                Text => e.Text, Code => e.Code, Weight => e.Weight, _ => e.Stem
            })
            .ToArray();
        var last = Array.FindLastIndex(vals, static s => s is {});
        return string.Join('\t', vals.AsSpan(..(last + 1)));
    }

    public static EntryLine? TryNewEntry(
        string text,
        string? code,
        string? weight,
        string? stem,
        Cols cols) {
        if (TrimOrNull(text) is not {} t) return null;
        var c = TrimOrNull(code);
        if (c is {} && !cols.Contains(Code)) return null;
        var w = TrimOrNull(weight);
        if (w is {} && !cols.Contains(Weight)) return null;
        var s = TrimOrNull(stem);
        if (s is {} && !cols.Contains(Stem)) return null;
        return new(0, t, c, w, s);
    }

    private static string? TrimOrNull(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : s.Trim();
}
