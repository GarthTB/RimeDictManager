namespace RimeDictManager.Models.Serde;

using System.Diagnostics;
using static Column;

public static class LineCodec {
    public static bool Deserialize(
        string line,
        uint num,
        IReadOnlyList<Column> cols,
        out EntryLine e,
        out RawLine r) {
        var span = line.AsSpan().TrimStart(' ');
        if (span.Length == 0 || span[0] == '#') {
            e = default;
            r = new(num, line);
            return false;
        }

        var cnt = cols.Count;
        var parts = line.Split('\t', cnt + 1, StringSplitOptions.TrimEntries);
        if (parts.Length > cnt) throw new FormatException($"第{num}行词条超过定义的{cnt}列");

        string text = "", code = "", weight = "", stem = "";
        for (var i = 0; i < parts.Length; i++)
            switch (cols[i]) {
            case Text: text = parts[i]; break;
            case Code: code = parts[i]; break;
            case Weight: weight = parts[i]; break;
            case Stem: stem = parts[i]; break;
            default: throw new UnreachableException();
            }
        if (text.Length == 0) throw new FormatException($"第{num}行词条文本为空");

        e = new(num, text, code, weight, stem);
        r = default;
        return true;
    }

    public static string Serialize(this EntryLine e, IReadOnlyList<Column> cols) {
        var cnt = cols.Count;
        for (var i = cnt - 1; e[cols[i]].Length == 0; i--) cnt--;
        var vals = new string[cnt];
        for (var i = 0; i < cnt; i++) vals[i] = e[cols[i]];
        return string.Join('\t', vals);
    }

    public static bool TryNewEntry(
        uint num,
        string text,
        string code,
        string weight,
        string stem,
        IReadOnlyList<Column> cols,
        out EntryLine e) {
        if (string.IsNullOrWhiteSpace(text)) goto Fail;

        // ReSharper disable LoopCanBeConvertedToQuery
        var mask = 0;
        foreach (var c in cols) mask |= 1 << (int)c;

        if ((string.IsNullOrWhiteSpace(code) || (mask & (1 << (int)Code)) != 0)
         && (string.IsNullOrWhiteSpace(weight) || (mask & (1 << (int)Weight)) != 0)
         && (string.IsNullOrWhiteSpace(stem) || (mask & (1 << (int)Stem)) != 0)) {
            e = new(num, text.Trim(), code.Trim(), weight.Trim(), stem.Trim());
            return true;
        }

    Fail:
        e = default;
        return false;
    }
}
