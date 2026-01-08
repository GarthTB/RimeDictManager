namespace RimeDictManager.Models;

/// <summary> 词库前缀树：提供按编码的快速前缀搜索 </summary>
internal sealed class EntryTrie
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new(new(8105), []); // 假设覆盖通规

    /// <summary> 插入条目 </summary>
    /// <param name="entry"> 待插入的条目：必须为条目行 </param>
    public void Insert(Line entry) =>
        (entry.Code ?? "").Aggregate( // 无编码的条目插入根节点
            _root,
            static (node, c) => node.Children.TryGetValue(c, out var child)
                ? child
                : node.Children[c] = new([], []))
        .Entries.Add(entry);

    /// <summary> 删除条目 </summary>
    /// <param name="entry"> 待删除的条目：必须为已有条目行 </param>
    public void Remove(Line entry) {
        var node = _root;
        if ((entry.Code ?? "").Any(c => !node.Children.TryGetValue(c, out node))
         || !node.Entries.Remove(entry))
            throw new ArgumentException("Trie试图删除不存在的条目", nameof(entry));
    }

    /// <summary> 按编码搜索条目 </summary>
    /// <param name="code"> 编码 </param>
    /// <param name="exact"> true时精确搜索，false时前缀搜索 </param>
    /// <returns> 无重、无序的条目 </returns>
    public IReadOnlyList<Line> Search(string code, bool exact) {
        var node = _root;
        if (code.Length == 0 || code.Any(c => !node.Children.TryGetValue(c, out node)))
            return [];
        if (exact)
            return node.Entries;

        Stack<Node> nodes = new(16);
        nodes.Push(node);
        List<Line> entries = new(16);
        while (nodes.TryPop(out var top)) {
            entries.AddRange(top.Entries);
            foreach (var child in top.Children.Values)
                nodes.Push(child);
        }
        return entries;
    }

    /// <summary> Trie节点 </summary>
    /// <param name="Children"> 子节点 </param>
    /// <param name="Entries"> 当前节点的条目 </param>
    private sealed record Node(Dictionary<char, Node> Children, List<Line> Entries);
}
