namespace RimeDictManager.Models;

/// <summary> 词库的一行 </summary>
/// <param name="Idx"> 行索引（新条目为null） </param>
/// <param name="Word"> 字词：条目行必需 </param>
/// <param name="Code"> 编码：条目行可选 </param>
/// <param name="Weight"> 权重：条目行可选，"非负整数"或"浮点数%" </param>
/// <param name="Comment"> 注释：条目行为null </param>
/// <remarks> 格式标准见 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
internal sealed record Line(uint? Idx, string? Word, string? Code, string? Weight, string? Comment)
{
    /// <summary> 空行为null，注释行为false，条目行为true，格式错误则抛出 </summary>
    public bool? IsEntry =>
        (Idx, Word, Comment) switch {
            ({}, null, null) => null, // 空行
            ({}, null, { Length: > 0 }) => false, // 注释行
            (_, { Length: > 0 }, null) => true, // 条目行
            _ => throw new FormatException("条目格式错误，请报告异常")
        };

    /// <summary> 将字符串解析为对象 </summary>
    /// <param name="idx"> 行索引 </param>
    /// <param name="line"> 词库的一行 </param>
    /// <returns> 一定有效的一行 </returns>
    public static Line FromString(uint idx, string line) {
        if (line.Length == 0) // 空行不含任何字符
            return new(idx, null, null, null, null);
        if (line[0] == '#') // 注释行的行首为#
            return new(idx, null, null, null, line);
        if (line.Split('\t', 4) is { Length: < 4 } parts // 条目行最多3列
         && parts.ElementAtOrDefault(0) is { Length: > 0 } word) // 且必须有字词
            return new(idx, word, parts.ElementAtOrDefault(1), parts.ElementAtOrDefault(2), null);
        throw new FormatException($"词库第{idx + 1}行格式错误：{line}");
    }

    /// <summary> 将对象转换为字符串 </summary>
    /// <remarks> 需与FromString可逆 </remarks>
    public override string ToString() =>
        IsEntry != true
            ? Comment ?? "" // 空行或注释行
            : (Code, Weight) switch { // 条目行
                (null, null) => Word!,
                ({}, null) => $"{Word}\t{Code}",
                ({}, {}) => $"{Word}\t{Code}\t{Weight}",
                _ => throw new FormatException("条目格式错误，请报告异常")
            };
}
