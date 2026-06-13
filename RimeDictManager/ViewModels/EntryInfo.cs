namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;
using Services.Data;

public sealed partial class EntryInfo(string dict, IReadOnlyList<Col> cols, EntryLine src)
    : ObservableObject {
    public string Dict { get; } = dict;
    public EntryLine Src { get; } = src;

    public uint Num { get; } = src.Num;
    [ObservableProperty] public partial string Text { get; set; } = src.Text;
    [ObservableProperty] public partial string Code { get; set; } = src.Code ?? "";
    [ObservableProperty] public partial string Weight { get; set; } = src.Weight ?? "";
    [ObservableProperty] public partial string Stem { get; set; } = src.Stem ?? "";

    public EntryLine? TryNewIfMod() =>
        Text != Src.Text
     && Code != (Src.Code ?? "")
     && Weight != (Src.Weight ?? "")
     && Stem != (Src.Stem ?? "")
            ? LineCodec.TryNewEntry(Num, Text, Code, Weight, Stem, cols)
            : null;
}
