namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

internal sealed class CodeTrie {
    private readonly List<Node> _pool = new(262144) { new() };

    public void Insert(Entry entry) {
        var i = 0;
        foreach (var c in entry.Code ?? "") {
            ref var next = ref GetValueRefOrAddDefault(_pool[i].Next, c, out var exist);
            if (!exist) {
                next = _pool.Count;
                _pool.Add(new());
            }
            i = next;
        }
        _pool[i].Entries.Add(entry);
    }

    public void Remove(Entry entry) {
        var i = 0;
        foreach (var c in entry.Code ?? "")
            if (!_pool[i].Next.TryGetValue(c, out i))
                throw new KeyNotFoundException("找不到待删除词条");
        if (!_pool[i].Entries.Remove(entry)) throw new InvalidOperationException("删除失败");
    }

    public IReadOnlyList<Entry> Search(string code, bool exact) {
        var i = 0;
        foreach (var c in code)
            if (!_pool[i].Next.TryGetValue(c, out i))
                return [];

        if (exact) return _pool[i].Entries;

        List<Entry> entries = new(64);
        Stack<int> idx = new(64);
        for (idx.Push(i); idx.TryPop(out i); entries.AddRange(_pool[i].Entries))
            foreach (var j in _pool[i].Next.Values)
                idx.Push(j);
        return entries;
    }

    private sealed class Node {
        public readonly List<Entry> Entries = [];
        public readonly Dictionary<char, int> Next = [];
    }
}
