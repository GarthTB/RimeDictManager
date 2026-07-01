namespace RimeDictManager.Services;

using System.Collections.Frozen;
using Common;
using Models;
using ZLinq;
using OpEx = InvalidOperationException;

public static class Encoder {
    private static FrozenDictionary<char, string[]>? _merged;
    private static readonly List<SingleDict> Dicts = [];
    public static IReadOnlyList<SingleDict> AllDicts => Dicts;

    public static bool Ready => _merged is {};

    public static InputMethod Method { get; private set; } = InputMethod.FlyPyTigerWubi;

    /// <summary> 加载整个目录 </summary>
    /// <param name="dir"> 目录 </param>
    /// <returns> 载入的码表数 </returns>
    public static async Task<uint> LoadDirAsync(string dir) {
        var olds = Dicts.AsValueEnumerable().Select(static x => x.Path).ToFrozenSet();
        var news = Directory.EnumerateFiles(dir, $"*{FileTypes.DictExt}");
        var cnt = 0u;
        foreach (var path in news.Where(x => !olds.Contains(x)))
            try {
                var dict = await DictIO.LoadSingleDictAsync(path);
                if (dict.Cnt == 0) continue; // 不是单字码表
                Dicts.Add(dict);
                cnt++;
                _merged = null;
                Log.Info($"添加单字码表\t{path}");
            } catch { Log.Info($"跳过单字码表\t{path}"); }
        return cnt;
    }

    public static async Task<SingleDict> AddDictAsync(string path) {
        if (Dicts.AsValueEnumerable().Any(x => x.Path == path)) throw new OpEx("单字码表重复");
        var dict = await DictIO.LoadSingleDictAsync(path);
        Dicts.Add(dict);
        _merged = null;
        Log.Info($"添加单字码表\t{path}");
        return dict;
    }

    public static void RemoveDict(SingleDict dict) {
        if (!Dicts.Remove(dict)) throw new OpEx("内部移除单字码表失败");
        _merged = null;
        Log.Info($"移除单字码表\t{dict.Path}");
    }

    public static void Prepare(InputMethod method) {
        if (_merged is {} && Method == method) return;
        var merged = SingleDict.Merge(Dicts, method.StemLen);
        if (merged.Count == 0) {
            if (_merged is {}) Log.Info("禁用编码器：归并单字码表为空");
            _merged = null;
        } else {
            _merged = merged;
            Log.Info($"启用编码器\t'{method.Name}'方案，覆盖{merged.Count}个单字");
        }
        Method = method;
    }

    public static string[] Encode(string s) =>
        Method.Encode(s, _merged ?? throw new OpEx("未启用编码器"));
}
