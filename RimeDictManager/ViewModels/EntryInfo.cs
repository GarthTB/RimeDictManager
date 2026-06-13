namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;
using Services.Data;

public sealed partial class EntryInfo(EntryLine src, Dict dict): ObservableObject {
    public EntryLine Src { get; } = src;
    public string DictName { get; } = dict.Name;
    public string DictPath { get; } = dict.Path;
    public IReadOnlyList<Col> Cols { get; } = dict.Cols;
    public uint Num => Src.Num;
    [ObservableProperty] public partial string Text { get; set; } = src.Text;
    [ObservableProperty] public partial string Code { get; set; } = src.Code ?? "";
    [ObservableProperty] public partial string Weight { get; set; } = src.Weight ?? "";
    [ObservableProperty] public partial string Stem { get; set; } = src.Stem ?? "";

    public bool TryNewIfMod(out EntryLine aft) {
        if (Text != Src.Text
         && Code != (Src.Code ?? "")
         && Weight != (Src.Weight ?? "")
         && Stem != (Src.Stem ?? ""))
            return LineCodec.TryNewEntry(Num, Text, Code, Weight, Stem, Cols, out aft);
        aft = default;
        return false;
    }
}
