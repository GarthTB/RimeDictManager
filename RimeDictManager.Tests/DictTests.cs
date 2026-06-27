namespace RimeDictManager.Tests;

using Models;
using ZLinq;
using static Assert;

public sealed class DictTests {
    private static Dict CreateDict(params EntryLine[] entries) =>
        new(
            "test.dict.yaml",
            "---\nname: test\n...", // 一共 3 行
            "test",
            DictCols.Default,
            [], // 没有非词条行
            entries,
            (uint)(entries.Length + 4)); // 词条从第 4 行开始

    #region Insert And ForEach

    [Fact]
    public void Insert_VarNumEntries_AssignIncrementalNumIf0() {
        EntryLine e1 = new(0, "词1", "a", "", ""),
            e2 = new(0, "词2", "b", "100", ""),
            e3 = new(1, "词3", "c", "", ""),
            e4 = new(8, "词4", "d", "50", ""),
            expected1 = e1 with { Num = 4 },
            expected2 = e2 with { Num = 5 };
        var dict = CreateDict();
        Equal(0u, dict.Cnt);
        False(dict.Modified);

        Equal(expected1, dict.Insert(e1));
        Equal(expected2, dict.Insert(e2));
        Equal(e3, dict.Insert(e3));
        Equal(e4, dict.Insert(e4));

        True(new HashSet<EntryLine> { expected1, expected2, e3, e4 }.SetEquals(dict.Entries));
        Equal(4u, dict.Cnt);
        True(dict.Modified);
    }

    [Fact]
    public void Insert_SimilarEntries_Independent() {
        EntryLine e1 = new(4, "文本", "a", "", ""),
            e2 = new(5, "文本", "b", "", ""), // 同文本
            e3 = new(6, "词词", "a", "", ""); // 同编码
        var dict = CreateDict();
        Equal(0u, dict.Cnt);
        False(dict.Modified);

        Equal(e1, dict.Insert(e1));
        Equal(e2, dict.Insert(e2));
        Equal(e3, dict.Insert(e3));
        Equal(e1, dict.Insert(e1)); // 全同

        Equal(4u, dict.Cnt);
        True(dict.Modified);
        AssertCountConsistent(dict, e1, 2);
        AssertCountConsistent(dict, e2, 1);
        AssertCountConsistent(dict, e3, 1);
    }

    [Fact]
    public void InsertAndForEach_CommonEntries_Correct() {
        EntryLine e1 = new(4, "文本", "a", "", ""),
            e2 = new(5, "词2", "b", "100", ""),
            e3 = new(6, "词3", "ab", "", ""),
            e4 = new(7, "词4", "ab", "", ""),
            e5 = new(8, "文本", "abc", "50", "");
        var dict = CreateDict(e1, e2, e3, e4, e5); // 内部走 Insert，顺带检验索引

        List<EntryLine> v = new(4);
        dict.ForEachByCode("ab", v.Add);
        True(new HashSet<EntryLine> { e3, e4 }.SetEquals(v));

        v.Clear();
        dict.ForEachByCodePrefix("ab", v.Add);
        True(new HashSet<EntryLine> { e3, e4, e5 }.SetEquals(v));

        v.Clear();
        dict.ForEachByText("文本", v.Add);
        True(new HashSet<EntryLine> { e1, e5 }.SetEquals(v));
    }

    #endregion Insert And ForEach

    #region Remove

    [Fact]
    public void Remove_NonexistentEntry_False() {
        EntryLine e = new(0, "文本", "code", "", "");
        var dict = CreateDict();

        False(dict.Remove(e));

        Equal(0u, dict.Cnt);
        False(dict.Modified);
        AssertCountConsistent(dict, e, 0);
    }

    [Fact]
    public void Remove_SimilarEntries_1By1() {
        EntryLine e1 = new(4, "文本", "a", "", ""),
            e2 = new(5, "文本", "b", "", ""), // 同文本
            e3 = new(6, "词词", "a", "", ""); // 同编码
        var dict = CreateDict(e1, e2, e3, e1);

        True(dict.Remove(e1));
        True(dict.Modified);

        Equal(3u, dict.Cnt);
        AssertCountConsistent(dict, e1, 1);
        AssertCountConsistent(dict, e2, 1);
        AssertCountConsistent(dict, e3, 1);

        True(dict.Remove(e1));

        Equal(2u, dict.Cnt);
        AssertCountConsistent(dict, e1, 0);
        AssertCountConsistent(dict, e2, 1);
        AssertCountConsistent(dict, e3, 1);

        True(dict.Remove(e2));

        Equal(1u, dict.Cnt);
        AssertCountConsistent(dict, e1, 0);
        AssertCountConsistent(dict, e2, 0);
        AssertCountConsistent(dict, e3, 1);

        True(dict.Remove(e3));

        Equal(0u, dict.Cnt);
        AssertCountConsistent(dict, e1, 0);
        AssertCountConsistent(dict, e2, 0);
        AssertCountConsistent(dict, e3, 0);
    }

    #endregion Remove

    #region State

    [Fact]
    public void NotifySaved_ModifiedTrue_SetFalse() {
        var dict = CreateDict();
        dict.Insert(new(0, "词1", "a", "", ""));
        True(dict.Modified);

        dict.NotifySaved();

        False(dict.Modified);
    }

    private static void AssertCountConsistent(Dict dict, EntryLine e, int cnt) {
        Equal(cnt, dict.Entries.AsValueEnumerable().Count(x => x == e));
        var cnt1 = 0;
        dict.ForEachByCode(
            e.Code,
            x => {
                if (x == e) cnt1++;
            });
        Equal(cnt, cnt1);
        var cnt2 = 0;
        dict.ForEachByCodePrefix(
            e.Code,
            x => {
                if (x == e) cnt2++;
            });
        Equal(cnt, cnt2);
        var cnt3 = 0;
        dict.ForEachByText(
            e.Text,
            x => {
                if (x == e) cnt3++;
            });
        Equal(cnt, cnt3);
    }

    #endregion State
}
