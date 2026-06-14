namespace RimeDictManager.Services.Core;

using Data;
using Models;
using Utils;
using ViewModels;
using ZLinq;
using OpEx = InvalidOperationException;

public static class DictManager {
    /// <summary> 词库集合：首项为加词目标 </summary>
    private static readonly List<Dict> Dicts = [];

    public static bool Ready => Dicts.Count > 0;

    public static IEnumerable<IDictInfo> AllDicts => Dicts;
    public static IReadOnlySet<Col>? TgtCols => Dicts.FirstOrDefault()?.Cols.ToHashSet();

    public static IReadOnlySet<Col> UnionCols =>
        Dicts.AsValueEnumerable().SelectMany(static x => x.Cols).Distinct().ToHashSet();

    #region 文件

    public static IDictInfo AddDict(string path) {
        if (Dicts.AsValueEnumerable().Any(x => x.Path == path)) throw new OpEx("词库重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        Log.Info($"添加词库：{path}");
        return dict;
    }

    /// <summary> 移除词库 </summary>
    /// <param name="dict"> 词库 </param>
    /// <returns> 加词目标：没变时null </returns>
    /// <remarks> 有变更时不拦截 </remarks>
    public static IDictInfo? RemoveDict(IDictInfo dict) {
        var i = Dicts.FindIndex(x => x == dict);
        if (i == -1) throw new OpEx("找不到要移除的词库");
        Dicts.RemoveAt(i);
        Log.Info($"移除词库：{dict.Path}");
        return i == 0
            ? Dicts.FirstOrDefault()
            : null;
    }

    public static async Task SaveDict(IDictInfo dict, string? path, bool reorder) {
        var i = Dicts.FindIndex(x => x == dict);
        if (i == -1) throw new OpEx("找不到要保存的词库");
        await Dicts[i].SaveAsync(path, reorder);
        var msg0 = reorder
            ? "重新排序"
            : "不重新排序";
        var msg1 = path is {}
            ? $"另存词库，{msg0}：来源'{dict.Path}'，目标'{path}'"
            : $"覆写词库，{msg0}：{dict.Path}";
        Log.Info(msg1);
    }

    /// <summary> 设置加词目标 </summary>
    /// <param name="dict"> 词库 </param>
    /// <returns> 旧的加词目标词库 </returns>
    public static IDictInfo SetTgtDict(IDictInfo dict) {
        var i = Dicts.FindIndex(x => x == dict);
        if (i == -1) throw new OpEx("找不到要设为加词目标的词库");
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        Log.Info($"设为加词目标：{dict.Path}");
        return Dicts[i];
    }

    #endregion 文件

    #region 搜索

    public enum SearchMode: byte { 编码前缀, 文本精确 }

    public static void Search(string s, SearchMode mode, Action<EntryVM> f) {
        if (s.Length == 0) return; // 禁止匹配整个Trie或查空词
        if (mode == SearchMode.编码前缀)
            foreach (var dict in Dicts)
                dict.ForEachByCode(s, false, e => f(new(e, dict)));
        else if (mode == SearchMode.文本精确)
            foreach (var dict in Dicts)
                dict.ForEachByText(s, e => f(new(e, dict)));
    }

    #endregion 搜索

    #region 操作

    public static async Task<EntryVM?> InsertEntry(
        string text,
        string? code,
        string? weight,
        string? stem) {
        var dict = Dicts[0];
        if (!LineCodec.TryNewEntry(0, text, code, weight, stem, dict.Cols, out var e))
            throw new OpEx("文本为空或字段不符合词库列定义");

        List<EntryLine> related = [];
        dict.ForEachByText(e.Text, related.Add);
        if (e.Code is {} s) dict.ForEachByCode(s, true, related.Add);
        if (related.Count > 0) {
            var msg = string.Join('\n', related.Select(x => x.Serialize(dict.Cols)));
            if (!await MsgBox.Ask<bool>($"已有以下词条，是否仍要添加？\n{msg}")) return null;
        }

        dict.Insert(e);
        Log.Crud("添加词条", e.Serialize(dict.Cols));
        return new(e, dict);
    }

    public static void RemoveEntry(EntryVM e) => throw new NotImplementedException();

    public static void ShortenEntry(EntryVM e, string tgtCode) =>
        throw new NotImplementedException();

    public static void ModifyEntry(EntryVM bef, EntryVM aft) => throw new NotImplementedException();

    #endregion 操作
}
