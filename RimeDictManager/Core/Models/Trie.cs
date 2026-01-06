namespace RimeDictManager.Core.Models;

/// <summary> 词库前缀树：提供快速前缀搜索 </summary>
internal sealed class Trie
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new(new(8105), []); // 假设覆盖通规

    /// <summary> 条目总数 </summary>
    public uint Count { get; private set; }

    /// <summary> 插入条目 </summary>
    /// <param name="line"> 待插入的对象 </param>
    public void Insert(Line line) {
        if (line.Word is { Length: 0 } || line.Comment is {})
            throw new ArgumentException("待插入的条目异常", nameof(line));

        var node = (line.Code ?? "").Aggregate( // 无编码的条目插入根节点
            _root,
            static (node, c) => node.Children.TryGetValue(c, out var child)
                ? child
                : node.Children[c] = new([], []));
        if (node.Entries.Any(l =>
            l.Word == line.Word && l.Code == line.Code && l.Weight == line.Weight))
            throw new ArgumentException("试图插入重复条目", nameof(line));
        node.Entries.Add(line);

        Count++;
    }

    /// <summary> 删除条目 </summary>
    /// <param name="line"> 待删除的对象 </param>
    public void Remove(Line line) {
        if (line.Word is { Length: 0 } || line.Comment is {})
            throw new ArgumentException("待删除的条目异常", nameof(line));

        var node = _root;
        if ((line.Code ?? "").Any(c => !node.Children.TryGetValue(c, out node))
         || !node.Entries.Remove(line))
            throw new ArgumentException("试图删除不存在的条目", nameof(line));

        Count--;
    }

    /// <summary> 按编码搜索条目 </summary>
    /// <param name="code"> 编码 </param>
    /// <param name="exact"> true时精确搜索，false时前缀搜索 </param>
    /// <returns> 无重、无序的条目 </returns>
    public IReadOnlyList<Line> Search(string code, bool exact) {
        var node = _root;
        if (code.Any(c => !node.Children.TryGetValue(c, out node)))
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
