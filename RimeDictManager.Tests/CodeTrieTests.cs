namespace RimeDictManager.Tests;

using Models;
using static Assert;

public sealed class CodeTrieTests {
    [Fact]
    public void Indexer() {
        CodeTrie trie = new(16);

        var v0 = trie["abc"];

        Null(v0);

        trie.Insert("abc", 1);

        var v1 = trie["abc"];

        NotNull(v1);
        Single(v1);
        Equal(1, v1[0]);
        Null(trie["abd"]);
        Null(trie["ab"]);
    }
}
