namespace RimeDictManager.VMs;

using Models;
using Services;

/// <summary> 词条的可变副本，供DataGrid编辑 </summary>
/// <param name="src"> 源词条 </param>
internal sealed class MutEntry(Entry src) {
    public Entry Src { get; } = src;

    public uint Num { get; } = src.Num;
    public string Text { get; set; } = src.Text;
    public string Code { get; set; } = src.Code ?? "";
    public string Weight { get; set; } = src.Weight ?? "";
    public string Stem { get; set; } = src.Stem ?? "";

    public bool Modified =>
        Text != Src.Text
     || Code != (Src.Code ?? "")
     || Weight != (Src.Weight ?? "")
     || Stem != (Src.Stem ?? "");

    public Entry? TryNewIfModified() =>
        Modified && LineCodec.TryNewEntry(Text, Code, Weight, Stem) is {} e
            ? e with { Num = Num }
            : null;
}
