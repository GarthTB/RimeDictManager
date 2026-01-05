namespace RimeDictManager.Core.Models;

/// <summary> 词库的一行 </summary>
/// <param name="Idx"> 行索引（新条目为null） </param>
/// <param name="Word"> 字词：条目行必需 </param>
/// <param name="Code"> 编码：条目行可选 </param>
/// <param name="Weight"> 权重：条目行可选，"非负整数"或"浮点数%" </param>
/// <param name="Comment"> 注释：条目行为null </param>
/// <remarks> 格式标准见 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
internal sealed record Line(uint? Idx, string? Word, string? Code, string? Weight, string? Comment)
{
    /// <summary> 将字符串解析为对象，一定有效 </summary>
    /// <param name="index"> 行索引 </param>
    /// <param name="line"> 词库的一行 </param>
    public static Line FromString(uint index, string line) {
        if (line.Length == 0) // 空行不含任何字符
            return new(index, null, null, null, null);
        if (line.StartsWith('#')) // 注释行的行首为#
            return new(index, null, null, null, line);
        if (line.Split('\t', 4) is { Length: < 4 } parts // 条目行最多3列
         && parts.ElementAtOrDefault(0) is {} word) // 且必须有字词
            return new(index, word, parts.ElementAtOrDefault(1), parts.ElementAtOrDefault(2), null);
        throw new FormatException($"词库第{index + 1}行格式错误：{line}");
    }

    /// <summary> 将对象转换为字符串 </summary>
    public override string ToString() =>
        (Word, Code, Weight, Comment) switch {
            (null, null, null, _) => Comment ?? "", // 注释行或空行
            ({}, null, null, null) => Word,
            ({}, {}, null, null) => $"{Word}\t{Code}",
            ({}, {}, {}, null) => $"{Word}\t{Code}\t{Weight}",
            _ => throw new FormatException("条目格式错误，请报告异常")
        };
}
