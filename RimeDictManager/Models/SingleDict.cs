namespace RimeDictManager.Models;

using System.Collections.Frozen;
using Utils;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class SingleDict {
    public SingleDict(string path) {
        using StreamReader reader = new(path);
        DictParser.ReadHeader(reader, path, out var name, out var cols, out var num);
        if (!cols.Contains(Col.Code)) throw new FormatException("单字码表未定义编码列");
        Name = name;
        Path = path;
        Dictionary<char, List<string>> entries = new(4096);
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (!LineCodec.Deserialize(l, num, cols, out var e, out _)
             || e is not { Text: [var c], Code: {} code })
                continue;
            ref var codes = ref GetValueRefOrAddDefault(entries, c, out var exists);
            if (exists)
                codes!.Add(code);
            else
                codes = [code];
        }
        Entries = entries.ToFrozenDictionary();
    }

    public IReadOnlyDictionary<char, List<string>> Entries { get; }
    public string Name { get; }
    public string Path { get; }
}
