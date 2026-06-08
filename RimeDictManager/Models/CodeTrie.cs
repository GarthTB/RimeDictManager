namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed class CodeTrie(int cap) {
    private readonly List<Dictionary<char, int>?> _children = new(cap) { null };
    private readonly List<List<int>?> _values = new(cap) { null };

    public void Insert(string? code, int v) {
        var i = 0;
        for (var j = 0; j < code?.Length; j++) {
            ref var k = ref GetValueRefOrAddDefault(_children[i] ??= [], code[j], out var exists);
            if (!exists) {
                _children.Add(null);
                _values.Add(null);
                k = _children.Count - 1;
            }
            i = k;
        }
        (_values[i] ??= []).Add(v);
    }

    public bool Remove(string? code, int v) {
        if (!TryFind(code, out var i)
         || _values[i] is not {} vals
         || vals.IndexOf(v) is not (>= 0 and var j))
            return false;
        vals[j] = vals[^1];
        vals.RemoveAt(vals.Count - 1);
        return true;
    }

    public bool HasValue(string? code) => TryFind(code, out var i) && _values[i]?.Count > 0;
    public bool HasChildValue(string? code) => TryFind(code, out var i) && HasChildValue(i);

    public void ForEachBy(string? code, bool exact, Action<int> f) {
        if (!TryFind(code, out var i)) return;
        _values[i]?.ForEach(f);
        if (!exact) Dfs(i);

        void Dfs(int j) {
            if (_children[j] is not {} ch) return;
            foreach (var (_, k) in ch) {
                _values[k]?.ForEach(f);
                Dfs(k);
            }
        }
    }

    private bool TryFind(string? code, out int i) {
        i = 0;
        for (var j = 0; j < code?.Length; j++)
            if (_children[i] is not {} ch || !ch.TryGetValue(code[j], out i))
                return false;
        return true;
    }

    private bool HasChildValue(int i) {
        if (_children[i] is not {} ch) return false;
        foreach (var (_, j) in ch)
            if (_values[j]?.Count > 0 || HasChildValue(j))
                return true;
        return false;
    }
}
