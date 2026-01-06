namespace RimeDictManager.Core.Encoding.Encoders;

/// <summary> 编码器工具 </summary>
internal static class Utils
{
    /// <summary> 将单字词库加载为字典 </summary>
    /// <param name="lines"> 单字词库的各行 </param>
    /// <param name="len"> 截取前len码 </param>
    /// <returns> 单字 -> 前len码的集合 </returns>
    public static Dictionary<char, string[]>
        LoadCharsDict(this IEnumerable<string> lines, byte len) =>
        lines.Where(static line => line.Length > 0 && line[0] != '#') // 条目行
            .Select(static line => line.Split('\t', 3))
            .Where(parts => parts.Length > 1 // 有编码
                         && parts[0].Length == 1 // 是单字
                         && parts[1].Length >= len) // 码长够
            .GroupBy(static parts => parts[0][0], parts => parts[1][..len])
            .ToDictionary(static g => g.Key, static g => g.Distinct().ToArray());

    /// <summary> 获取词组中前3个及末1个有效单字的编码 </summary>
    /// <param name="word"> 词组 </param>
    /// <param name="charsDict"> 单字词库 </param>
    /// <returns> 各有效单字的编码集合 </returns>
    public static List<string[]> F3L1Codes(this string word, Dictionary<char, string[]> charsDict) {
        List<string[]> set = new(4);
        for (var i = 0; i < word.Length && set.Count < 4; i++)
            if (charsDict.TryGetValue(word[i], out var codes))
                set.Add(codes);

        if (set.Count != 4)
            return set;

        for (var (i, ori) = (word.Length - 1, set[3]); i >= 4 && set[3] == ori; i--)
            if (charsDict.TryGetValue(word[i], out var codes))
                set[3] = codes;

        return set;
    }
}
