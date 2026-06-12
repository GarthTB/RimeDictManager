namespace RimeDictManager.Services;

using System.Collections.Frozen;
using Models;
using OpEx = InvalidOperationException;
using static System.Runtime.InteropServices.CollectionsMarshal;

public static class Encoder {
    private static FrozenDictionary<char, string[]>? _masterDict;
    private static readonly List<SingleDict> Dicts = [];
    public static EncodeMethod Method { get; private set; } = EncodeMethod.All[0];
    public static IReadOnlyList<SingleDict> DictList => Dicts;
    public static bool Ready => _masterDict is {};

    public static SingleDict AddDict(string path) {
        if (Dicts.Any(x => x.Path == path)) throw new OpEx("单字码表重复");
        SingleDict dict = new(path);
        Dicts.Add(dict);
        _masterDict = null;
        Log.Info($"添加单字码表：{path}");
        return dict;
    }

    public static void RemoveDict(SingleDict dict) {
        if (!Dicts.Remove(dict)) throw new OpEx("移除失败");
        _masterDict = null;
        Log.Info($"移除单字码表：{dict.Path}");
    }

    public static void Prepare(EncodeMethod method) {
        if (_masterDict is {} && Method == method) return;
        Method = method;

        var cap = Dicts.Sum(static x => x.Entries.Count);
        Dictionary<char, HashSet<string>> master = new(cap);
        var stemLen = method.StemLen;
        foreach (var dict in Dicts)
        foreach (var (c, codes) in dict.Entries) {
            ref var masterCodes = ref GetValueRefOrAddDefault(master, c, out var exists);
            if (!exists) masterCodes = new(codes.Count);
            foreach (var code in codes)
                if (code.Length >= stemLen)
                    masterCodes!.Add(code[..stemLen]);
        }

        if (master.Count == 0) {
            _masterDict = null;
            Log.Info("禁用编码器：归并单字码表为空");
            return;
        }
        _masterDict = master.ToFrozenDictionary(static x => x.Key, static x => x.Value.ToArray());
        Log.Info($"启用编码器：'{method.Name}'方案，覆盖{master.Count}个字");
    }

    public static IEnumerable<string> Encode(string s) =>
        _masterDict is {} dict
            ? Method.Encode(s, dict).Distinct()
            : throw new OpEx("未启用编码器");
}
