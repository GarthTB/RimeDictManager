namespace RimeDictManager.ViewModels;

using Models;

/// <summary> 词库条目的可变副本，供DataGrid编辑 </summary>
/// <param name="srcEntry"> 源条目 </param>
internal sealed class MutEntry(Line srcEntry)
{
    public string Idx { get; } = $"{srcEntry.Idx}";
    public string Word { get; set; } = srcEntry.Word!;
    public string Code { get; set; } = srcEntry.Code ?? "";
    public string Weight { get; set; } = srcEntry.Weight ?? "";

    private string? CurCode =>
        Code.Trim() is { Length: > 0 } code
            ? code
            : null;

    private string? CurWeight =>
        Weight.Trim() is { Length: > 0 } weight
            ? weight
            : null;

    /// <summary> 是否有改动 </summary>
    public bool Modified =>
        Word.Trim() != srcEntry.Word! || CurCode != srcEntry.Code || CurWeight != srcEntry.Weight;

    /// <summary> 将可变副本构造为条目行 </summary>
    /// <returns> 新条目，待检验有效性 </returns>
    public Line ToLine() =>
        srcEntry with { Word = Word.Trim(), Code = CurCode, Weight = CurWeight };
}
