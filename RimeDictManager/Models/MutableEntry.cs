namespace RimeDictManager.Models;

/// <summary> 词条的可变副本，供DataGrid编辑 </summary>
internal class MutableEntry(Entry entry)
{
    /// <summary> 原始词条 </summary>
    public Entry OriginalEntry { get; } = entry;

    public string Word { get; set; } = entry.Word;
    public string Code { get; set; } = entry.Code;
    public string Weight { get; set; } = entry.Weight;
    public string Stem { get; set; } = entry.Stem;

    /// <summary> 当前词条 </summary>
    public Entry CurrentEntry => new(
        Word.Trim(), Code.Trim(), Weight.Trim(), Stem.Trim());
}
