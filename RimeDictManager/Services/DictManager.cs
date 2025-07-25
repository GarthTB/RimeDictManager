using RimeDictManager.Models;
using RimeDictManager.Utils;
using System.IO;

namespace RimeDictManager.Services;

/// <summary> Rime词库管理器 </summary>
internal class DictManager
{
    /// <summary> 词库源文件（.dict.yaml）路径 </summary>
    private readonly string _sourcePath;

    /// <summary> 词库文件头 </summary>
    private readonly string[] _header;

    /// <summary> 用于按编码的高效前缀搜索 </summary>
    private readonly EntryTrie _trie;

    /// <returns> 唯一词条总数 </returns>
    public int Count => _trie.Count;

    /// <summary>
    /// 用于按词组的高效精准搜索。依赖EntryTrie实现无重
    /// </summary>
    private readonly Dictionary<string, List<Entry>> _dict;

    /// <returns> 是否已修改（即是否需要保存） </returns>
    public bool IsModified { get; private set; } = false;

    /// <summary> 构造时载入Rime词库文件（.dict.yaml） </summary>
    public DictManager(string dictPath)
    {
        _header = [.. File.ReadLines(dictPath)
            .SkipWhile(static line => line.Trim() != "---")
            .TakeWhile(static line => line.Trim() != "...")
            .Append("...")];
        if (_header.Length < 2)
            throw new InvalidDataException("词库缺失文件头！");
        (_trie, _dict) = DictFileUtils.GetEntriesParallel(dictPath)
            .ToTrieAndDict();
        _sourcePath = dictPath;
    }

    /// <summary> 插入词条 </summary>
    /// <returns> 是否原无该词条且插入成功 </returns>
    public bool Insert(Entry entry)
    {
        if (!_trie.Insert(entry))
            return false;
        if (_dict.TryGetValue(entry.Word, out var list))
            list.Add(entry);
        else _dict[entry.Word] = [entry];
        IsModified = true;
        return true;
    }

    /// <summary> 删除词条 </summary>
    /// <returns> 是否原有该词条且删除成功 </returns>
    public bool Remove(Entry entry)
    {
        if (!_trie.Remove(entry))
            return false;
        if (!_dict.TryGetValue(entry.Word, out var list)
            || !list.Remove(entry))
            throw new InvalidOperationException(
                "_dict和_trie不一致，请勿继续操作！");
        IsModified = true;
        return true;
    }

    /// <summary>
    /// 按默认规则排序词条并保存为Rime词库文件（.dict.yaml），
    /// 若文件存在，则覆盖原文件
    /// </summary>
    public async Task SortAndSaveAsync(string? path = null)
    {
        var entries = _dict.Values.SelectMany(static list => list)
            .OrderBy(static entry => entry.OrderKey)
            .Select(static entry => entry.ToString());
        await File.WriteAllLinesAsync(path ?? _sourcePath,
            _header.Concat(entries));
        IsModified = false;
    }

    /// <returns> 词组精准匹配的词条（唯一，无序） </returns>
    public List<Entry> SearchWord(string word)
        => _dict.TryGetValue(word, out var list) ? list : [];

    /// <summary>
    /// 按编码搜索词条。若exact为true，则返回编码精准匹配的词条；
    /// 否则返回前缀匹配（包含code本身）的词条（唯一，无序）
    /// </summary>
    public List<Entry> SearchCode(string code, bool exact)
        => _trie.Search(code, exact);

    /// <returns>
    /// 词组、编码至少有一个精准匹配，但元素不全同的词条（唯一，无序）
    /// </returns>
    public IEnumerable<Entry> SearchSimilarEntries(Entry entry)
        => SearchWord(entry.Word)
        .Union(_trie.Search(entry.Code, true))
        .Where(e => e != entry);
}
