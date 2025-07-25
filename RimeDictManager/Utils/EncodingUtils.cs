using RimeDictManager.Models;
using RimeDictManager.Services;
using RimeDictManager.Services.Encoding;

namespace RimeDictManager.Utils;

/// <summary> 用于编码相关的工具类 </summary>
internal static class EncodingUtils
{
    /// <summary>
    /// 提取所有单字词条中的前length码（忽略码长不足的词条）
    /// </summary>
    /// <returns> 单字 -> 所有前length码 的字典 </returns>
    public static Dictionary<char, string[]> ExtractCharCodes(
        this ParallelQuery<(int, Entry Entry)> entries, int length)
        => entries.Where(tuple
            => tuple.Entry.Word.Length == 1 // 是单字
            && tuple.Entry.Code.Length >= length) // 长度足够
        .GroupBy(
            static tuple => tuple.Entry.Word[0], // 按单字分组
            tuple => tuple.Entry.Code[..length]) // 取前length码
        .ToDictionary(
            static group => group.Key,
            static group => group.Distinct().ToArray()); // 去重数组

    /// <returns>
    /// 词组中前3个及末1个有效单字的编码。
    /// 若有效单字不足4个，则为所有有效单字的编码。
    /// </returns>
    public static string[][] First3Last1(
        string word, Dictionary<char, string[]> charCodes)
    {
        var result = new string[4][];
        int i = 0, j = word.Length - 1, count = 0;

        // 顺序找前4个有效单字的编码
        while (i < word.Length && count < 4)
            if (charCodes.TryGetValue(word[i++], out var codes))
                result[count++] = codes;

        // 若未遍历完就找到了4个，则找剩余部分的末1个有效单字的编码
        if (i < word.Length && count == 4)
            while (j >= i) // 不重复遍历，最坏情况O(n)
                if (charCodes.TryGetValue(word[j--], out var codes))
                { result[3] = codes; break; }

        return result[..count];
    }

    /// <summary> 获取某短编码对应的唯一且空闲的长编码 </summary>
    public static string? GetLengthenedCode(
        string code,
        string[] fullCodes,
        DictManager dictManager,
        IVarLenEncoder encoder)
    {
        for (var len = code.Length + 1; len <= fullCodes[0].Length; len++)
            if (encoder.ShortenCodes(fullCodes, len)
                .SingleOrDefault(c => c.StartsWith(code))
                is string c
                && dictManager.SearchCode(c, true).Count == 0)
                return c;
        return null;
    }
}
