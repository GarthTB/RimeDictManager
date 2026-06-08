namespace RimeDictManager.Utils;

using Models;
using SharpYaml;
using static Col;
using Cols = IReadOnlyList<Col>;
using FmtEx = FormatException;

public enum Col: byte { Text, Code, Weight, Stem }

public static class Codec {
    private static readonly Cols DefaultCols = [Text, Code, Weight, Stem];

    public static void ParseHeader(string yaml, string path, out string name, out Cols cols) {
        var info = YamlSerializer.Deserialize<HeaderInfo>(yaml)
                ?? throw new FmtEx("词库文件头无法作为YAML解析");

        const string ext = ".dict.yaml";
        name = info.Name
            ?? (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
                   ? path[..^ext.Length]
                   : path);

        if (info.Cols is not {} sCols) {
            cols = DefaultCols;
            return;
        }
        if (sCols.Length == 0) throw new FmtEx("词库列定义为空");
        if (sCols.Length > 4) throw new FmtEx("词库超过4列");
        var eCols = Array.ConvertAll(
            sCols,
            static s => Enum.TryParse(TrimOrNull(s), true, out Col col)
                ? col
                : throw new FmtEx($"词库列名无效：'{s}'"));
        cols = !eCols.Contains(Text)
            ? throw new FmtEx("词库未定义文本列")
            : eCols.Distinct().Count() != eCols.Length
                ? throw new FmtEx("词库有重复列名")
                : eCols;
    }

    public static EntryLine Deserialize(uint num, string line, Cols cols) {
        var cnt = cols.Count;
        var parts = line.Split('\t', cnt + 1);
        if (parts.Length > cnt) throw new FmtEx($"词条超过{cnt}列");

        var vals = new string?[4];
        for (var i = 0; i < parts.Length; i++) vals[(int)cols[i]] = TrimOrNull(parts[i]);

        return vals[(int)Text] is {} t
            ? new(num, t, vals[(int)Code], vals[(int)Weight], vals[(int)Stem])
            : throw new FmtEx("词条文本为空");
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

    private sealed class HeaderInfo {
        public string? Name { get; }
        public string[]? Cols { get; }
    }
}
