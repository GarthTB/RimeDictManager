namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;

public sealed partial class DictWindowVM: ObservableObject {
    public DictWindowVM() {
        foreach (var dict in DictManager.DictInfos) DictInfos.Add(dict);
        if (DictInfos.Count > 0) DictInfos[0].SetTgt(true);
        foreach (var dict in Encoder.DictList) SingleDicts.Add(dict);
    }

    #region 词库

    public ObservableCollection<DictInfo> DictInfos { get; } = [];

    [ObservableProperty,
     NotifyCanExecuteChangedFor(
         nameof(RemoveDictCommand),
         nameof(SaveDictCommand),
         nameof(SetTgtDictCommand))]
    public partial DictInfo? SelDictInfo { get; set; }

    private bool HasSelDictInfo => SelDictInfo is {};
    private bool SelDictInfoMod => SelDictInfo is { Mod: true };
    private bool SelDictInfoNotTgt => SelDictInfo is { Tgt: false };

    public void AddDict(string path) {
        DictInfos.Add(DictManager.AddDict(path));
        if (DictInfos is [var dict]) dict.SetTgt(true);
    }

    [RelayCommand(CanExecute = nameof(HasSelDictInfo))]
    private Task RemoveDict() => throw new NotImplementedException();

    [RelayCommand(CanExecute = nameof(SelDictInfoMod))]
    private Task SaveDict() => throw new NotImplementedException();

    [RelayCommand(CanExecute = nameof(SelDictInfoNotTgt))]
    private Task SetTgtDict() => throw new NotImplementedException();

    #endregion 词库

    #region 单字

    public ObservableCollection<SingleDict> SingleDicts { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveSingleDictCommand))]
    public partial SingleDict? SelSingleDict { get; set; }

    private bool HasSelSingleDict => SelSingleDict is {};

    public static IReadOnlyList<EncodeMethod> EncodeMethods => EncodeMethod.All;
    [ObservableProperty] public partial EncodeMethod SelEncodeMethod { get; set; } = Encoder.Method;

    public void AddSingleDict(string path) => SingleDicts.Add(Encoder.AddDict(path));

    [RelayCommand(CanExecute = nameof(HasSelSingleDict))]
    private Task RemoveSingleDict() => throw new NotImplementedException();

    #endregion 单字
}
