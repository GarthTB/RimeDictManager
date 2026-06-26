namespace RimeDictManager.Tests;

using Models;
using Services;
using static Assert;

public sealed class DictIOTests {
    private static string CreateTestFile(string text) {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, text);
        return path;
    }

    #region LoadDictAsync

    [Fact]
    public async Task LoadDictAsync_BasicContent_Trim() {
        const string text = // 缺省列定义
            "---\n"
          + "name: test  \n" // 行尾有空格
          + "...\n"
          + "你好\tni hao\t100\n"
          + " 世界 \t shi jie \t 50 \n"; // 有空格
        const string expectedHeader = "---\nname: test\n..."; // 没有换行符
        var path = CreateTestFile(text);

        var dict = await DictIO.LoadDictAsync(path);

        Equal(expectedHeader, dict.Header);
        Equal("test", dict.Name);
        Equal(3, dict.Cols.Count);
        Equal(DictCol.Text, dict.Cols[0]);
        Equal(DictCol.Code, dict.Cols[1]);
        Equal(DictCol.Weight, dict.Cols[2]);

        var entries = dict.Entries.ToArray();
        Equal(2, entries.Length);
        Equal("你好", entries[0].Text);
        Equal("ni hao", entries[0].Code);
        Equal("100", entries[0].Weight);
        Equal("世界", entries[1].Text);
        Equal("shi jie", entries[1].Code);
        Equal("50", entries[1].Weight);
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
          + "你好\tni hao\t100\n"
          + "# 注释\n"
          + "你好 # 行尾注释\n"
          + "# no comment\n"
          + "# 这是词条\n";
        var path = CreateTestFile(text);

        var dict = await DictIO.LoadDictAsync(path);

        Equal(5, dict.RawLines.Count);
        Equal("", dict.RawLines[0].Content); // 纯空行
        Equal("", dict.RawLines[1].Content); // 仅空格
        Equal("", dict.RawLines[2].Content); // 仅 Tab
        Equal("# 注释", dict.RawLines[3].Content);
        Equal("# no comment", dict.RawLines[4].Content);

        var entries = dict.Entries.ToArray();
        Equal(3, entries.Length);
        Equal("你好", entries[0].Text);
        Equal("你好 # 行尾注释", entries[1].Text);
        Equal("# 这是词条", entries[2].Text);
    }

    [Theory, InlineData("[text, code, weight]"), InlineData("[  text  ,   code  ,   weight  ]"),
     InlineData("\n  - text\n  - code\n  - weight"),
     InlineData("\n  -   text  \n  -   code  \n  -   weight  ")]
    public async Task LoadDictAsync_VarCols_ParseAndTrim(string cols) {
        var text = $"---\nname: test\ncolumns: {cols}\n...\n你好\tni hao\t100\n";
        var path = CreateTestFile(text);

        var dict = await DictIO.LoadDictAsync(path);

        Equal(3, dict.Cols.Count);
        Equal(DictCol.Text, dict.Cols[0]);
        Equal(DictCol.Code, dict.Cols[1]);
        Equal(DictCol.Weight, dict.Cols[2]);
    }

    [Fact]
    public async Task LoadDictAsync_MessCols_Works() {
        const string text
            = "---\nname: test\ncolumns: [weight, text, stem, code]\n...\n100\t你好\tabc\tni hao\n";
        var path = CreateTestFile(text);

        var dict = await DictIO.LoadDictAsync(path);

        Equal(4, dict.Cols.Count);
        Equal(DictCol.Weight, dict.Cols[0]);
        Equal(DictCol.Text, dict.Cols[1]);
        Equal(DictCol.Stem, dict.Cols[2]);
        Equal(DictCol.Code, dict.Cols[3]);

        var entries = dict.Entries.ToArray();
        Single(entries);
        Equal("你好", entries[0].Text);
        Equal("ni hao", entries[0].Code);
        Equal("100", entries[0].Weight);
        Equal("abc", entries[0].Stem);
    }

    [Theory, InlineData("[]"), InlineData("[text, abc]"), InlineData("[text, text]"),
     InlineData("[code, weight]")]
    public async Task LoadDictAsync_WeirdCols_Throws(string cols) {
        var text = $"---\nname: test\ncolumns: {cols}\n...\n你好\tni hao\t100\n";
        var path = CreateTestFile(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(path));
    }

    [Fact]
    public async Task LoadDictAsync_NoHeader_Throws() {
        const string text = "你好\tni hao\t100\n";
        var path = CreateTestFile(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(path));
    }

    [Fact]
    public async Task LoadDictAsync_NoEntries_Works() {
        const string text = "---\nname: test\n...\n";
        var path = CreateTestFile(text);

        var dict = await DictIO.LoadDictAsync(path);

        Equal(0u, dict.Cnt);
        Empty(dict.Entries);
    }

    [Theory, InlineData("\tni hao\t100"), InlineData("你好\tni hao\t100\textra"),
     InlineData("单\t\t100")]
    public async Task LoadDictAsync_WeirdEntry_Throws(string entry) {
        var text = $"---\nname: test\n...\n{entry}\n";
        var path = CreateTestFile(text);

        await ThrowsAsync<FormatException>(() => DictIO.LoadDictAsync(path));
    }

    #endregion LoadDictAsync
}
