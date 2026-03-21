namespace RimeDictManager.Models;

using static StringSplitOptions;

/// <summary> 词条 </summary>
/// <param name="Num"> 行号：1开头，新词条为0 </param>
/// <param name="Word"> 字词：必需 </param>
/// <param name="Code"> 编码：可选 </param>
/// <param name="Weight"> 权重：可选，"非负整数"或"浮点数%" </param>
/// <remarks> 参考 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
internal sealed record Entry(uint Num, string Word, string? Code, string? Weight): Line(Num, null) {
    public static Entry FromString(uint num, string line) =>
        line.Split('\t', 4, TrimEntries | RemoveEmptyEntries) is { Length: 1 or 2 or 3 } parts
     && !string.IsNullOrWhiteSpace(parts[0])
            ? new(num, parts[0], parts.ElementAtOrDefault(1), parts.ElementAtOrDefault(2))
            : throw new FormatException($"格式异常：第{num}行'{line}'");

    /// <summary> 尝试构造新词条 </summary>
    /// <param name="word"> 字词：必需 </param>
    /// <param name="code"> 编码：可选 </param>
    /// <param name="weight"> 权重：可选，"非负整数"或"浮点数%" </param>
    /// <returns> 非null则一定有效 </returns>
    public static Entry? TryNew(string word, string? code, string? weight) {
        word = word.Trim();
        code = TrimAndToNull(code);
        weight = TrimAndToNull(weight);
        return word.Length == 0 || (code is null && weight is {})
            ? null
            : new(0, word, code, weight);
    }

    private static string? TrimAndToNull(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : s.Trim();

    public override string ToString() =>
        (Code, Weight) switch {
            (_, {}) => $"{Word}\t{Code}\t{Weight}", ({}, _) => $"{Word}\t{Code}", _ => Word
        };
}
