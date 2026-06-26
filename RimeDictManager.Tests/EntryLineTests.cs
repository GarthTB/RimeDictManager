namespace RimeDictManager.Tests;

using Models;
using static Assert;
using static Models.EntryLine;

public sealed class EntryLineTests {
    private const string Text = "文本", Code = "code", Weight = "100", Stem = "abc";

    private readonly DictCol[] _fullCols = [
        DictCol.Text, DictCol.Code, DictCol.Weight, DictCol.Stem
    ];

    #region Format

    [Theory, InlineData(Code, Weight, Stem, $"{Text}\t{Code}\t{Weight}\t{Stem}"),
     InlineData(Code, Weight, "", $"{Text}\t{Code}\t{Weight}"),
     InlineData(Code, "", Stem, $"{Text}\t{Code}\t\t{Stem}"),
     InlineData("", Weight, "", $"{Text}\t\t{Weight}")]
    public void Format_VarEntries_JoinsWithTabAndTrimsEnd(
        string code,
        string weight,
        string stem,
        string expected) =>
        Equal(expected, new EntryLine(1, Text, code, weight, stem).Format(_fullCols));

    #endregion Format

    #region TryNew

    [Theory, InlineData(Text, Code, Weight, Stem),
     InlineData($" {Text} ", $" {Code} ", $" {Weight} ", $" {Stem} ")]
    public void TryNew_VarFields_TrueAndTrims(
        string text,
        string code,
        string weight,
        string stem) {
        True(TryNew(1, text, code, weight, stem, _fullCols, out var e));
        Equal(new(1, Text, Code, Weight, Stem), e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_OmittedField_TrueAndSetEmpty(string code) {
        True(TryNew(1, Text, code, Weight, Stem, _fullCols, out var e));
        Equal(new(1, Text, "", Weight, Stem), e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_EmptyOrSpaceText_FalseAndDefault(string text) {
        False(TryNew(1, text, Code, Weight, Stem, _fullCols, out var e));
        Equal(default, e);
    }

    [Fact]
    public void TryNew_SingleTextNoCode_FalseAndDefault() {
        False(TryNew(1, "单", "", Weight, Stem, _fullCols, out var e));
        Equal(default, e);
    }

    [Fact]
    public void TryNew_UndefinedCols_FalseAndDefault() {
        False(TryNew(1, Text, Code, Weight, Stem, [DictCol.Text, DictCol.Code], out var e));
        Equal(default, e);
    }

    #endregion TryNew
}
