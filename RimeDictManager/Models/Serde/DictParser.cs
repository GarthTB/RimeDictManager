namespace RimeDictManager.Models.Serde;

using System.Text;
using System.Text.Json.Serialization;
using Common;
using SharpYaml;
using SharpYaml.Serialization;
using FmtEx = FormatException;

public static class DictParser {
    public static string ReadHeader(
        TextReader reader,
        string path,
        out string name,
        out Column[] cols,
        out uint num) {
        StringBuilder s = new(1024);
        var start = 0;
        num = 1;
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (s.Length > 16384) throw new FmtEx($"文件头过长，疑似缺失或未闭合\n文件：{path}");
            if (string.IsNullOrWhiteSpace(l))
                s.Append('\n');
            else if (l == "---")
                start = s.Append(l).Append('\n').Length;
            else if (l == "...") {
                s.Append(l);
                num++;
                break;
            } else
                s.Append(l).Append('\n');
        }
        var raw = s.ToString();
        if (raw.Length < 3 || raw.AsSpan(^3..) is not "...")
            throw new FmtEx($"文件头缺失或未闭合\n文件：{path}");

        try {
            var yaml = raw.AsSpan(start..^3);
            var header = YamlSerializer.Deserialize(yaml, HeaderContext.Default.Header)
                      ?? throw new FmtEx("YAML解析器返回NULL");
            name = header.Name ?? TrimExt(Path.GetFileName(path));
            cols = ParseCols(header.Columns);
        } catch (Exception ex) { throw new FmtEx($"文件头解析失败\n文件：{path}", ex); }

        return raw;
    }

    private static string TrimExt(string name) =>
        name.AsSpan().EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? name[..^FileTypes.DictExt.Length]
            : name;

    private static Column[] ParseCols(string[]? cols) {
        if (cols is null) return Columns.Default;
        if (cols.Length == 0) throw new FmtEx("列定义为空");
        if (cols.Length > Columns.Cnt) throw new FmtEx($"词库超过{Columns.Cnt}列");

        var vals = (stackalloc Column[cols.Length]);
        var mask = 0;
        for (var i = 0; i < cols.Length; i++) {
            var s = cols[i]; // Yaml解析器会Trim
            if (!Enum.TryParse(s, true, out Column col)) throw new FmtEx($"列名无效：'{s}'");
            vals[i] = col;
            var bit = 1 << (int)col;
            if ((mask & bit) != 0) throw new FmtEx($"列名重复：'{s}'");
            mask |= bit;
        }

        if ((mask & (1 << (int)Column.Text)) == 0) throw new FmtEx("未定义文本列");
        return vals.ToArray();
    }
}

public sealed class Header {
    public string? Name { get; init; }
    public string[]? Columns { get; init; }
}

[YamlSerializable(typeof(Header)),
 YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class HeaderContext: YamlSerializerContext;
