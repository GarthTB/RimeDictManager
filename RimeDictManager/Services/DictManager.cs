namespace RimeDictManager.Services;

using Models;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static IReadOnlyList<Dict> DictList => Dicts;
    public static void AddDict(Dict dict) => Dicts.Add(dict);
    public static bool RemoveDict(Dict dict) => Dicts.Remove(dict);

    public static Dict SetTgtDict(Dict dict) {
        var i = Dicts.IndexOf(dict);
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        return Dicts[i];
    }
}
