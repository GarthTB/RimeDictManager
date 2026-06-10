namespace RimeDictManager.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;

/// <summary> 词库信息：DataGrid专供 </summary>
public sealed partial class DictInfo(Dict src): ObservableObject {
    public Dict Src { get; } = src;
    public string Name { get; } = src.Name;
    public string Path { get; } = src.Path;
    public uint Cnt { get; } = src.Cnt;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(ModText))]
    public partial bool Mod { get; private set; } = src.Mod;

    public string ModText =>
        Mod
            ? "有"
            : "无";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(TgtText))]
    public partial bool Tgt { get; private set; }

    public string TgtText =>
        Tgt
            ? "是"
            : "否";

    public void NotifySaved() => Mod = false;
    public void SetTgt(bool tgt) => Tgt = tgt;
}
