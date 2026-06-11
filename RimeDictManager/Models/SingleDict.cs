namespace RimeDictManager.Models;

using Utils;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class SingleDict {
    private readonly Dictionary<char, List<string>> _entries;

    public SingleDict(string path) {
        using StreamReader reader = new(Path = path);

        DictParser.ReadHeader(path, reader, out var name, out var cols, out var num);
        Name = name;
        if (!cols.Contains(Col.Code)) throw new FormatException("单字码表未定义编码列");

        _entries = new(4096);
        for (string? l; (l = reader.ReadLine()) is {}; num++) {
            if (string.IsNullOrWhiteSpace(l) || l[0] == '#') continue;
            var entry = LineCodec.Deserialize(num, l, cols);
            if (entry is not { Text: [var c], Code: {} code }) continue;
            ref var codes = ref GetValueRefOrAddDefault(_entries, c, out var exists);
            if (exists)
                codes!.Add(code);
            else
                codes = [code];
        }

        Cnt = (uint)_entries.Count;
    }

    public string Name { get; }
    public string Path { get; }
    public uint Cnt { get; }
    public IReadOnlyDictionary<char, List<string>> Entries => _entries;
}
