namespace RimeDictManager.Services;

using System.Diagnostics;
using Models;
using ViewModels;
using OpEx = InvalidOperationException;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static IEnumerable<DictInfo> DictInfos => Dicts.Select(static x => new DictInfo(x));
    public static bool Ready => Dicts.Count > 0;

    public static DictInfo AddDict(string path) {
        if (Dicts.Any(x => x.Path == path)) throw new OpEx("单字码表重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        return new(dict);
    }

    public static void RemoveDict(DictInfo dict) {
        var cnt = Dicts.RemoveAll(x => x.Path == dict.Path);
        if (cnt == 0) throw new OpEx("找不到要移除的词库");
        if (cnt > 1) throw new UnreachableException("严重错误：请停用并报告异常E");
    }

    public static Task SaveDict(DictInfo dict, string? path, bool reorder) =>
        Dicts.FindIndex(x => x.Path == dict.Path) is not (>= 0 and var i)
            ? throw new OpEx("找不到要保存的词库")
            : Dicts[i].SaveAsync(path, reorder);

    public static string SetTgtDict(DictInfo dict) {
        var i = Dicts.FindIndex(x => x.Path == dict.Path);
        if (i < 0) throw new OpEx("找不到要设为加词目标的词库");
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        return Dicts[i].Path;
    }
}
