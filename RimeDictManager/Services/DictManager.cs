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
        if (Dicts.Any(x => x.Path == path)) throw new OpEx("词库重复");
        Dict dict = new(path);
        Dicts.Add(dict);
        Log.Info($"添加词库：{path}");
        return new(dict);
    }

    public static void RemoveDict(DictInfo dict) {
        // 外部验证丢弃编辑
        var cnt = Dicts.RemoveAll(x => x.Path == dict.Path);
        if (cnt == 0) throw new OpEx("找不到要移除的词库");
        if (cnt > 1) throw new UnreachableException("严重错误：请停用并报告异常E");
        Log.Info($"移除词库：{dict.Path}");
    }

    public static async Task SaveDict(DictInfo dict, string? path, bool reorder) {
        var i = Dicts.FindIndex(x => x.Path == dict.Path);
        if (i == -1) throw new OpEx("找不到要保存的词库");
        await Dicts[i].SaveAsync(path, reorder);
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
}
