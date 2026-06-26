namespace RimeDictManager.Tests;

using Models;
using Services;
using static Assert;

public sealed class DictIOTests {
    #region LoadSingleDictAsync

    [Fact]
    public async Task LoadSingleDictAsync_NoCodeCol_Throw() {
        const string text = "---\nname: test\ncolumns: [text, weight, stem]\n...\n单\tcode\t100\n";
        using TestFile file = new(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadSingleDictAsync(file.Name));
    }

    #endregion LoadSingleDictAsync

    private sealed class TestFile: IDisposable {
        public TestFile(string text) => File.WriteAllText(Name, text);
        public string Name { get; } = Path.GetTempFileName();
        public void Dispose() => File.Delete(Name);
    }

    #region LoadDictAsync

    [Fact]
    public async Task LoadDictAsync_BasicContent_Trim() {
        const string text = // 缺省列定义
            "---\n"
          + "name: test  \n" // 行尾有空格
          + "...\n"
          + "滚滚\tgun gun\t100\n"
          + " 长江 \t chang jiang \t 50 \n" // 有空格
          + "东逝水\tdong shi shui\n"; // 有省略
        const string expectedHeader = "---\nname: test\n..."; // 没有换行符
        using TestFile file = new(text);

        var dict = await DictIO.LoadDictAsync(file.Name);

        Equal(expectedHeader, dict.Header);
        Equal("test", dict.Name);
        Equal(3, dict.Cols.Count);
        Equal(DictCol.Text, dict.Cols[0]);
        Equal(DictCol.Code, dict.Cols[1]);
        Equal(DictCol.Weight, dict.Cols[2]);

        var entries = dict.Entries.ToArray();
        Equal(3, entries.Length);
        Equal("滚滚", entries[0].Text);
        Equal("gun gun", entries[0].Code);
        Equal("100", entries[0].Weight);
        Equal("长江", entries[1].Text);
        Equal("chang jiang", entries[1].Code);
        Equal("50", entries[1].Weight);
        Equal("东逝水", entries[2].Text);
        Equal("dong shi shui", entries[2].Code);
        Equal("", entries[2].Weight);
    }

    [Fact]
    public async Task LoadDictAsync_RawLines_TrimEndAndStore() {
        const string text = // 缺省列定义
            "---\n"
          + "name: test\n"
          + "...\n"
          + "\n" // 纯空行
          + "  \n" // 仅空格
          + "\t\n" // 仅 Tab
          + "# 注释\n"
          + "  # 注释\n" // 有前导空格
          + "文本 # 注释\n"
          + "  # no comment\n" // 有前导空格
          + "#no comment\n" // 无中间空格
          + "# no comment  \n" // 有尾随空格
          + "# 词条\n";
        using TestFile file = new(text);

        var dict = await DictIO.LoadDictAsync(file.Name);

        Equal(6, dict.RawLines.Count);
        Equal("", dict.RawLines[0].Content); // 纯空行
        Equal("", dict.RawLines[1].Content); // 仅空格
        Equal("", dict.RawLines[2].Content); // 仅 Tab
        Equal("# 注释", dict.RawLines[3].Content);
        Equal("#no comment", dict.RawLines[4].Content);
        Equal("# no comment", dict.RawLines[5].Content); // 尾随空格被 Trim

        var entries = dict.Entries.ToArray();
        Equal(4, entries.Length);
        Equal("# 注释", entries[0].Text); // 前导空格被 Trim
        Equal("文本 # 注释", entries[1].Text);
        Equal("# no comment", entries[2].Text); // 前导空格被 Trim
        Equal("# 词条", entries[3].Text);
    }

    [Fact]
    public async Task LoadDictAsync_NoEntries_Works() {
        const string text = "---\nname: test\n...\n";
        using TestFile file = new(text);

        var dict = await DictIO.LoadDictAsync(file.Name);

        Equal(0u, dict.Cnt);
        Empty(dict.Entries);
    }

    [Theory, InlineData("[text, code, weight, stem]"),
     InlineData("[  text  ,   code  ,   weight  ,   stem    ]"),
     InlineData("\n  - text\n  - code\n  - weight\n  - stem"),
     InlineData("\n  -   text  \n  -   code  \n  -   weight  \n  -   stem  ")]
    public async Task LoadDictAsync_VarCols_ParseAndTrim(string cols) {
        var text = $"---\nname: test\ncolumns: {cols}\n...\n";
        using TestFile file = new(text);

        var dict = await DictIO.LoadDictAsync(file.Name);

        Equal(4, dict.Cols.Count);
        Equal(DictCol.Text, dict.Cols[0]);
        Equal(DictCol.Code, dict.Cols[1]);
        Equal(DictCol.Weight, dict.Cols[2]);
        Equal(DictCol.Stem, dict.Cols[3]);
    }

