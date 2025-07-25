using RimeDictManager.Utils;

namespace RimeDictManager.Services.Encoding.Encoders;

/// <summary> 星空键道6编码器 </summary>
internal class Xkjd6(string dictPath)
    : EncoderBase(dictPath), IVarLenEncoder
{
    public (byte, byte) CodeLengthRange => (3, 6);

    protected override Dictionary<char, string[]>
        LoadCharCodes(string dictPath)
        => DictFileUtils.GetEntriesParallel(dictPath)
        .ExtractCharCodes(3); // 只有前3码参与词组编码

    public override IEnumerable<string> Encode(string word)
    {
        var charsCodes = EncodingUtils.First3Last1(word, CharCodes);
        return (charsCodes.Length switch
        {
            2 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 select $"{c1[..2]}{c2[..2]}{c1[2]}{c2[2]}",
            3 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 from c3 in charsCodes[2]
                 select $"{c1[0]}{c2[0]}{c3[0]}{c1[2]}{c2[2]}{c3[2]}",
            4 => from c1 in charsCodes[0]
                 from c2 in charsCodes[1]
                 from c3 in charsCodes[2]
                 from c4 in charsCodes[3]
                 select $"{c1[0]}{c2[0]}{c3[0]}{c4[0]}{c1[2]}{c2[2]}",
            _ => []
        }).Distinct();
    }

    public IEnumerable<string> ShortenCodes(
        IEnumerable<string> fullCodes, int length) => length switch
        {
            6 => fullCodes,
            > 2 => fullCodes.Select(c => c[..length]),
            _ => throw new ArgumentOutOfRangeException(nameof(length),
                "码长超出星空键道6的有效范围！")
        };
}
