// ReSharper disable StringLiteralTypo

namespace RimeDictManager.Tests;

using static Assert;
using static Models.InputMethod;

public sealed class InputMethodTests {
    private static readonly Dictionary<char, string[]> Dict = new() {
        ['一'] = ["abc"],
        ['二'] = ["def"],
        ['三'] = ["ghi"],
        ['四'] = ["jkl"],
        ['五'] = ["mno"]
    };

    #region Shared

    [Fact]
    public void Encode_MultiCodesPerChar_CartesianAndDistinct() {
        Dictionary<char, string[]> dict = new() { ['多'] = ["pqr", "pqs"], ['码'] = ["tu", "vw"] };
        var v = Erbi.Encode("多码", dict);
        True(new HashSet<string> { "pqtu", "pqvw" }.SetEquals(v));
    }

    [Fact]
    public void Encode_WithInvalidChars_SkipAndUseValid() {
        var v = Erbi.Encode("a一b二c三d四e五f", Dict);
        Single(v);
        Equal("adgm", v[0]);
    }

    [Theory, InlineData("一abc"), InlineData("abc"), InlineData(""), InlineData("  ")]
    public void Encode_LessThan2ValidChars_Empty(string text) => Empty(Erbi.Encode(text, Dict));

    #endregion Shared

    #region Methods

    [Theory, InlineData("一二", "abde"), InlineData("一二三", "abdg"), InlineData("一二三四", "adgj"),
     InlineData("一二三四五", "adgm")]
    public void Encode_Erbi_VarLen_Correct(string word, string expected) {
        var v = Erbi.Encode(word, Dict);
        Single(v);
        Equal(expected, v[0]);
    }

    [Theory, InlineData("一二", "abde"), InlineData("一二三", "adgh"), InlineData("一二三四", "adgj"),
     InlineData("一二三四五", "adgm")]
    public void Encode_FlyPyTigerWubi_VarLen_Correct(string word, string expected) {
        var v = FlyPyTigerWubi.Encode(word, Dict);
        Single(v);
        Equal(expected, v[0]);
    }

    [Theory, InlineData("一二", "abdecf"), InlineData("一二三", "adgcfi"), InlineData("一二三四", "adgjcf"),
     InlineData("一二三四五", "adgmcf")]
    public void Encode_KeyTao_VarLen_Correct(string word, string expected) {
        var v = KeyTao.Encode(word, Dict);
        Single(v);
        Equal(expected, v[0]);
    }

    #endregion Methods
}
