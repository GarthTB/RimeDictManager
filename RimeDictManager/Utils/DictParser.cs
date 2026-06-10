// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace RimeDictManager.Utils;

using System.Text;
using Common;
using SharpYaml;
using FmtEx = FormatException;

public enum Col: byte { Text, Code, Weight, Stem }

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Header {
    public string? name { get; set; }
    public string[]? columns { get; set; }
}

public static class DictParser {
    private static readonly Col[] DefaultCols = [Col.Text, Col.Code, Col.Weight, Col.Stem];

    public static Header ReadHeader(TextReader reader, out string raw, out uint num) {
        StringBuilder s = new(1024);
        var start = 0;
        num = 1u;
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (s.Length > 16384) throw new FmtEx("词库文件头过长，疑似缺失或未闭合");
            if (string.IsNullOrWhiteSpace(l))
                s.Append('\n');
            else if (l == "---")
                start = s.Append(l).Append('\n').Length;
            else if (l == "...") {
                var yaml = s.ToString(start, s.Length - start);
                var header = YamlSerializer.Deserialize<Header>(yaml)
                          ?? throw new FmtEx("词库文件头无法作为YAML解析");
                raw = s.Append(l).ToString();
                num++;
                return header;
            } else
                s.Append(l).Append('\n');
        }
        throw new FmtEx("词库文件头缺失或未闭合");
    }

    public static string GetName(string path) =>
        path.EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? path[..^FileTypes.DictExt.Length]
            : path;

    public static Col[] ParseCols(string[]? cols) {
        if (cols is null) return DefaultCols;
        if (cols.Length == 0) throw new FmtEx("词库列定义为空");
        if (cols.Length > 4) throw new FmtEx("词库超过4列");
        var result = Array.ConvertAll(
            cols,
            static s => Enum.TryParse(s.Trim(), true, out Col col)
                ? col
                : throw new FmtEx($"词库列名无效：'{s}'"));
        return !result.Contains(Col.Text)
            ? throw new FmtEx("词库未定义文本列")
            : result.Distinct().Count() < result.Length
                ? throw new FmtEx("词库有重复列名")
                : result;
    }
}
