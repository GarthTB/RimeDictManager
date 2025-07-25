namespace RimeDictManager.Models;

/// <summary> 词条（.dict.yaml文件中的一行） </summary>
/// <param name="Word"> 词组（不能为空） </param>
/// <param name="Code"> 编码（不能为空） </param>
/// <param name="Weight"> 权重（可为空） </param>
/// <param name="Stem"> 造词码（可为空） </param>
internal record Entry(
    string Word, string Code, string Weight, string Stem)
{
    /// <returns> 词条是否有效 </returns>
    public bool IsValid
        => !string.IsNullOrWhiteSpace(Word)
        && !string.IsNullOrWhiteSpace(Code);

    /// <summary> 排序的优先级 </summary>
    public (string, double, string, string) OrderKey
        => (Code, -WeightValue ?? 0, Word, Stem);

    /// <summary> 权重值 </summary>
    public double? WeightValue { get; }
        = double.TryParse(Weight, out var w) ? w : null;

    /// <summary> 词条的字符串形式 </summary>
    public override string ToString()
        => !string.IsNullOrWhiteSpace(Stem)
        ? $"{Word}\t{Code}\t{Weight}\t{Stem}"
        : WeightValue.HasValue
        ? $"{Word}\t{Code}\t{Weight}"
        : $"{Word}\t{Code}";
}
