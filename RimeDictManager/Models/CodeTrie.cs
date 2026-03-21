namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

internal sealed class CodeTrie {
    private readonly Node _root = new(32);

    public void Insert(Entry entry) {
        var node = _root;
        foreach (var c in entry.Code ?? "") {
            ref var nodeRef = ref GetValueRefOrAddDefault(node.Children, c, out var exist);
            node = exist
                ? nodeRef!
                : nodeRef = new(0);
        }
        node.Entries.Add(entry);
    }

    public void Remove(Entry entry) {
        var node = _root;
        if ((entry.Code ?? "").Any(c => !node.Children.TryGetValue(c, out node))
         || !node.Entries.Remove(entry))
            throw new KeyNotFoundException("找不到待删除词条");
    }

    public IReadOnlyList<Entry> Search(string code, bool exact) {
        var node = _root;
        if (code.Any(c => !node.Children.TryGetValue(c, out node))) return [];

        if (exact) return node.Entries.AsReadOnly();

        List<Entry> entries = new(64);
        Stack<Node> nodes = new(64);
        nodes.Push(node);
        while (nodes.TryPop(out var top)) {
            entries.AddRange(top.Entries);
            foreach (var child in top.Children.Values) nodes.Push(child);
        }
        return entries.AsReadOnly();
    }

    private sealed class Node(int capacity) {
        public readonly Dictionary<char, Node> Children = new(capacity);
        public readonly List<Entry> Entries = [];
    }
}
