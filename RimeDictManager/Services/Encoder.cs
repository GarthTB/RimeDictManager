namespace RimeDictManager.Services;

using System.Collections.Frozen;
using System.IO;
using MethodInfo
    = (byte MinLen, byte MaxLen, byte CharCodeLen, Func<string, IEnumerable<string>> Encode);

internal static class Encoder {
    private static FrozenDictionary<char, string[]>? _charsDict;

    public static readonly Dictionary<string, MethodInfo> Methods = new() {
        ["二笔 | 两笔"] = (4, 4, 2, static word => Encode2B(word).Distinct()),
        ["虎码"] = (4, 4, 2, static word => Encode5B(word).Distinct()),
        ["五笔"] = (4, 4, 2, static word => Encode5B(word).Distinct()),
        ["小鹤音形"] = (3, 4, 2, static word => Encode5B(word).Distinct()),
        ["星空键道6"] = (3, 6, 3, static word => EncodeJD6(word).Distinct())
    };

    /// <summary> 设置单字码表 </summary>
    /// <param name="path"> 路径 </param>
    /// <param name="charCodeLen"> 单字编码中参与词组编码部分的长度 </param>
    /// <returns> 覆盖字数 </returns>
    public static uint SetCharsDict(string path, byte charCodeLen) =>
        (uint)(_charsDict = File.ReadLines(path)
            .Select(static line => line.Split('\t', 3))
            .Where(parts => parts.Length > 1 // 有编码
                         && parts[0] is [not '#'] // 是单字条目
                         && parts[1].Length >= charCodeLen) // 码长够用
            .GroupBy(static parts => parts[0][0], static parts => parts[1])
            .ToFrozenDictionary(static g => g.Key, static g => g.Distinct().ToArray())).Count;

    private static IEnumerable<string> Encode2B(string word) =>
        CodesOfF3L1Chars(word, out var codes) switch {
            2 =>
                from s1 in codes[0]
                from s2 in codes[1]
                select $"{s1[..2]}{s2[..2]}",
            3 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                select $"{s1[..2]}{s2[0]}{s3[0]}",
            4 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                from s4 in codes[3]
                select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}",
            _ => []
        };

    private static IEnumerable<string> Encode5B(string word) =>
        CodesOfF3L1Chars(word, out var codes) switch {
            2 =>
                from s1 in codes[0]
                from s2 in codes[1]
                select $"{s1[..2]}{s2[..2]}",
            3 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                select $"{s1[0]}{s2[0]}{s3[..2]}",
            4 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                from s4 in codes[3]
                select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}",
            _ => []
        };

    private static IEnumerable<string> EncodeJD6(string word) =>
        CodesOfF3L1Chars(word, out var codes) switch {
            2 =>
                from s1 in codes[0]
                from s2 in codes[1]
                select $"{s1[..2]}{s2[..2]}{s1[2]}{s2[2]}",
            3 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                select $"{s1[0]}{s2[0]}{s3[0]}{s1[2]}{s2[2]}{s3[2]}",
            4 =>
                from s1 in codes[0]
                from s2 in codes[1]
                from s3 in codes[2]
                from s4 in codes[3]
                select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}{s1[2]}{s2[2]}",
            _ => []
        };

    /// <summary> 获取词组中前3个及末1个有效单字的编码 </summary>
    /// <param name="word"> 词组 </param>
    /// <param name="codes"> 各有效单字的所有编码 </param>
    /// <returns> 有效字数 </returns>
    private static byte CodesOfF3L1Chars(string word, out string[][] codes) {
        if (_charsDict is null) throw new InvalidOperationException("未设置单字码表");
        codes = new string[4][];

        byte cnt = 0;
        for (var i = 0; i < word.Length && cnt < 4; i++)
            if (_charsDict.TryGetValue(word[i], out var charCodes))
                codes[cnt++] = charCodes;
        if (cnt < 4) return cnt;

        for (var (i, found) = (word.Length - 1, false); i > 3 && !found; i--)
            if (_charsDict.TryGetValue(word[i], out var charCodes))
                (codes[3], found) = (charCodes, true);
        return 4;
    }
}
