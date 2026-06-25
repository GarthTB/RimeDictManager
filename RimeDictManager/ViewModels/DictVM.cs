namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;

public sealed partial class DictVM(IDictInfo src): ObservableObject {
    public IDictInfo Src { get; } = src;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ModifiedText))]
    public partial bool Modified { get; private set; } = src.Modified;

    public string ModifiedText =>
        Modified
            ? "●"
            : "○";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(TgtText))]
    public partial bool Tgt { get; private set; }

    public string TgtText =>
        Tgt
            ? "●"
            : "";

    public void NotifySaved() => Modified = false;
    public void SetTgt(bool tgt) => Tgt = tgt;
}
