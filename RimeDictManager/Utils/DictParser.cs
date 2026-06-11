namespace RimeDictManager.Utils;

using System.Text;
using Common;
using SharpYaml;
using FmtEx = FormatException;

public enum Col: byte { Text, Code, Weight, Stem }

// ReSharper disable once ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
file sealed record Header(string? name, string[]? columns);

public static class DictParser {
    private static readonly Col[] DefaultCols = [Col.Text, Col.Code, Col.Weight, Col.Stem];

    public static string ReadHeader(
        TextReader reader,
        string path,
        out string name,
        out Col[] cols,
        out uint num) {
        StringBuilder s = new(1024);
        var start = 0;
        num = 1;
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (s.Length > 16384) throw new FmtEx($"文件头过长，疑似缺失或未闭合\nRIME 词库：{path}");
            if (string.IsNullOrWhiteSpace(l))
                s.Append('\n');
            else if (l == "---") {
                start = s.Length;
                s.Append(l).Append('\n');
            } else if (l == "...") {
                s.Append(l);
                num++;
                break;
            } else
                s.Append(l).Append('\n');
        }
        var raw = s.ToString();
        if (raw is not [.., '.', '.', '.']) throw new FmtEx($"文件头缺失或未闭合\nRIME 词库：{path}");

        var header = YamlSerializer.Deserialize<Header>(raw.AsSpan(start))
                  ?? throw new FmtEx($"文件头无法作为YAML解析\nRIME 词库：{path}");
        name = header.name ?? GetName(path);
        cols = ParseCols(name, header.columns);
        return raw;
    }

    private static string GetName(string path) =>
        path.EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? path[..^FileTypes.DictExt.Length]
            : path;

    private static Col[] ParseCols(string name, string[]? cols) {
        if (cols is null) return DefaultCols;
        if (cols.Length == 0) throw new FmtEx($"列定义为空\nRIME 词库：{name}");
        if (cols.Length > 4) throw new FmtEx($"词库超过4列\nRIME 词库：{name}");
        HashSet<Col> result = new(4);
        foreach (var s in cols) {
            if (!Enum.TryParse(s.Trim(), true, out Col col))
                throw new FmtEx($"列名无效：'{s}'\nRIME 词库：{name}");
            if (!result.Add(col)) throw new FmtEx($"有重复列名'{s}'\nRIME 词库：{name}");
        }
        return result.Contains(Col.Text)
            ? result.ToArray()
            : throw new FmtEx($"未定义文本列\nRIME 词库：{name}");
    }
}
