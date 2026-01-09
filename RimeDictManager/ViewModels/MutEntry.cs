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

    /// <summary> 将可变副本构造为新条目行 </summary>
    /// <param name="entry"> 新条目行：无改动则为null </param>
    /// <returns> 是否有改动 </returns>
    public bool ToNewEntry(out Line? entry) {
        var (word, code, weight) = (Word.Trim(), Code.Trim(), Weight.Trim());
        if (code.Length == 0)
            code = null;
        if (weight.Length == 0)
            weight = null;

        var modified = word != Src.Word || code != Src.Code || weight != Src.Weight;
        entry = modified
            ? Src with { Word = word, Code = code, Weight = weight }
            : null;
        return modified;
    }
}
