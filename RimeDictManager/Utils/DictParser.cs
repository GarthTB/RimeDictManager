// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace RimeDictManager.Utils;

using System.Text;
using Common;
using SharpYaml;
using FmtEx = FormatException;

public enum Col: byte { Text, Code, Weight, Stem }

// ReSharper disable once ClassNeverInstantiated.Local
file sealed class Header {
    public string? name { get; set; }
    public string[]? columns { get; set; }
}

public static class DictParser {
    private static readonly Col[] DefaultCols = [Col.Text, Col.Code, Col.Weight, Col.Stem];

    public static string ReadHeader(
        string path,
        TextReader reader,
        out string name,
        out Col[] cols,
        out uint num) {
        StringBuilder s = new(1024);
        var start = 0;
        num = 1;
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (s.Length > 16384) throw new FmtEx($"词库：{path}\n文件头过长，疑似缺失或未闭合");
            if (string.IsNullOrWhiteSpace(l))
                s.Append('\n');
            else if (l == "---")
                start = s.Append(l).Append('\n').Length;
            else if (l == "...") {
                var yaml = s.ToString(start, s.Length - start);
                var header = YamlSerializer.Deserialize<Header>(yaml)
                          ?? throw new FmtEx($"词库：{path}\n文件头无法作为YAML解析");
                name = header.name ?? GetName(path);
                cols = ParseCols(name, header.columns);
                num++;
                return s.Append(l).ToString();
            } else
                s.Append(l).Append('\n');
        }
        throw new FmtEx($"词库：{path}\n文件头缺失或未闭合");
    }

    private static string GetName(string path) =>
        path.EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? path[..^FileTypes.DictExt.Length]
            : path;

    private static Col[] ParseCols(string name, string[]? cols) {
        if (cols is null) return DefaultCols;
        if (cols.Length == 0) throw new FmtEx($"词库：{name}\n列定义为空");
        if (cols.Length > 4) throw new FmtEx($"词库：{name}\n超过4列");
        var result = Array.ConvertAll(
            cols,
            s => Enum.TryParse(s.Trim(), true, out Col col)
                ? col
                : throw new FmtEx($"词库：{name}\n列名无效：'{s}'"));
        return !result.Contains(Col.Text)
            ? throw new FmtEx($"词库：{name}\n未定义文本列")
            : result.Distinct().Count() < result.Length
                ? throw new FmtEx($"词库：{name}\n有重复列名")
                : result;
    }
}
