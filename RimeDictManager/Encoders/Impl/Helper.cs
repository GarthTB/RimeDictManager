namespace RimeDictManager.Encoders.Impl;

using System.Collections.Frozen;
using System.IO;

/// <summary> 编码器工具 </summary>
internal static class Helper
{
    /// <summary> 将单字词库加载为字典 </summary>
    /// <param name="path"> 单字词库路径 </param>
    /// <param name="len"> 编码截取长度 </param>
    /// <returns> 单字 -> 前len码的集合 </returns>
    public static FrozenDictionary<char, string[]> ToCharsDict(this string path, byte len) =>
        File.ReadLines(path)
            .Select(static line => line.Split('\t', 3))
            .Where(parts => parts.Length > 1 // 有编码
                         && parts[0].Length == 1 // 是单字
                         && parts[0][0] != '#' // 是条目
                         && parts[1].Length >= len) // 码长够
            .GroupBy(static parts => parts[0][0], parts => parts[1][..len])
            .ToFrozenDictionary(static g => g.Key, static g => g.Distinct().ToArray());

    /// <summary> 获取词组中前3个及末1个有效单字的编码 </summary>
    /// <param name="word"> 词组 </param>
    /// <param name="charsDict"> 单字词库 </param>
    /// <param name="codes"> 各有效单字的编码集合 </param>
    /// <returns> 有效字数 </returns>
    public static int First3Last1(
        this string word,
        FrozenDictionary<char, string[]> charsDict,
        out string[][] codes) {
        codes = new string[4][];
        var cnt = 0;
        for (var i = 0; i < word.Length && cnt < 4; i++)
            if (charsDict.TryGetValue(word[i], out var charCodes))
                codes[cnt++] = charCodes;
        if (cnt < 4)
            return cnt;

        for (var (i, found) = (word.Length - 1, false); i > 3 && !found; i--)
            if (charsDict.TryGetValue(word[i], out var charCodes))
                (codes[3], found) = (charCodes, true);
        return 4;
    }
}
