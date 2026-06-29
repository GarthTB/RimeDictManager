namespace RimeDictManager.Services;

using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Common;
using Models;
using SharpYaml;
using SharpYaml.Serialization;
using static System.Runtime.InteropServices.CollectionsMarshal;
using FmtEx = FormatException;

public static class DictIO {
    public static async Task<Dict> LoadDictAsync(string path) {
        using DictReader reader = new(path);
        var (header, name) = await reader.ReadHeaderAsync();
        List<EntryLine> entries = new(4096);
        List<RawLine> rawLines = new(64);
        var num = await reader.ReadLinesAsync(rawLines.Add, entries.Add);
        return new(path, header, name, reader.Cols, rawLines, entries, num);
    }

    public static async Task<SingleDict> LoadSingleDictAsync(string path) {
        using DictReader reader = new(path);
        var (_, name) = await reader.ReadHeaderAsync();
        if (!reader.Cols.Contains(DictCol.Code)) throw new FmtEx("单字码表未定义编码列");
        Dictionary<char, List<string>> entries = new(4096);
        await reader.ReadLinesAsync(
            null,
            e => {
                if (e.Text is not [var c]) return;
                ref var codes = ref GetValueRefOrAddDefault(entries, c, out var exists);
                if (exists)
                    codes!.Add(e.Code);
                else
                    codes = [e.Code];
            });
        return new(path, name, entries);
    }

    /// <summary> 保存词库（不迁移路径） </summary>
    /// <param name="dict"> 词库 </param>
    /// <param name="path"> 目标路径：null 则覆写 </param>
    /// <param name="reorder"> true：词条先按 Code 升序再按 Num 升序重排，非词条行按原序排在末尾；false：保持原有行，新词条按插入顺序排在末尾 </param>
    public static async Task SaveAsync(Dict dict, string? path, bool reorder) {
        await using StreamWriter writer = new(path ?? dict.Path);
        writer.NewLine = "\n";
        await writer.WriteLineAsync(dict.Header);

        if (reorder) {
            foreach (var e in dict.Entries.OrderBy(static e => (e.Code, e.Num)))
                await writer.WriteLineAsync(e.Format(dict.Cols));
            foreach (var r in dict.RawLines) await writer.WriteLineAsync(r.Content);
        } else {
            using var entries = dict.Entries.OrderBy(static e => e.Num).GetEnumerator();
            using var rawLines = dict.RawLines.GetEnumerator();
            var anyE = entries.MoveNext();
            var anyR = rawLines.MoveNext();
            while (anyE || anyR)
                if (anyE && (!anyR || entries.Current.Num <= rawLines.Current.Num)) {
                    await writer.WriteLineAsync(entries.Current.Format(dict.Cols));
                    anyE = entries.MoveNext();
                } else {
                    await writer.WriteLineAsync(rawLines.Current.Content);
                    anyR = rawLines.MoveNext();
                }
        }

        dict.NotifySaved();
    }
}

public sealed class Header {
    public string? Name { get; init; }
    public string[]? Columns { get; init; }
}

[YamlSerializable(typeof(Header)),
 YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class HeaderContext: YamlSerializerContext;

file sealed class DictReader(string path): IDisposable {
    private readonly StreamReader _reader = new(path);
    private uint _num = 1;
    public IReadOnlyList<DictCol> Cols { get; private set; } = null!;

    public void Dispose() => _reader.Dispose();

    public async Task<(string, string)> ReadHeaderAsync() {
        try {
            StringBuilder s = new(1024);
            var start = 0;
            for (; await _reader.ReadLineAsync() is {} l; _num++) {
                if (s.Length > 16384) throw new FmtEx("文件头过长，疑似缺失或未闭合");
                if (string.IsNullOrWhiteSpace(l)) {
                    s.Append('\n');
                    continue;
                }
                l = l.TrimEnd();
                if (l == "---")
                    start = s.Append(l).Append('\n').Length;
                else if (l == "...") {
                    s.Append(l);
                    _num++;
                    break;
                } else
                    s.Append(l).Append('\n');
            }
            var raw = s.ToString();
            if (raw.Length < 3 || !raw.AsSpan().EndsWith("...")) throw new FmtEx("文件头缺失或未闭合");

            var header = YamlSerializer.Deserialize(raw[start..^3], HeaderContext.Default.Header)
                      ?? throw new FmtEx("YAML 解析器返回 NULL，文件头可能为空");
            var name = header.Name ?? TrimExt(Path.GetFileName(path));
            ParseCols(header.Columns);

            return (raw, name);
        } catch (Exception ex) { throw new FmtEx($"文件头解析失败\n文件：{path}", ex); }
    }

    public async Task<uint> ReadLinesAsync(Action<RawLine>? fr, Action<EntryLine> fe) {
        for (var canComment = true; await _reader.ReadLineAsync() is {} l; _num++) {
            if (string.IsNullOrWhiteSpace(l)) {
                fr?.Invoke(new(_num, ""));
                continue;
            }
            l = l.TrimEnd();
            if (l[0] == '#' && canComment) {
                if (l == "# no comment") canComment = false;
                fr?.Invoke(new(_num, l));
                continue;
            }

            string text = "", code = "", weight = "", stem = "";
            for (int i = 0, col = 0, start = 0; i <= l.Length; i++) {
                if (i < l.Length && l[i] != '\t') continue;
                if (col >= Cols.Count) throw new FmtEx($"第{_num}行词条列数超出定义");
                var v = l[start..i];
                switch (Cols[col]) {
                case DictCol.Text: text = v; break;
                case DictCol.Code: code = v; break;
                case DictCol.Weight: weight = v; break;
                case DictCol.Stem: stem = v; break;
                default: throw new UnreachableException();
                }
                col++;
                start = i + 1;
            }
            if (EntryLine.TryNew(_num, text, code, weight, stem, Cols, out var e))
                fe(e);
            else
                throw new FmtEx($"第{_num}行词条无效");
        }

        return _num;
    }

    private static string TrimExt(string name) =>
        name.AsSpan().EndsWith(FileTypes.DictExt, StringComparison.OrdinalIgnoreCase)
            ? name[..^FileTypes.DictExt.Length]
            : name;

    private void ParseCols(string[]? cols) {
        if (cols is null) {
            Cols = DictCols.Default;
            return;
        }
        if (cols.Length == 0) throw new FmtEx("列定义为空");

        var vals = new DictCol[cols.Length];
        var mask = 0;
        for (var i = 0; i < cols.Length; i++) {
            var s = cols[i]; // Yaml 解析器会 Trim
            if (!Enum.TryParse(s, true, out DictCol col)) throw new FmtEx($"列名无效：'{s}'");
            vals[i] = col;

            var bit = 1 << (int)col;
            if ((mask & bit) != 0) throw new FmtEx($"列名重复：'{s}'");
            mask |= bit;
        }
        if ((mask & (1 << (int)DictCol.Text)) == 0) throw new FmtEx("未定义文本列");

        Cols = vals;
    }
}
