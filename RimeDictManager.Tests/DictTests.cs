namespace RimeDictManager.Tests;

using Models;
using static Assert;

public sealed class DictTests {
    private static Dict CreateDict(params EntryLine[] entries) =>
        new(
            "test.dict.yaml",
            "---\nname: test\n...",
            "test",
            DictCols.Default,
            [],
            entries,
            (uint)(entries.Length + 4));

    #region Ctor

    [Fact]
    public void Ctor_NoEntries_EmptyAndNotModified() {
        var dict = CreateDict();

        Equal("---\nname: test\n...", dict.Header);
        Empty(dict.RawLines);
        Empty(dict.Entries);
        Equal("test.dict.yaml", dict.Path);
        Equal("test", dict.Name);
        Equal(DictCols.Default, dict.Cols);
        Equal(0u, dict.Cnt);
        False(dict.Modified);
    }

    [Fact]
    public void Ctor_WithEntries_AccessibleAndNotModified() {
        EntryLine e1 = new(4, "滚滚", "gun gun", "100", ""),
            e2 = new(5, "长江", "chang jiang", "50", "");

        var dict = CreateDict(e1, e2);

        Equal("---\nname: test\n...", dict.Header);
        Empty(dict.RawLines);
        Equal("test.dict.yaml", dict.Path);
        Equal("test", dict.Name);
        Equal(DictCols.Default, dict.Cols);
        Equal(2u, dict.Cnt);
        False(dict.Modified);

        var entries = dict.Entries.ToArray();
        Equal(2, entries.Length);
        Equal("滚滚", entries[0].Text);
        Equal("gun gun", entries[0].Code);
        Equal("100", entries[0].Weight);
        Equal("长江", entries[1].Text);
        Equal("chang jiang", entries[1].Code);
        Equal("50", entries[1].Weight);

        Equal(1, dict.EntriesAtCode("gun gun"));
        dict.ForEachByText("滚滚", e => Equal(e1, e));
        Equal(1, dict.EntriesAtCode("chang jiang"));
        dict.ForEachByText("长江", e => Equal(e2, e));
    }

    #endregion Ctor

    #region Insert

    [Theory, InlineData(0, 6), InlineData(7, 7)]
    public void Insert_VarEntries_GiveNextNumIf0(uint num, uint expected) {
        var dict = CreateDict(
            new(4, "滚滚", "gun gun", "100", ""),
            new(5, "长江", "chang jiang", "50", ""));
        EntryLine e0 = new(num, "东逝水", "dong shi shui", "", "");

        var e1 = dict.Insert(e0);

        Equal(expected, e1.Num);
        Equal(3u, dict.Cnt);
        True(dict.Modified);

        var entries = dict.Entries.ToHashSet();
        Equal(3, entries.Count);
        Contains(e0 with { Num = expected }, entries);
    }

    [Theory, InlineData(0), InlineData(1)]
    public void Insert_DuplicateEntries_Works(uint num) {
        var dict = CreateDict();
        EntryLine e = new(num, "文本", "code", "100", "");

        dict.Insert(e);
        dict.Insert(e);

        Equal(2u, dict.Cnt);
        True(dict.Modified);
    }

    #endregion Insert

    #region Remove

    [Fact]
    public void Remove_ExistentEntry_TrueAndClean() {
        EntryLine e = new(4, "文本", "code", "100", "");
        var dict = CreateDict(e);

        True(dict.Remove(e));

        Empty(dict.Entries);
        Equal(0u, dict.Cnt);
        True(dict.Modified);
        Equal(0, dict.EntriesAtCode("code"));
        dict.ForEachByText("文本", static _ => Fail());
    }

    [Fact]
    public void Remove_NonexistentEntry_FalseAndRemain() {
        EntryLine e = new(4, "滚滚", "gun gun", "100", "");
        var dict = CreateDict(e);

        False(dict.Remove(new(5, "长江", "chang jiang", "50", "")));

        Equal(1u, dict.Cnt);
        False(dict.Modified);
        var entries = dict.Entries.ToArray();
        Single(entries);
        Equal(e, entries[0]);
        Equal(1, dict.EntriesAtCode("gun gun"));
        dict.ForEachByText("滚滚", x => Equal(e, x));
    }

    [Fact]
    public void Remove_DuplicateEntries_1By1() {
        EntryLine e = new(4, "文本", "code", "100", "");
        var dict = CreateDict(e, e);

        True(dict.Remove(e));

        Equal(1u, dict.Cnt);
        True(dict.Modified);
        var entries = dict.Entries.ToArray();
        Single(entries);
        Equal(e, entries[0]);
        Equal(1, dict.EntriesAtCode("code"));
        dict.ForEachByText("文本", x => Equal(e, x));

        True(dict.Remove(e));

        Empty(dict.Entries);
        Equal(0u, dict.Cnt);
        Equal(0, dict.EntriesAtCode("code"));
        dict.ForEachByText("文本", static _ => Fail());

        False(dict.Remove(e));
    }

    #endregion Remove
}
