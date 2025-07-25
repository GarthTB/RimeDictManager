using RimeDictManager.Models;
using System.IO;

namespace RimeDictManager.Utils;

/// <summary> Rime词库文件（.dict.yaml）读写工具类 </summary>
internal static class DictFileUtils
{
    /// <summary> 读取Rime词库文件（.dict.yaml）中的词条 </summary>
    /// <returns> 所有有效词条（并行）及其行索引 </returns>
    public static ParallelQuery<(int Index, Entry Entry)>
        GetEntriesParallel(string path)
        => File.ReadLines(path)
        .AsParallel()
        .Select(static (line, index) =>
        {
            var idx = line.IndexOf('#');
            var data = idx < 0 ? line : line[..idx];
            var parts = data.Split('\t', 5, StringSplitOptions.TrimEntries);
            return (index, entry: new Entry(
                parts.ElementAtOrDefault(0) ?? "",
                parts.ElementAtOrDefault(1) ?? "",
                parts.ElementAtOrDefault(2) ?? "",
                parts.ElementAtOrDefault(3) ?? ""));
        })
        .Where(static tuple => tuple.entry.IsValid);

    /// <summary>
    /// 将词条转换为Trie树和字典，
    /// 并警告或解决词条顺序错乱的潜在问题
    /// </summary>
    public static (EntryTrie, Dictionary<string, List<Entry>>) ToTrieAndDict(
        this ParallelQuery<(int Index, Entry Entry)> entries)
    {
        var reassignWeight = MsgBox.Confirm("操作请求",
            "如果词库中存在编码相同且权重值相同的词条，\n"
          + "修改后不会保留原有顺序，进而可能影响选重。\n"
          + "是否允许按照现有顺序为这部分词条赋予新的权重？");

        var preprocessedEntries = entries
            .GroupBy(static tuple => tuple.Entry.Code)
            .SelectMany(group =>
            {
                var uniqueEntries = group.DistinctBy(
                    static tuple => (tuple.Entry.Word, tuple.Entry.Stem))
                .ToArray();
                var uniqueWeightValues = uniqueEntries.DistinctBy(
                    static tuple => tuple.Entry.WeightValue ?? 0);
                return uniqueEntries.Length == uniqueWeightValues.Count()
                    || !reassignWeight
                    ? uniqueEntries.Select(static tuple => tuple.Entry)
                    : uniqueEntries
                        .OrderByDescending(static tuple => tuple.Index)
                        .Select(static (tuple, index) => new Entry(
                            tuple.Entry.Word,
                            tuple.Entry.Code,
                            (index + 1).ToString(), // Index越小权重越高
                            tuple.Entry.Stem));
            });

        EntryTrie trie = new();
        Dictionary<string, List<Entry>> dict = [];
        foreach (var entry in preprocessedEntries.AsSequential())
            if (trie.Insert(entry))
                if (dict.TryGetValue(entry.Word, out var list))
                    list.Add(entry);
                else dict[entry.Word] = [entry];

        return (trie, dict);
    }
}
