namespace RimeDictManager.Core.Models;

/// <summary> 词库的一行 </summary>
/// <param name="Idx"> 行索引（新条目为null） </param>
/// <param name="Word"> 字词：条目行必需 </param>
/// <param name="Code"> 编码：条目行可省 </param>
/// <param name="Weight"> 权重：条目行可省，"非负整数"或"浮点数%" </param>
/// <param name="Comment"> 整行注释 </param>
/// <remarks> 格式标准见 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
internal sealed record Line(uint? Idx, string? Word, string? Code, string? Weight, string? Comment)
{
    /// <summary> 将字符串解析为对象 </summary>
    /// <param name="index"> 行索引 </param>
    /// <param name="line"> 词库的一行 </param>
    public static Line FromString(uint index, string line) {
        if (line.Length == 0) // 空行不含任何字符
            return new(index, null, null, null, null);
        if (line.StartsWith('#')) // 注释行的行首为#
            return new(index, null, null, null, line);
        if (line.Split('\t', 4) is not { Length: < 4 } parts)
            throw new FormatException($"词库第{index + 1}行格式错误：{line}");
        var word = parts.ElementAtOrDefault(0);
        var code = parts.ElementAtOrDefault(1);
        var weight = parts.ElementAtOrDefault(2);
        return new(index, word, code, weight, null);
    }

    /// <summary> 将对象转换为字符串 </summary>
    public override string ToString() =>
        (Word, Code, Weight, Comment) switch {
            (null, null, null, _) => Comment ?? "",
            ({}, null, null, null) => Word,
            ({}, {}, null, null) => $"{Word}\t{Code}",
            ({}, {}, {}, null) => $"{Word}\t{Code}\t{Weight}",
            _ => throw new FormatException("对象格式错误！请报告异常！")
        };
}
