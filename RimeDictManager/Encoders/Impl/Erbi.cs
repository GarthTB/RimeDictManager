namespace RimeDictManager.Encoders.Impl;

using System.Collections.Frozen;

/// <summary> 二笔编码器 </summary>
/// <param name="dictPath"> 单字词库路径 </param>
internal sealed class Erbi(string dictPath): IEncoder
{
    /// <summary> 单字词库 </summary>
    private readonly FrozenDictionary<char, string[]>
        _charsDict = dictPath.ToCharsDict(2); // 仅前2码参与词组编码

    public (uint CharCnt, byte MaxLen, byte MinLen) Spec => ((uint)_charsDict.Count, 4, 4);

    public IEnumerable<string> Encode(string word) =>
        (word.First3Last1(_charsDict, out var codes) switch {
            2 =>
                from c1 in codes[0]
                from c2 in codes[1]
                select $"{c1[..2]}{c2[..2]}",
            3 =>
                from c1 in codes[0]
                from c2 in codes[1]
                from c3 in codes[2]
                select $"{c1[..2]}{c2[0]}{c3[0]}",
            4 =>
                from c1 in codes[0]
                from c2 in codes[1]
                from c3 in codes[2]
                from c4 in codes[3]
                select $"{c1[0]}{c2[0]}{c3[0]}{c4[0]}",
            _ => []
        }).Distinct();
}
