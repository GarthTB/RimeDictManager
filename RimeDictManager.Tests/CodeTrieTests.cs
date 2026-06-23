namespace RimeDictManager.Tests;

using Models;
using static Assert;

public sealed class CodeTrieTests {
    #region 索引器

    [Fact] public void Indexer_EmptyTrie_ReturnsNull() => Null(new CodeTrie(16)["abc"]);

    [Fact]
    public void Indexer_InsertedCode_ReturnsValues() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        var v = trie["abc"];

        NotNull(v);
        Single(v);
        Equal(1, v[0]);
    }

    [Fact]
    public void Indexer_NonexistentCode_ReturnsNull() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        Null(trie["abd"]);
        Null(trie["ab"]);
    }

    #endregion
}
