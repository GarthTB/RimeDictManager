namespace RimeDictManager.Models;

using System.Collections.Frozen;
using ZLinq;
using static System.Runtime.InteropServices.CollectionsMarshal;

public sealed record SingleDict(
    string Path,
    string Name,
    IReadOnlyDictionary<char, List<string>> Entries) {
    public static FrozenDictionary<char, string[]> Merge(
        IReadOnlyList<SingleDict> dicts,
        byte stemLen) {
        var cap = dicts.AsValueEnumerable().Sum(static x => x.Entries.Count);
        Dictionary<char, HashSet<string>> merged = new(cap);
        foreach (var dict in dicts)
        foreach (var (c, local) in dict.Entries) {
            ref var global = ref GetValueRefOrAddDefault(merged, c, out var exists);
            if (!exists) global = new(local.Count);
            foreach (var code in local.AsValueEnumerable().Where(x => x.Length >= stemLen))
                global!.Add(code[..stemLen]);
        }
        return merged.ToFrozenDictionary(static x => x.Key, static x => x.Value.ToArray());
    }
}
