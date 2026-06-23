namespace RimeDictManager.Services;

using Models;
using Models.Serde;
using ZLinq;
using OpEx = InvalidOperationException;

public static class DictManager {
    private static readonly List<Dict> Dicts = []; // 首项为加词目标
    public static IReadOnlyList<IDictInfo> AllDicts => Dicts;

    public static bool Ready => Dicts.Count > 0;

    public static IReadOnlySet<Column>? TgtCols => Dicts.FirstOrDefault()?.Cols.ToHashSet();

    public static IReadOnlySet<Column> UnionCols =>
        Dicts.AsValueEnumerable().SelectMany(static x => x.Cols).Distinct().ToHashSet();

    #region 文件

    public static IDictInfo AddDict(string path) {
        if (Dicts.AsValueEnumerable().Any(x => x.Path == path)) throw new OpEx("词库重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        Log.Info($"添加词库\t{path}");
        return dict;
    }

    /// <summary> 移除词库 </summary>
    /// <param name="dict"> 词库 </param>
    /// <returns> (是否已删除, 新加词目标) </returns>
    public static async Task<(bool, IDictInfo?)> RemoveDictAsync(IDictInfo dict) {
        if (dict.Modified && !await MsgBox.Ask<bool>("变更未保存，是否丢弃？")) return (false, null);
        if (!Dicts.Remove((Dict)dict)) throw new OpEx("移除失败");
        Log.Info($"移除词库\t{dict.Path}");
        return (true, Dicts.FirstOrDefault());
    }

    public static async Task SaveAsync(IDictInfo dict, string? path, bool reorder) {
        await ((Dict)dict).SaveAsync(path, reorder);
        var msg0 = reorder
            ? "重新排序"
            : "不重新排序";
        var msg1 = path is {}
            ? $"另存词库，{msg0}\t来源'{dict.Path}'，目标'{path}'"
            : $"覆写词库，{msg0}\t{dict.Path}";
        Log.Info(msg1);
    }

    /// <summary> 设置加词目标 </summary>
    /// <param name="dict"> 词库 </param>
    /// <returns> 旧加词目标 </returns>
    public static IDictInfo SetTgtDict(IDictInfo dict) {
        var i = Dicts.FindIndex(x => x == dict);
        if (i == -1) throw new OpEx("找不到要设为加词目标的词库");
        (Dicts[0], Dicts[i]) = (Dicts[i], Dicts[0]);
        Log.Info($"设为加词目标\t{dict.Path}");
        return Dicts[i];
    }

    #endregion 文件

    #region 搜索

    public enum SearchMode: byte { 编码前缀, 文本精确 }

    public static IReadOnlyList<DictEntry> Search(string s, SearchMode mode) {
        List<DictEntry> result = new(64);
        if (mode == SearchMode.编码前缀)
            foreach (var dict in Dicts)
                dict.ForEachByCodePrefix(s, x => result.Add(new(dict, x)));
        else if (mode == SearchMode.文本精确)
            foreach (var dict in Dicts)
                dict.ForEachByText(s, x => result.Add(new(dict, x)));
        return result;
    }

    #endregion 搜索

    #region 操作

    public static async Task<DictEntry?> InsertEntryAsync(
        string text,
        string code,
        string weight,
        string stem) {
        var dict = Dicts[0];
        if (!LineCodec.TryNewEntry(0, text, code, weight, stem, dict.Cols, out var e))
            throw new OpEx("文本为空或字段不符合词库列定义");
        var eStr = e.Serialize(dict.Cols);

        var msg = $"确认添加词条？\n\n{eStr}";
        List<EntryLine> related = [];
        dict.ForEachByText(e.Text, related.Add);
        if (e.Code.Length > 0) dict.ForEachByCode(e.Code, related.Add);
        if (related.Count > 0)
            msg += "\n\n同词库内，已有以下相关词条：\n\n"
                 + string.Join('\n', related.Distinct().Select(x => x.Serialize(dict.Cols)));
        if (!await MsgBox.Ask<bool>(msg)) return null;

        var numbered = dict.Insert(e);
        Log.Crud("添加词条", eStr);
        return new(dict, numbered);
    }

    public static async Task<bool> RemoveEntryAsync(DictEntry e) {
        var dict = (Dict)e.Dict;
        var eStr = e.Entry.Serialize(dict.Cols);

        var msg = $"确认删除词条？\n\n{eStr}";
        if (e.Entry.Code.Length > 0 && dict.IsOnlyCodePrefix(e.Entry.Code))
            msg += $"\n\n删除后，编码'{e.Entry.Code}'将空缺，有更长编码可被截短";
        if (!await MsgBox.Ask<bool>(msg)) return false;

        if (!dict.Remove(e.Entry)) throw new OpEx("删除失败");
        Log.Crud("删除词条", eStr);
        return true;
    }

    public static async Task<bool> ShortenEntryAsync(DictEntry e, string tgt) {
        var dict = (Dict)e.Dict;
        var cols = dict.Cols;

        var ol = e.Entry; // 旧长码词条
        if (!LineCodec.TryNewEntry(ol.Num, ol.Text, tgt, ol.Weight, ol.Stem, cols, out var ns))
            throw new OpEx("截短编码后词条无效");
        List<EntryLine> osList = []; // 旧短码词条
        dict.ForEachByCode(tgt, osList.Add);

        return osList.Count switch {
            0 => await OnlyShortenAsync(dict, ol, ns),
            1 => await ShortenAndLengthen(dict, ol, ns, osList[0]),
            _ => throw new OpEx("同词库内，目标短码被多个词条占用")
        };
    }

    private static async Task<bool> OnlyShortenAsync(Dict dict, EntryLine ol, EntryLine ns) {
        var olStr = ol.Serialize(dict.Cols);
        var nsStr = ns.Serialize(dict.Cols);

        var msg = $"截短前：\t'{olStr}'\n截短后：\t'{nsStr}'";
        if (dict.IsOnlyCodePrefix(ol.Code)) msg += $"\n\n截短后，编码'{ol.Code}'将空缺，有更长编码可被截短";
        if (!await MsgBox.Ask<bool>($"确认截短编码？\n\n{msg}")) return false;

        if (!dict.Remove(ol)) throw new OpEx("删除原词条失败");
        Log.Crud("删除词条", olStr);
        dict.Insert(ns);
        Log.Crud("添加词条", nsStr);
        return true;
    }

    private static async Task<bool> ShortenAndLengthen(
        Dict dict,
        EntryLine ol,
        EntryLine ns,
        EntryLine os) {
        var fullCodes = Encoder.Encode(os.Text)
            .AsValueEnumerable()
            .Where(x => x.AsSpan().StartsWith(os.Code))
            .ToArray();
        if (fullCodes.Length == 0) throw new OpEx("原短码词条没有匹配的更长编码");

        var tgt = fullCodes.AsValueEnumerable().Any(x => x.AsSpan().StartsWith(ol.Code))
            ? ol.Code // 直接交换编码
            : Lengthen();
        if (!LineCodec.TryNewEntry(os.Num, os.Text, tgt, os.Weight, os.Stem, dict.Cols, out var nl))
            throw new OpEx("延长编码后词条无效");

        var olStr = ol.Serialize(dict.Cols);
        var nsStr = ns.Serialize(dict.Cols);
        var osStr = os.Serialize(dict.Cols);
        var nlStr = nl.Serialize(dict.Cols);

        var msg = $"截短前：\t'{olStr}'\n"
                + $"截短后：\t'{nsStr}'\n\n"
                + $"延长前：\t'{osStr}'\n"
                + $"延长后：\t'{nlStr}'";
        if (ol.Code != nl.Code && dict.IsOnlyCodePrefix(ol.Code))
            msg += $"\n\n截短后，编码'{ol.Code}'将空缺，有更长编码可被截短";
        if (!await MsgBox.Ask<bool>($"确认截短并延长编码？\n\n{msg}")) return false;

        if (!dict.Remove(ol)) throw new OpEx("删除原长码词条失败");
        Log.Crud("删除词条", olStr);
        dict.Insert(ns);
        Log.Crud("添加词条", nsStr);
        if (!dict.Remove(os)) throw new OpEx("删除原短码词条失败");
        Log.Crud("删除词条", osStr);
        dict.Insert(nl);
        Log.Crud("添加词条", nlStr);
        return true;

        string Lengthen() {
            for (var len = os.Code.Length + 1; len <= Encoder.Method.MaxLen; len++) {
                var l = len; // 消除 Rider 警告
                var codes = fullCodes.AsValueEnumerable().Select(x => x[..l]).Distinct().ToArray();
                if (codes.Length > 1) throw new OpEx("原短码词条的更长编码不唯一");
                if (!dict.ContainsCode(codes[0])) return codes[0];
            }
            throw new OpEx("原短码词条没有空闲的更长编码");
        }
    }

    public static async Task<bool> ModifyEntriesAsync(
        IReadOnlyList<(DictEntry Src, EntryLine Tgt)> mods) {
        var newCodes = mods.AsValueEnumerable().Select(static m => m.Tgt.Code).ToHashSet();
        List<string> msg = new(1 + 4 * mods.Count) { "确认应用修改？" };
        var modInfo = mods.AsValueEnumerable()
            .Select(mod => {
                var dict = (Dict)mod.Src.Dict;
                var src = mod.Src.Entry;
                var tgt = mod.Tgt;
                var srcStr = src.Serialize(dict.Cols);
                var tgtStr = tgt.Serialize(dict.Cols);

                msg.Add("\n");
                msg.Add($"修改前：\t'{srcStr}'");
                msg.Add($"修改后：\t'{tgtStr}'");
                if (src.Code.Length > 0
                 && src.Code != tgt.Code
                 && !newCodes.Contains(src.Code)
                 && dict.IsOnlyCodePrefix(src.Code))
                    msg.Add($"修改后，编码'{src.Code}'将空缺，有更长编码可被截短");

                return (dict, src, tgt, srcStr, tgtStr);
            })
            .ToArray();
        if (!await MsgBox.Ask<bool>(string.Join('\n', msg))) return false;

        foreach (var (dict, src, tgt, srcStr, tgtStr) in modInfo) {
            if (!dict.Remove(src)) throw new OpEx("删除原词条失败");
            Log.Crud("删除词条", srcStr);
            dict.Insert(tgt);
            Log.Crud("添加词条", tgtStr);
        }
        return true;
    }

    #endregion 操作
}
