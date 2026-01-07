namespace RimeDictManager.Encoders.Impl;

using System.IO;

/// <summary> 五笔编码器 </summary>
/// <param name="dictPath"> 单字词库路径 </param>
internal sealed class Wubi(string dictPath): IEncoder
{
    /// <summary> 单字词库 </summary>
    private readonly Dictionary<char, string[]> _charsDict
        = File.ReadLines(dictPath).ToCharsDict(2); // 只有前2码参与词组编码

    public uint Chars => (uint)_charsDict.Count;
    public (byte Min, byte Max) LenRange => (4, 4);

    public IEnumerable<string> Encode(string word) =>
        (_charsDict.F3L1Codes(word) switch {
            { Count: 2 } codes =>
                from c1 in codes[0]
                from c2 in codes[1]
                select $"{c1[..2]}{c2[..2]}",
            { Count: 3 } codes =>
                from c1 in codes[0]
                from c2 in codes[1]
                from c3 in codes[2]
                select $"{c1[0]}{c2[0]}{c3[..2]}",
            { Count: 4 } codes =>
                from c1 in codes[0]
                from c2 in codes[1]
                from c3 in codes[2]
                from c4 in codes[3]
                select $"{c1[0]}{c2[0]}{c3[0]}{c4[0]}",
            _ => []
        }).Distinct();
}
