namespace RimeDictManager.VMs;

using Models;

/// <summary> 词条的可变副本，供DataGrid编辑 </summary>
/// <param name="src"> 源词条 </param>
internal sealed class MutEntry(Entry src) {
    public Entry Src => src;
    public uint Num { get; } = src.Num;
    public string Word { get; set; } = src.Word;
    public string Code { get; set; } = src.Code ?? "";
    public string Weight { get; set; } = src.Weight ?? "";
    public bool Modified => (Word, Code, Weight) != (src.Word, src.Code ?? "", src.Weight ?? "");

    /// <summary> 用当前属性构造新词条 </summary>
    /// <returns> 非null则有改动且新词条有效 </returns>
    public Entry? TryNew() =>
        Modified && Entry.TryNew(Word, Code, Weight) is {} entry
            ? entry with { Num = Num }
            : null;
}
