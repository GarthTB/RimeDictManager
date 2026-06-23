namespace RimeDictManager.Tests;

using Models;
using static Assert;
using static Models.Column;
using static Models.Serde.LineCodec;

public sealed class LineCodecTests {
    private const string FullEntry = "文本\tcode\t100\tcd", OmittedEntry = "文本\tcode";
    private readonly Column[] _completeCols = Columns.Default;

    private readonly EntryLine _expectedCompleteEntry = new(1, "文本", "code", "100", "cd"),
        _expectedIncompleteEntry = new(1, "文本", "code", "", "");

    #region Deserialize

    [Theory, InlineData(""), InlineData("  "), InlineData("#abc"), InlineData("  #abc")]
    public void Deserialize_EmptyOrComment_FalseOutRawLine(string line) {
        False(Deserialize(line, 1, _completeCols, out var e, out var r));
        Equal(default, e);
        Equal(1u, r.Num);
        Equal(line, r.Content);
    }

    [Theory, InlineData(FullEntry), InlineData(" 文本 \t code \t 100 \t cd ")]
    public void Deserialize_CompleteEntry_TrueOutTrimmed(string line) {
        True(Deserialize(line, 1, _completeCols, out var e, out var r));
        Equal(_expectedCompleteEntry, e);
        Equal(default, r);
    }

    [Fact]
    public void Deserialize_OmittedEntry_TrueOutIncomplete() {
        True(Deserialize(OmittedEntry, 1, _completeCols, out var e, out var r));
        Equal(_expectedIncompleteEntry, e);
        Equal(default, r);
    }

    [Fact]
    public void Deserialize_MessEntry_TrueOutComplete() {
        True(Deserialize("cd\t100\t文本\tcode", 1, [Stem, Weight, Text, Code], out var e, out var r));
        Equal(_expectedCompleteEntry, e);
        Equal(default, r);
    }

    [Fact]
    public void Deserialize_NoTextEntry_Throws() =>
        Throws<FormatException>(() => Deserialize("\tcode", 1, _completeCols, out _, out _));

    [Fact]
    public void Deserialize_TooManyCols_Throws() =>
        Throws<FormatException>(() => Deserialize(FullEntry, 1, [Text, Code], out _, out _));

    #endregion Deserialize

    #region Serialize

    [Fact]
    public void Serialize_CompleteEntry_JoinsWithTab() =>
        Equal(FullEntry, _expectedCompleteEntry.Serialize(_completeCols));

    [Fact]
    public void Serialize_IncompleteEntry_Omits() =>
        Equal(OmittedEntry, _expectedIncompleteEntry.Serialize(_completeCols));

    #endregion Serialize

    #region TryNewEntry

    [Fact]
    public void TryNewEntry_ValidFields_TrueAndComplete() {
        True(TryNewEntry(1, "文本", "code", "100", "cd", _completeCols, out var e));
        Equal(_expectedCompleteEntry, e);
    }

    [Fact]
    public void TryNewEntry_SpacedFields_TrueAndTrims() {
        True(TryNewEntry(1, " 文本 ", " code ", " 100 ", " cd ", _completeCols, out var e));
        Equal(_expectedCompleteEntry, e);
    }

    [Fact]
    public void TryNewEntry_IncompleteFields_TrueAndIncomplete() {
        True(TryNewEntry(1, "文本", "code", "", "", _completeCols, out var e));
        Equal(_expectedIncompleteEntry, e);
    }

    [Theory, InlineData(""), InlineData("  ")]
    public void TryNewEntry_EmptyOrSpaceText_FalseAndDefault(string text) {
        False(TryNewEntry(1, text, "code", "100", "cd", _completeCols, out var e));
        Equal(default, e);
    }

    [Fact]
    public void TryNewEntry_TooManyCols_FalseAndDefault() {
        False(TryNewEntry(1, "文本", "code", "100", "cd", [Text, Code], out var e));
        Equal(default, e);
    }

    #endregion TryNewEntry
}
