namespace RimeDictManager.Services;

using System.Collections.Frozen;
using Models;
using ZLinq;
using OpEx = InvalidOperationException;
using static System.Runtime.InteropServices.CollectionsMarshal;

public static class Encoder {
    private static FrozenDictionary<char, string[]>? _merged;
    private static readonly List<SingleDict> Dicts = [];
    public static IReadOnlyList<SingleDict> AllDicts => Dicts;

    public static bool Ready => _merged is {};

    public static InputMethod Method { get; private set; } = InputMethod.All[0];

    public static SingleDict AddDict(string path) {
        if (Dicts.AsValueEnumerable().Any(x => x.Path == path)) throw new OpEx("单字码表重复");
        SingleDict dict = new(path);
        Dicts.Add(dict);
        _merged = null;
        Log.Info($"添加单字码表：{path}");
        return dict;
    }

    public static void RemoveDict(SingleDict dict) {
        if (!Dicts.Remove(dict)) throw new OpEx("移除失败");
        _merged = null;
        Log.Info($"移除单字码表：{dict.Path}");
    }

    public static void Prepare(InputMethod method) {
        if (_merged is {} && Method == method) return;

        var cap = Dicts.AsValueEnumerable().Sum(static x => x.Entries.Count);
        Dictionary<char, HashSet<string>> merged = new(cap);
        foreach (var dict in Dicts)
        foreach (var (c, codes) in dict.Entries) {
            ref var mCodes = ref GetValueRefOrAddDefault(merged, c, out var exists);
            if (!exists) mCodes = new(codes.Count);
            foreach (var code in codes)
                if (code.Length >= method.StemLen)
                    mCodes!.Add(code[..method.StemLen]);
        }

        if (merged.Count == 0) {
            if (_merged is {}) Log.Info("禁用编码器：归并单字码表为空");
            _merged = null;
        } else {
            _merged = merged.ToFrozenDictionary(static x => x.Key, static x => x.Value.ToArray());
            Log.Info($"启用编码器：'{method.Name}'方案，覆盖{merged.Count}个单字");
        }
        Method = method;
    }

    public static string[] Encode(string s) =>
        Method.Encode(s, _merged ?? throw new OpEx("未启用编码器"));
}
