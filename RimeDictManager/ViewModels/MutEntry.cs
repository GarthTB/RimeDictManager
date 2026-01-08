namespace RimeDictManager.ViewModels;

using Models;

/// <summary> 词库条目的可变副本，供DataGrid编辑 </summary>
/// <param name="srcEntry"> 源条目 </param>
internal class MutEntry(Line srcEntry)
{
    /// <summary> 字词：条目行必需 </summary>
    public string Word { get; set; } = srcEntry.Word ?? "";

    /// <summary> 编码：条目行可选 </summary>
    public string Code { get; set; } = srcEntry.Code ?? "";

    /// <summary> 权重：条目行可选，"非负整数"或"浮点数%" </summary>
    public string Weight { get; set; } = srcEntry.Weight ?? "";

    /// <summary> 是否已修改 </summary>
    public bool Modified =>
        (srcEntry.Word ?? "") != Word.Trim()
     || (srcEntry.Code ?? "") != Code.Trim()
     || (srcEntry.Weight ?? "") != Weight.Trim();

    /// <summary> 将可变副本构造为条目行 </summary>
    /// <returns> 新条目，待检验有效性 </returns>
    public Line ToLine() => new(null, Word.Trim(), Code.Trim(), Weight.Trim(), null);
}
