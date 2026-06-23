namespace RimeDictManager.Tests;

using Models;
using static Assert;

public sealed class CodeTrieTests {
    #region Indexer And Insert

    [Fact] public void Indexer_EmptyTrie_Null() => Null(new CodeTrie(16)["abc"]);

    [Fact]
    public void Indexer_NonexistentCode_Null() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        Null(trie["abd"]);
        Null(trie["ab"]);
    }

    [Fact]
    public void Insert_ValuesAt1Code_IndexerReturnsAll() {
        CodeTrie trie = new(16);

        trie.Insert("abc", 1);
        trie.Insert("abc", 2);

        var v = trie["abc"];
        NotNull(v);
        True(new HashSet<int> { 1, 2 }.SetEquals(v));
    }

    [Fact]
    public void Insert_SharedPrefix_Independent() {
        CodeTrie trie = new(16);

        trie.Insert("ab", 1);
        trie.Insert("abc", 2);

        var v1 = trie["ab"];
        NotNull(v1);
        Single(v1);
        Equal(1, v1[0]);
        var v2 = trie["abc"];
        NotNull(v2);
        Single(v2);
        Equal(2, v2[0]);
    }

    [Fact]
    public void Insert_EmptyCode_Works() {
        CodeTrie trie = new(16);

        trie.Insert("", 1);

        var v = trie[""];
        NotNull(v);
        Single(v);
        Equal(1, v[0]);
    }

    #endregion Indexer And Insert

    #region Remove

    [Fact] public void Remove_NonexistentCode_False() => False(new CodeTrie(16).Remove("abc", 1));

    [Fact]
    public void Remove_1OfMultiValues_TrueAndOthersRemain() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);
        trie.Insert("abc", 2);

        True(trie.Remove("abc", 1));

        var v = trie["abc"];
        NotNull(v);
        Single(v);
        Equal(2, v[0]);
    }

    [Fact]
    public void Remove_InsertedCodeNonexistentValue_FalseAndRemains() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        False(trie.Remove("abc", 2));

        var v = trie["abc"];
        NotNull(v);
        Single(v);
        Equal(1, v[0]);
    }

    #endregion Remove

    #region AnyDescendantValue

    [Fact]
    public void AnyDescendantValue_EmptyTrie_False() =>
        False(new CodeTrie(16).AnyDescendantValue("abc"));

    [Fact]
    public void AnyDescendantValue_NonexistentCode_False() {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        False(trie.AnyDescendantValue("abd"));
    }

    [Theory, InlineData("abc", false), InlineData("ab", true), InlineData("a", true)]
    public void AnyDescendantValue_VarDepths_SelfFalseAndDeeperTrue(string code, bool expected) {
        CodeTrie trie = new(16);
        trie.Insert("abc", 1);

        Equal(expected, trie.AnyDescendantValue(code));
    }

    #endregion AnyDescendantValue

    #region ForEachSubtreeValue

    [Fact]
    public void ForEachSubtreeValue_NonexistentCode_DoNothing() =>
        new CodeTrie(16).ForEachSubtreeValue("abc", static _ => Fail());

    [Fact]
    public void ForEachSubtreeValue_CommonTrie_VisitsSelfAndDescendant() {
        CodeTrie trie = new(16);
        trie.Insert("", 1);
        trie.Insert("a", 2);
        trie.Insert("ab", 3);
        trie.Insert("acd", 4);
        List<int> v = new(3);

        trie.ForEachSubtreeValue("a", v.Add);

        True(new HashSet<int> { 2, 3, 4 }.SetEquals(v));
    }

    #endregion ForEachSubtreeValue
}
