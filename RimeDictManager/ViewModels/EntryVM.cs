namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;
using Models.Serde;

public sealed partial class EntryVM(DictEntry src): ObservableObject {
    public DictEntry Src { get; } = src;

    public uint Num => Src.Entry.Num;
    [ObservableProperty] public partial string Text { get; set; } = src.Entry.Text;
    [ObservableProperty] public partial string Code { get; set; } = src.Entry.Code ?? "";
    [ObservableProperty] public partial string Weight { get; set; } = src.Entry.Weight ?? "";
    [ObservableProperty] public partial string Stem { get; set; } = src.Entry.Stem ?? "";

    public bool TryNewIfMod(out EntryLine aft) {
        if (Text != Src.Entry.Text
         || Code != (Src.Entry.Code ?? "")
         || Weight != (Src.Entry.Weight ?? "")
         || Stem != (Src.Entry.Stem ?? ""))
            return LineCodec.TryNewEntry(Num, Text, Code, Weight, Stem, Src.Dict.Cols, out aft);
        aft = default;
        return false;
    }
}
