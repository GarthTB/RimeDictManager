namespace RimeDictManager.VMs;

using Models;

/// <summary> 词条的可变副本，供DataGrid编辑 </summary>
/// <param name="Src"> 源词条 </param>
internal sealed record MutEntry(Entry Src) {
    public uint Num { get; } = Src.Num;
    public string Word { get; set; } = Src.Word;
    public string Code { get; set; } = Src.Code ?? "";
    public string Weight { get; set; } = Src.Weight ?? "";
    public bool Modified => (Word, Code, Weight) != (Src.Word, Src.Code ?? "", Src.Weight ?? "");

    /// <summary> 用当前属性构造新词条 </summary>
    /// <returns> 非null则有改动且新词条有效 </returns>
    public Entry? TryNew() =>
        Modified && Entry.TryNew(Word, Code, Weight) is {} entry
            ? entry with { Num = Num }
            : null;
}
