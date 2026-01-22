namespace RimeDictManager.ViewModels;

using Models;

/// <summary> 词库条目的可变副本，供DataGrid编辑 </summary>
/// <param name="Src"> 源条目 </param>
internal sealed record MutEntry(Line Src)
{
    public string Idx { get; } = $"{Src.Idx}";
    public string Word { get; set; } = Src.Word!;
    public string Code { get; set; } = Src.Code ?? "";
    public string Weight { get; set; } = Src.Weight ?? "";

    /// <summary> 将可变副本构造为新条目行 </summary>
    /// <param name="entry"> 新条目行：无改动则为null </param>
    /// <returns> 是否有改动 </returns>
    public bool ToNewEntry(out Line? entry) {
        var word = Word.Trim();
        var code = string.IsNullOrWhiteSpace(Code)
            ? null
            : Code.Trim();
        var weight = string.IsNullOrWhiteSpace(Weight)
            ? null
            : Weight.Trim();
        entry = word == Src.Word && code == Src.Code && weight == Src.Weight
            ? null
            : Src with { Word = word, Code = code, Weight = weight };
        return entry is {};
    }
}
