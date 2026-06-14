namespace RimeDictManager.Models.Serde;

using System.Text;
using System.Text.Json.Serialization;
using Common;
using SharpYaml;
using SharpYaml.Serialization;
using FmtEx = FormatException;

public static class DictParser {
    private static readonly Column[] DefaultCols
        = [Column.Text, Column.Code, Column.Weight, Column.Stem];

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
        if (raw.AsSpan(^3..) is not "...") throw new FmtEx($"文件头缺失或未闭合\n文件：{path}");

        var header = YamlSerializer.Deserialize(raw.AsSpan(start..^3), HeaderContext.Default.Header)
                  ?? throw new FmtEx($"文件头无法作为YAML解析\n文件：{path}");
        name = header.Name ?? TrimExt(Path.GetFileName(path));
        try { cols = ParseCols(header.Columns); } catch (Exception ex) {
            throw new FmtEx($"无法解析列定义\n文件：{path}", ex);
        }
        return raw;
    }

    private static string TrimExt(string name) =>
        name.EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? name[..^FileTypes.DictExt.Length]
            : name;

    private static Column[] ParseCols(string[]? cols) {
        if (cols is null) return DefaultCols;
        if (cols.Length == 0) throw new FmtEx("列定义为空");
        if (cols.Length > 4) throw new FmtEx("词库超过4列");
        HashSet<Column> result = new(4);
        foreach (var s in cols) {
            if (!Enum.TryParse(s.Trim(), true, out Column col)) throw new FmtEx($"列名无效：'{s}'");
            if (!result.Add(col)) throw new FmtEx($"有重复列名'{s}'");
        }
        return result.Contains(Column.Text)
            ? result.ToArray()
            : throw new FmtEx("未定义文本列");
    }
}

public sealed class Header {
    public string? Name { get; init; }
    public string[]? Columns { get; init; }
}

[YamlSerializable(typeof(Header)),
 YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class HeaderContext: YamlSerializerContext;
