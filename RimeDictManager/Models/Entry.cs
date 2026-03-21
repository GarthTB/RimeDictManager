namespace RimeDictManager.Models;

/// <summary> 词条 </summary>
/// <param name="Num"> 行号：1开头，新词条为0 </param>
/// <param name="Word"> 字词：必需 </param>
/// <param name="Code"> 编码：可选 </param>
/// <param name="Weight"> 权重：可选，"非负整数"或"浮点数%" </param>
/// <remarks> 参考 https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
internal sealed record Entry(uint Num, string Word, string? Code, string? Weight): Line(Num, null) {
    public static Entry FromString(uint num, string line) =>
        line.Split('\t', 4) is [{ Length: > 0 } word, ..] parts and { Length: < 4 }
            ? new(num, word, parts.ElementAtOrDefault(1), parts.ElementAtOrDefault(2))
            : throw new NotSupportedException($"格式不支持：第{num}行'{line}'");

    public override string ToString() =>
        Weight is {}
            ? $"{Word}\t{Code}\t{Weight}"
            : Code is {}
                ? $"{Word}\t{Code}"
                : Word;
}