    [Fact]
    public async Task LoadDictAsync_MessCols_Works() {
        const string text
            = "---\nname: test\ncolumns: [weight, text, stem, code]\n...\n100\t文本\tabc\tcode\n";
        using TestFile file = new(text);

        var dict = await DictIO.LoadDictAsync(file.Name);

        Equal(4, dict.Cols.Count);
        Equal(DictCol.Weight, dict.Cols[0]);
        Equal(DictCol.Text, dict.Cols[1]);
        Equal(DictCol.Stem, dict.Cols[2]);
        Equal(DictCol.Code, dict.Cols[3]);

        var entries = dict.Entries.ToArray();
        Single(entries);
        Equal("文本", entries[0].Text);
        Equal("code", entries[0].Code);
        Equal("100", entries[0].Weight);
        Equal("abc", entries[0].Stem);
    }

    [Theory, InlineData("[]"), InlineData("[text, abc]"), InlineData("[text, text]"),
     InlineData("[code, weight]")]
    public async Task LoadDictAsync_WeirdCols_Throw(string cols) {
        var text = $"---\nname: test\ncolumns: {cols}\n...\n文本\tcode\t100\n";
        using TestFile file = new(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(file.Name));
    }

    [Theory, InlineData("文本\tcode\t100\n"), InlineData("---\nname: test\n文本\tcode\t100\n")]
    public async Task LoadDictAsync_NoHeader_Throw(string text) {
        using TestFile file = new(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(file.Name));
    }

    [Theory, InlineData("\tcode\t100"), InlineData("文本\tcode\t100\tabc"), InlineData("单\t\t100")]
    public async Task LoadDictAsync_WeirdEntry_Throw(string entry) {
        var text = $"---\nname: test\n...\n{entry}\n";
        using TestFile file = new(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(file.Name));
    }

    #endregion LoadDictAsync

    #region SaveAsync

    [Fact]
    public async Task SaveAsync_Reorder_SortEntries_RawLinesAtEnd() {
        const string header = "---\nname: test\n...";
        RawLine[] rawLines = [new(4, ""), new(6, "# comment")];
        EntryLine[] entries = [
            new(5, "词3", "b", "300", ""), new(7, "词1", "a", "200", ""), new(8, "词2", "a", "100", "")
        ];
        Dict dict = new("test.dict.yaml", header, "test", DictCols.Default, rawLines, entries, 9);
        using TestFile file = new("");

        await DictIO.SaveAsync(dict, file.Name, true);

        var lines = await File.ReadAllLinesAsync(file.Name, TestContext.Current.CancellationToken);
        Equal(8, lines.Length);
        Equal("---", lines[0]);
        Equal("name: test", lines[1]);
        Equal("...", lines[2]);
        Equal("词1\ta\t200", lines[3]);
        Equal("词2\ta\t100", lines[4]);
        Equal("词3\tb\t300", lines[5]);
        Equal("", lines[6]);
        Equal("# comment", lines[7]);
    }

    [Fact]
    public async Task SaveAsync_NoReorder_MergeByNum() {
        const string header = "---\nname: test\n...";
        RawLine[] rawLines = [new(4, "# line 1"), new(6, "# line 3"), new(8, "# line 5")];
        EntryLine[] entries = [new(5, "词2", "b", "200", ""), new(7, "词4", "d", "400", "")];
        Dict dict = new("test.dict.yaml", header, "test", DictCols.Default, rawLines, entries, 9);
        using TestFile file = new("");

        await DictIO.SaveAsync(dict, file.Name, false);

        var lines = await File.ReadAllLinesAsync(file.Name, TestContext.Current.CancellationToken);
        Equal(8, lines.Length);
        Equal("---", lines[0]);
        Equal("name: test", lines[1]);
        Equal("...", lines[2]);
        Equal("# line 1", lines[3]);
        Equal("词2\tb\t200", lines[4]);
        Equal("# line 3", lines[5]);
        Equal("词4\td\t400", lines[6]);
        Equal("# line 5", lines[7]);
    }

    [Fact]
    public async Task SaveAsync_AfterSave_ModifiedFalse() {
        const string header = "---\nname: test\n...";
        Dict dict = new("test.dict.yaml", header, "test", DictCols.Default, [], [], 4);
        dict.Insert(new(0, "文本", "code", "100", ""));
        True(dict.Modified);
        using TestFile file = new("");

        await DictIO.SaveAsync(dict, file.Name, false);

        False(dict.Modified);
    }

    [Theory, InlineData(true), InlineData(false)]
    public async Task SaveAsync_NoEntries_OnlyHeader(bool reorder) {
        const string header = "---\nname: test\n...";
        Dict dict = new("test.dict.yaml", header, "test", DictCols.Default, [], [], 4);
        using TestFile file = new("");

        await DictIO.SaveAsync(dict, file.Name, reorder);

        var lines = await File.ReadAllLinesAsync(file.Name, TestContext.Current.CancellationToken);
        Equal(3, lines.Length);
        Equal("---", lines[0]);
        Equal("name: test", lines[1]);
        Equal("...", lines[2]);
    }

    #endregion SaveAsync
}
