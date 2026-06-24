namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class CodeTrie(int cap) {
    private readonly List<Dictionary<char, int>?> _children = new(cap) { null };
    private readonly List<List<int>?> _values = new(cap) { null };

    public IReadOnlyList<int>? this[string code] =>
        IndexOf(code) is >= 0 and var i
            ? _values[i]
            : null;

    public void Insert(string code, int v) {
        var i = 0;
        foreach (var c in code) {
            ref var j = ref GetValueRefOrAddDefault(_children[i] ??= [], c, out var exists);
            if (!exists) {
                _children.Add(null);
                _values.Add(null);
                j = _children.Count - 1;
            }
            i = j;
        }
        if (_values[i] is {} vals)
            vals.Add(v);
        else
            _values[i] = [v];
    }

    public bool Remove(string code, int v) {
        if (IndexOf(code) is not (>= 0 and var i)
         || _values[i] is not {} vals
         || vals.IndexOf(v) is not (>= 0 and var j))
            return false;
        vals[j] = vals[^1];
        vals.RemoveAt(vals.Count - 1);
        return true;
    }

    private int IndexOf(string code) {
        var i = 0;
        foreach (var c in code)
            if (_children[i] is not {} ch || !ch.TryGetValue(c, out i))
                return -1;
        return i;
    }

    public bool AnyDescendantValue(string code) =>
        IndexOf(code) is >= 0 and var i && AnyDescendantValue(i);

    private bool AnyDescendantValue(int i) {
        if (_children[i] is not {} ch) return false;
        foreach (var (_, j) in ch)
            if (_values[j]?.Count > 0 || AnyDescendantValue(j))
                return true;
        return false;
    }

    public void ForEachSubtreeValue(string code, Action<int> f) {
        if (IndexOf(code) is >= 0 and var i) ForEachSubtreeValue(i, f);
    }

    private void ForEachSubtreeValue(int i, Action<int> f) {
        _values[i]?.ForEach(f);
        if (_children[i] is not {} ch) return;
        foreach (var (_, j) in ch) ForEachSubtreeValue(j, f);
    }
}
