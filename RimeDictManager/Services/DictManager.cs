namespace RimeDictManager.Services;

using Models;
using ViewModels;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static IEnumerable<DictInfo> DictInfos => Dicts.Select(static x => new DictInfo(x));

    public static DictInfo AddDict(string path) {
        if (Dicts.Any(x => x.Path == path)) throw new InvalidOperationException("单字码表重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        return new(dict);
    }

    public static void RemoveDict(DictInfo dict) {
        if (Dicts.RemoveAll(x => x.Path == dict.Path) == 0)
            throw new InvalidOperationException("找不到要移除的词库");
    }

    public static string SetTgtDict(DictInfo dict) {
        if (Dicts.FindIndex(x => x.Path == dict.Path) is not (>= 0 and var i))
            throw new InvalidOperationException("找不到要设为加词目标的词库");
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        return Dicts[i].Path;
    }
}
