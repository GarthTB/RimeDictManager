namespace RimeDictManager.Tests;

using Models;
using static Assert;
using static Models.EntryLine;

public sealed class EntryLineTests {
    private const string Text = "文本", Code = "code", Weight = "100", Stem = "abc", Single = "单";

    private readonly DictCol[] _briefCols = [DictCol.Text, DictCol.Weight],
        _fullCols = [DictCol.Text, DictCol.Code, DictCol.Weight, DictCol.Stem],
        _messCols = [DictCol.Weight, DictCol.Text, DictCol.Stem, DictCol.Code];

    #region Format

    [Theory, InlineData(Code, Weight, Stem, $"{Text}\t{Code}\t{Weight}\t{Stem}"),
     InlineData(Code, Weight, "", $"{Text}\t{Code}\t{Weight}"),
     InlineData(Code, "", Stem, $"{Text}\t{Code}\t\t{Stem}"),
     InlineData("", Weight, "", $"{Text}\t\t{Weight}"), InlineData("", "", "", Text)]
    public void Format_VarEntries_JoinWithTabAndTrimEnd(
        string code,
        string weight,
        string stem,
        string expected) =>
        Equal(expected, new EntryLine(1, Text, code, weight, stem).Format(_fullCols));

    [Theory, InlineData(Code, Weight, Stem, $"{Weight}\t{Text}\t{Stem}\t{Code}"),
     InlineData(Code, Weight, "", $"{Weight}\t{Text}\t\t{Code}"),
     InlineData(Code, "", Stem, $"\t{Text}\t{Stem}\t{Code}"),
     InlineData("", Weight, "", $"{Weight}\t{Text}"), InlineData("", "", "", $"\t{Text}")]
    public void Format_MessCols_Works(string code, string weight, string stem, string expected) =>
        Equal(expected, new EntryLine(1, Text, code, weight, stem).Format(_messCols));

    #endregion Format

    #region TryNew

    [Theory, InlineData(Text, Code, Weight, Stem),
     InlineData($" {Text} ", $" {Code} ", $" {Weight} ", $" {Stem} ")]
    public void TryNew_VarFields_TrueAndTrim(string text, string code, string weight, string stem) {
        True(TryNew(1, text, code, weight, stem, _fullCols, out var e));
        Equal(new(1, Text, Code, Weight, Stem), e);
    }

    [Fact]
    public void TryNew_MessCols_Works() {
        True(TryNew(1, Text, Code, Weight, Stem, _messCols, out var e));
        Equal(new(1, Text, Code, Weight, Stem), e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_OmittedFields_TrueAndSetEmpty(string omitted) {
        True(TryNew(1, Text, Code, Weight, omitted, _fullCols, out var e1));
        Equal(new(1, Text, Code, Weight, ""), e1);

        True(TryNew(1, Text, omitted, Weight, omitted, _fullCols, out var e2));
        Equal(new(1, Text, "", Weight, ""), e2);

        True(TryNew(1, Text, omitted, omitted, Stem, _fullCols, out var e3));
        Equal(new(1, Text, "", "", Stem), e3);

        True(TryNew(1, Text, omitted, omitted, omitted, _fullCols, out var e4));
        Equal(new(1, Text, "", "", ""), e4);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_OmittedFieldsAsDefined_TrueAndSetEmpty(string omitted) {
        True(TryNew(1, Text, omitted, Weight, omitted, _briefCols, out var e));
        Equal(new(1, Text, "", Weight, ""), e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_EmptyOrSpaceText_FalseAndDefault(string text) {
        False(TryNew(1, text, Code, Weight, Stem, _fullCols, out var e));
        Equal(default, e);
    }

    [Fact]
    public void TryNew_UndefinedFields_FalseAndDefault() {
        False(TryNew(1, Text, Code, Weight, Stem, _briefCols, out var e));
        Equal(default, e);
    }

    [Theory, InlineData(Single), InlineData($" {Single} ")]
    public void TryNew_SingleTextWithCode_TrueAndTrim(string single) {
        True(TryNew(1, single, Code, Weight, Stem, _fullCols, out var e));
        Equal(new(1, Single, Code, Weight, Stem), e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNew_SingleTextNoCode_FalseAndDefault(string code) {
        False(TryNew(1, Single, code, Weight, Stem, _fullCols, out var e));
        Equal(default, e);
    }

    #endregion TryNew
}
