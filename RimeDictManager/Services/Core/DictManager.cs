namespace RimeDictManager.Services.Core;

using System.Diagnostics;
using Data;
using Models;
using Utils;
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
    /// <param name="f"> 对新加词目标的响应动作 </param>
    /// <returns> 是否执行了删除 </returns>
    public static async Task<bool> RemoveDict(IDictInfo dict, Action<IDictInfo?> f) {
        if (dict.Mod && !await MsgBox.Ask<bool>("变更未保存，是否丢弃？")) return false;
        var i = Dicts.FindIndex(x => x == dict);
        if (i == -1) throw new OpEx("找不到要移除的词库");
        Dicts.RemoveAt(i);
        Log.Info($"移除词库：{dict.Path}");
        f(Dicts.FirstOrDefault());
        return true;
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
    /// <returns> 旧加词目标 </returns>
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

    public static void Search(string s, SearchMode mode, Action<DictEntry> f) {
        if (s.Length == 0) return; // 禁止匹配整个Trie或查空词
        if (mode == SearchMode.编码前缀)
            foreach (var dict in Dicts)
                dict.ForEachByCode(s, false, e => f(new(dict, e)));
        else if (mode == SearchMode.文本精确)
            foreach (var dict in Dicts)
                dict.ForEachByText(s, e => f(new(dict, e)));
    }

    #endregion 搜索

    #region 操作

    public static async Task<DictEntry?> InsertEntry(
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
        return new(dict, e);
    }

    public static async Task<bool> RemoveEntry(DictEntry e) {
        var dict = Dicts.AsValueEnumerable().First(x => x == e.Dict);
        var eStr = e.Entry.Serialize(dict.Cols);

        var msg = $"确认删除词条？\n{eStr}";
        if (e.Entry.Code is {} code && dict.IsCodePrefix(code))
            msg += $"\n删除后，编码'{code}'将空缺，有更长编码可被截短";
        if (!await MsgBox.Ask<bool>(msg)) return false;

        if (!dict.Remove(e.Entry)) throw new OpEx("找不到要删除的词条");
        Log.Crud("删除词条", eStr);
        return true;
    }

    public static async Task<bool> ShortenEntry(DictEntry e, string tgt) {
        var dict = Dicts.AsValueEnumerable().First(x => x == e.Dict);
        var cols = dict.Cols;

        var ol = e.Entry; // 旧长码词条
        if (!LineCodec.TryNewEntry(ol.Num, ol.Text, tgt, ol.Weight, ol.Stem, cols, out var ns))
            throw new UnreachableException("不可能错误，请停用并报告：截短编码后词条无效");

        List<EntryLine> osList = []; // 旧短码词条
        dict.ForEachByCode(tgt, true, osList.Add);
        if (osList.Count > 1) throw new OpEx("同词库内，目标短码被多个词条占用");

        var olStr = ol.Serialize(cols);
        var nsStr = ns.Serialize(cols);
        var msg = $"截短前：\n{olStr}\n截短后：\n{nsStr}";
        if (osList.Count == 0) {
            if (dict.IsCodePrefix(ol.Code)) msg += $"\n截短后，编码'{ol.Code}'将空缺，有更长编码可被截短";
            if (!await MsgBox.Ask<bool>($"确认截短编码？\n{msg}")) return false;

            if (!dict.Remove(ol)) throw new OpEx("找不到要截短的词条");
            Log.Crud("删除词条", olStr);
            dict.Insert(ns);
            Log.Crud("添加词条", nsStr);
            return true;
        }

        var os = osList[0]; // 旧短码词条

        var fullCodes = Encoder.Encode(os.Text)
            .AsValueEnumerable()
            .Where(x => x.AsSpan().StartsWith(tgt))
            .ToArray();
        if (fullCodes.Length == 0) throw new OpEx("占位词条没有匹配的更长编码");
        var tgt2 = fullCodes.AsValueEnumerable().Any(x => x.AsSpan().StartsWith(ol.Code))
            ? ol.Code // 直接交换编码
            : Determine();
        if (!LineCodec.TryNewEntry(os.Num, os.Text, tgt2, os.Weight, os.Stem, cols, out var nl))
            throw new UnreachableException("不可能错误，请停用并报告：延长编码后词条无效");

        var osStr = os.Serialize(cols);
        var nlStr = nl.Serialize(cols);
        msg += $"\n延长前：\n{osStr}\n延长后：\n{nlStr}";
        if (ol.Code != nl.Code && dict.IsCodePrefix(ol.Code))
            msg += $"\n截短后，编码'{ol.Code}'将空缺，有更长编码可被截短";
        if (!await MsgBox.Ask<bool>($"确认截短并延长编码？\n{msg}")) return false;

        if (!dict.Remove(ol)) throw new OpEx("找不到要截短的词条");
        Log.Crud("删除词条", olStr);
        dict.Insert(ns);
        Log.Crud("添加词条", nsStr);
        if (!dict.Remove(os)) throw new OpEx("找不到要延长的词条");
        Log.Crud("删除词条", osStr);
        dict.Insert(nl);
        Log.Crud("添加词条", nlStr);
        return true;

        string Determine() {
            for (var len = tgt.Length + 1; len <= Encoder.Method.MaxLen; len++) {
                var l = len; // Rider叫我这样写
                var codes = fullCodes.AsValueEnumerable().Select(x => x[..l]).Distinct().ToArray();
                if (codes.Length > 1) throw new OpEx("占位词条的更长编码不唯一");
                if (!dict.ContainsCode(codes[0])) return codes[0];
            }
            throw new OpEx("占位词条没有空闲的更长编码");
        }
    }

    public static async Task<bool> ModifyEntries(IReadOnlyList<(DictEntry O, EntryLine N)> mods) {
        List<string> msg = new(1 + 3 * mods.Count) { "确认应用修改？" };
        var newCodes = mods.AsValueEnumerable().Select(static m => m.N.Code).ToHashSet();
        var modInfo = mods.AsValueEnumerable()
            .Select(m => {
                var d = Dicts.AsValueEnumerable().First(d => d == m.O.Dict);
                EntryLine o = m.O.Entry, n = m.N;
                string oStr = o.Serialize(d.Cols), nStr = n.Serialize(d.Cols);
                msg.Add($"修改前：\n{oStr}");
                msg.Add($"修改后：\n{nStr}");
                if (o.Code is {} oCode
                 && oCode != n.Code
                 && !newCodes.Contains(oCode)
                 && d.IsCodePrefix(oCode))
                    msg.Add($"修改后，编码'{oCode}'将空缺，有更长编码可被截短");
                return (d, o, n, oStr, nStr);
            })
            .ToArray();
        if (!await MsgBox.Ask<bool>(string.Join('\n', msg))) return false;

        foreach (var (d, o, n, oStr, nStr) in modInfo) {
            if (!d.Remove(o)) throw new OpEx("找不到要修改的词条");
            Log.Crud("删除词条", oStr);
            d.Insert(n);
            Log.Crud("添加词条", nStr);
        }
        return true;
    }

    #endregion 操作
}
