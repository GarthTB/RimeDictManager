namespace RimeDictManager.Models;

using static System.Runtime.InteropServices.CollectionsMarshal;

internal sealed class CodeTrie(int poolCap) {
    private readonly List<Dictionary<char, int>?> _children = new(poolCap) { null };
    private readonly List<List<int>?> _values = new(poolCap) { null };

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
        if (TryFind(code) is not (>= 0 and var i)
         || _values[i] is not {} vals
         || vals.IndexOf(v) is not (>= 0 and var j))
            return false;

        vals[j] = vals[^1];
        vals.RemoveAt(vals.Count - 1);
        return true;
    }

    public bool ContainsKey(string? key) => TryFind(key) is >= 0 and var i && _values[i]?.Count > 0;

    public void ForEachByKey(string? key, bool exact, Action<int> f) {
        if (TryFind(key) is not (>= 0 and var i)) return;
        _values[i]?.ForEach(f);
        if (!exact) Dfs(i);

        void Dfs(int j) {
            if (_children[j] is not {} ch) return;
            foreach (var k in ch.Values) {
                _values[k]?.ForEach(f);
                Dfs(k);
            }
        }
    }

    public bool IsPrefix(string? key) {
        return TryFind(key) is >= 0 and var i && HasChildValue(i);

        bool HasChildValue(int j) {
            if (_children[j] is not {} ch) return false;
            foreach (var k in ch.Values)
                if (_values[k]?.Count > 0 || HasChildValue(k))
                    return true;
            return false;
        }
    }

    private int TryFind(string? key) {
        var i = 0;
        for (var j = 0; j < key?.Length; j++)
            if (_children[i] is not {} ch || !ch.TryGetValue(key[j], out i))
                return -1;
        return i;
    }
}
