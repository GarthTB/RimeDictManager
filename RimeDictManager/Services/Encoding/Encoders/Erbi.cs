using RimeDictManager.Utils;

namespace RimeDictManager.Services.Encoding.Encoders;

/// <summary> 二笔编码器 </summary>
internal class Erbi(string dictPath) : EncoderBase(dictPath)
{
    protected override Dictionary<char, string[]>
        LoadCharCodes(string dictPath)
        => DictFileUtils.GetEntriesParallel(dictPath)
        .ExtractCharCodes(2); // 只有前2码参与词组编码

    public override IEnumerable<string> Encode(string word)
    {
        var charsCodes = EncodingUtils.First3Last1(word, CharCodes);
        return (charsCodes.Length switch
        {
            2 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 select $"{c1[..2]}{c2[..2]}",
            3 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 from c3 in charsCodes[2]
                 select $"{c1[..2]}{c2[0]}{c3[0]}",
            4 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 from c3 in charsCodes[2]
                 from c4 in charsCodes[3]
                 select $"{c1[0]}{c2[0]}{c3[0]}{c4[0]}",
            _ => []
        }).Distinct();
    }
}
