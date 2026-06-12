namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;

public sealed partial class DictInfo(Dict src): ObservableObject {
    public string Name { get; } = src.Name;
    public string Path { get; } = src.Path;
    public uint Cnt { get; } = src.Cnt;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ModText))]
    public partial bool Mod { get; private set; } = src.Mod;

    public string ModText =>
        Mod
            ? "●"
            : "○";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(TgtText))]
    public partial bool Tgt { get; private set; }

    public string TgtText =>
        Tgt
            ? "●"
            : "";

    public void NotifySaved() => Mod = false;
    public void SetTgt(bool tgt) => Tgt = tgt;
}
