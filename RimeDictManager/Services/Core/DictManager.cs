namespace RimeDictManager.Services.Core;

using Models;
using Utils;
using ViewModels;
using ZLinq;
using OpEx = InvalidOperationException;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static bool Ready => Dicts.Count > 0;

    public static IReadOnlyList<DictInfo> DictInfos =>
        Dicts.AsValueEnumerable().Select(static x => new DictInfo(x)).ToArray();

    public static IReadOnlySet<Col>? TgtCols => Dicts.FirstOrDefault()?.Cols.ToHashSet();

    public static IReadOnlySet<Col> UnionCols =>
        Dicts.AsValueEnumerable().SelectMany(static x => x.Cols).Distinct().ToHashSet();

    #region 文件

    public static DictInfo AddDict(string path) {
        // 外部保证丢弃变更
        if (Dicts.AsValueEnumerable().Any(x => x.Path == path)) throw new OpEx("词库重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        Log.Info($"添加词库：{path}");
        return new(dict);
    }

    public static string? RemoveDict(DictInfo dict) {
        var i = Dicts.FindIndex(x => x.Path == dict.Path);
        if (i == -1) throw new OpEx("找不到要移除的词库");
        Dicts.RemoveAt(i);
        Log.Info($"移除词库：{dict.Path}");
        return i == 0 && Dicts is [var first, ..] // 更新加词目标
            ? first.Path
            : null;
    }

    public static async Task SaveDict(DictInfo dict, string? path, bool reorder) {
        var i = Dicts.FindIndex(x => x.Path == dict.Path);
        if (i == -1) throw new OpEx("找不到要保存的词库");
        await Dicts[i].SaveAsync(path, reorder);
        dict.NotifySaved();
        var msg0 = reorder
            ? "重新排序"
            : "不重新排序";
        var msg1 = path is {}
            ? $"另存词库，{msg0}：来源'{dict.Path}'，目标'{path}'"
            : $"覆写词库，{msg0}：{dict.Path}";
        Log.Info(msg1);
    }

    public static string SetTgtDict(DictInfo dict) {
        var i = Dicts.FindIndex(x => x.Path == dict.Path);
        if (i == -1) throw new OpEx("找不到要设为加词目标的词库");
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        Log.Info($"设为加词目标：{dict.Path}");
        return Dicts[i].Path;
    }

    #endregion 文件

    #region 搜索

    public enum SearchMode: byte { 编码前缀, 文本精确 }

    public static void Search(string s, SearchMode mode, Action<EntryInfo> f) {
        if (s.Length == 0) return; // 禁止匹配整个Trie或查空词
        if (mode == SearchMode.编码前缀)
            foreach (var dict in Dicts)
                dict.ForEachByCode(s, false, e => f(new(dict.Name, dict.Cols, e)));
        else if (mode == SearchMode.文本精确)
            foreach (var dict in Dicts)
                dict.ForEachByText(s, e => f(new(dict.Name, dict.Cols, e)));
    }

    #endregion 搜索
}
