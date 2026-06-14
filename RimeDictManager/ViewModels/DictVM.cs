namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;

public sealed partial class DictVM(IDictInfo src): ObservableObject {
    public IDictInfo Src { get; } = src;

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
