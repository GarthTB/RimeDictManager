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
        ['五'] = ["mno"],
        ['多'] = ["pq", "rs"],
        ['码'] = ["tu", "vw"]
    };

    #region Shared

    [Fact]
    public void Encode_MultiCodesPerChar_CartesianAndDistinct() {
        var v = Erbi.Encode("多码", Dict);
        True(new HashSet<string> { "pqtu", "pqvw", "rstu", "rsvw" }.SetEquals(v));
    }

    [Fact]
    public void Encode_WithInvalidChars_SkipsAndUsesValid() {
        var v = Erbi.Encode("A一B二C三D四E五F", Dict);
        Single(v);
        Equal("adgm", v[0]);
    }

    [Fact] public void Encode_LessThan2ValidChars_Empty() => Empty(Erbi.Encode("一XXX", Dict));

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
