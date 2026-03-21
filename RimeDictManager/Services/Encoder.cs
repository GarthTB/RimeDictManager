namespace RimeDictManager.Services;

using System.Collections.Frozen;
using System.IO;

internal static class Encoder {
    private static FrozenDictionary<char, string[]>? _charsDict;
    private static string? _curMethod;
    public static bool Ready => _charsDict?.Count > 0;
    public static IReadOnlyList<string> Methods => ["二笔 | 两笔", "虎码", "五笔", "小鹤音形", "星空键道6"];

    /// <summary> 设置编码方案 </summary>
    /// <param name="name"> 方案名 </param>
    /// <returns> 有效码长范围 </returns>
    public static (byte MinCodeLen, byte MaxCodeLen) SetMethod(string name) =>
        (_curMethod = name) switch {
            "二笔 | 两笔" or "虎码" or "五笔" => (4, 4), "小鹤音形" => (3, 4), _ => (3, 6)
        };

    /// <summary> 设置码表 </summary>
    /// <param name="path"> 路径：null时重置状态 </param>
    /// <returns> 覆盖字数 </returns>
    public static uint SetCharsDict(string? path) {
        if (path is null) {
            _charsDict = null;
            return 0;
        }
        var stemLen = _curMethod == "星空键道6"
            ? 3 // 键道6单字前3码参与词组编码
            : 2; // 其他方案单字前2码参与词组编码
        _charsDict = File.ReadLines(path)
            .Select(static line => line.Split('\t', 3, StringSplitOptions.TrimEntries))
            .Where(parts => parts is [[not '#'], var code, ..] && code.Length >= stemLen) // 是单字且码长够
            .GroupBy(static parts => parts[0][0], static parts => parts[1])
            .ToFrozenDictionary(static g => g.Key, static g => g.Distinct().ToArray());
        if (_charsDict.Count == 0) _charsDict = null;
        return (uint?)_charsDict?.Count ?? 0;
    }

    /// <summary> 为词组编码 </summary>
    /// <param name="word"> 词组 </param>
    /// <returns> 无重、无序的所有全码 </returns>
    public static IEnumerable<string> Encode(string word) =>
        _curMethod switch {
            "二笔 | 两笔" => Encode2B(word).Distinct(),
            "虎码" or "五笔" or "小鹤音形" => Encode5B(word).Distinct(),
            _ => EncodeJD6(word).Distinct()
        };

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
        if (_charsDict is not { Count: > 0 }) throw new InvalidOperationException("码表无效");
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
