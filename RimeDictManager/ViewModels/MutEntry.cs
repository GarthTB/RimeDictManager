namespace RimeDictManager.ViewModels;

using Models;

/// <summary> 词库条目的可变副本，供DataGrid编辑 </summary>
/// <param name="src"> 源条目 </param>
internal sealed class MutEntry(Line src)
{
    public readonly Line Src = src;
    public string Idx { get; } = $"{src.Idx}";
    public string Word { get; set; } = src.Word!;
    public string Code { get; set; } = src.Code ?? "";
    public string Weight { get; set; } = src.Weight ?? "";

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
        Word.Trim() != Src.Word! || CurCode != Src.Code || CurWeight != Src.Weight;

    /// <summary> 将可变副本构造为条目行 </summary>
    /// <returns> 新条目，待检验有效性 </returns>
    public Line ToLine() => Src with { Word = Word.Trim(), Code = CurCode, Weight = CurWeight };
}
