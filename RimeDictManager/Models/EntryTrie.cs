namespace RimeDictManager.Models;

/// <summary>
/// 用于存储唯一词条，并提供插入、删除、前缀搜索等功能
/// </summary>
internal class EntryTrie
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new();

    /// <returns> 唯一词条总数 </returns>
    public int Count { get; private set; } = 0;

    /// <summary> 插入词条 </summary>
    /// <returns> 是否原无该词条且插入成功 </returns>
    public bool Insert(Entry entry)
    {
        var node = _root;
        foreach (var c in entry.Code)
            node = node.Children.TryGetValue(c, out var child)
                ? child
                : node.Children[c] = new();
        if (!node.Entries.Add(entry))
            return false;
        Count++;
        return true;
    }

    /// <summary> 删除词条 </summary>
    /// <returns> 是否原有该词条且删除成功 </returns>
    public bool Remove(Entry entry)
    {
        var node = _root;
        foreach (var c in entry.Code)
            if (!node.Children.TryGetValue(c, out node))
                return false;
        if (!node.Entries.Remove(entry))
            return false;
        Count--;
        return true;
    }

    /// <summary>
    /// 按编码搜索词条。若exact为true，则返回编码精准匹配的词条；
    /// 否则返回前缀匹配（包含code本身）的词条（唯一，无序）
    /// </summary>
    public List<Entry> Search(string code, bool exact)
    {
        var node = _root;
        foreach (var c in code)
            if (!node.Children.TryGetValue(c, out node))
                return [];

        if (exact) return [.. node.Entries];

        List<Entry> entries = [];
        CollectEntries(node, entries);
        return entries;
    }

    /// <summary> 收集当前节点及其子节点的所有词条 </summary>
    private static void CollectEntries(Node node, List<Entry> entries)
    {
        entries.AddRange(node.Entries);
        foreach (var child in node.Children.Values)
            CollectEntries(child, entries);
    }

    private class Node
    {
        /// <summary> 子节点 </summary>
        public Dictionary<char, Node> Children { get; } = [];

        /// <summary> 当前节点结束的所有词条 </summary>
        public HashSet<Entry> Entries { get; } = [];
    }
}
