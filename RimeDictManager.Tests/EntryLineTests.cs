namespace RimeDictManager.Tests;

using Models;
using static Assert;
using static Models.EntryLine;

public sealed class EntryLineTests {
    private const string Text = "文本", Code = "code", Weight = "100", Stem = "abc";
    private const string FullEntry = "文本\tcode\t100\tcd", BriefEntry = "文本\t\t100";

    private readonly DictCol[] _fullCols = [
        DictCol.Text, DictCol.Code, DictCol.Weight, DictCol.Stem
    ];

    private readonly EntryLine _fullEntry = new(1, Text, Code, Weight, Stem),
        _briefEntry = new(1, Text, "", Weight, "");

    #region Format

    [Fact]
    public void Format_FullEntry_JoinsWithTab() => Equal(FullEntry, _fullEntry.Format(_fullCols));

    [Fact]
    public void Format_BriefEntry_Omits() => Equal(BriefEntry, _briefEntry.Format(_fullCols));

    #endregion Format

    #region TryNew

    [Fact]
    public void TryNew_SolidFields_TrueAndFull() {
        True(TryNew(1, Text, Code, Weight, Stem, _fullCols, out var e));
        Equal(_fullEntry, e);
    }

    [Fact]
    public void TryNew_SpacedFields_TrueAndTrims() {
        True(TryNew(1, $" {Text} ", $" {Code} ", $" {Weight} ", $" {Stem} ", _fullCols, out var e));
        Equal(_fullEntry, e);
    }

    [Fact]
    public void TryNew_BriefFields_TrueAndBrief() {
        True(TryNew(1, Text, "", Weight, "", _fullCols, out var e));
        Equal(_briefEntry, e);
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
