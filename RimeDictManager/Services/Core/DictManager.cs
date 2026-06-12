namespace RimeDictManager.Services.Core;

using System.Collections.Immutable;
using Data;
using Models;
using Utils;
using ViewModels;
using OpEx = InvalidOperationException;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static IEnumerable<DictInfo> DictInfos => Dicts.Select(static x => new DictInfo(x));

    public static IReadOnlySet<Col> IntersectCols =>
        Dicts.SelectMany(static x => x.Cols).ToImmutableHashSet();

    public static bool Ready => Dicts.Count > 0;

    #region 文件

    public static DictInfo AddDict(string path) {
        // 外部保证丢弃变更
        if (Dicts.Any(x => x.Path == path)) throw new OpEx("词库重复");
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
        var msg0 = path is {}
            ? $"另存词库：来源'{dict.Path}'，目标'{path}'"
            : $"覆写词库：{dict.Path}";
        var msg1 = reorder
            ? "重新排序"
            : "不重新排序";
        Log.Info($"{msg0}，{msg1}");
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

    public static void Search(string s, SearchMode mode) => throw new NotImplementedException();

    #endregion 搜索
}
