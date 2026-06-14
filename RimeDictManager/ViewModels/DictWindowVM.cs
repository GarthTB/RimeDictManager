namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services.Core;
using Services.Utils;
using ZLinq;

public sealed partial class DictWindowVM: ObservableObject {
    public DictWindowVM() {
        foreach (var dict in DictManager.AllDicts) Dicts.Add(new(dict));
        Dicts.FirstOrDefault()?.SetTgt(true);
        foreach (var dict in Encoder.AllDicts) SingleDicts.Add(dict);
    }

    [ObservableProperty] public partial InputMethod SelInputMethod { get; set; } = Encoder.Method;

    #region 词库

    public ObservableCollection<DictVM> Dicts { get; } = [];

    [ObservableProperty,
     NotifyCanExecuteChangedFor(nameof(RemoveDictCommand), nameof(SetTgtDictCommand)),
     NotifyPropertyChangedFor(nameof(SelDictMod))]
    public partial DictVM? SelDict { get; set; }

    private bool HasSelDict => SelDict is {};
    public bool SelDictMod => SelDict is { Mod: true };
    private bool SelDictNotTgt => SelDict is { Tgt: false };

    public void AddDict(string path) {
        DictVM dict = new(DictManager.AddDict(path));
        Dicts.Add(dict);
        if (Dicts.Count == 1) dict.SetTgt(true);
    }

    [RelayCommand(CanExecute = nameof(HasSelDict))]
    private async Task RemoveDict() {
        try {
            var dict = SelDict!;
            IDictInfo? tgt = null;
            if (!await DictManager.RemoveDict(dict.Src, x => tgt = x)) return;
            if (!Dicts.Remove(dict)) throw new UnreachableException("不可能错误，请停用并报告：词库集合VM与底层相违");
            if (tgt is {}) Dicts.AsValueEnumerable().First(x => x.Src == tgt).SetTgt(true);
        } catch (Exception ex) { await ex.Alert("移除词库"); }
    }

    [RelayCommand(CanExecute = nameof(SelDictNotTgt))]
    private async Task SetTgtDict() {
        try {
            var dict = SelDict!;
            var old = DictManager.SetTgtDict(dict.Src);
            dict.SetTgt(true);
            Dicts.AsValueEnumerable().First(x => x.Src == old).SetTgt(false);
        } catch (Exception ex) { await ex.Alert("设置加词目标"); }
    }

    #endregion 词库

    #region 单字

    public ObservableCollection<SingleDict> SingleDicts { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveSingleDictCommand))]
    public partial SingleDict? SelSingleDict { get; set; }

    private bool HasSelSingleDict => SelSingleDict is {};

    public void AddSingleDict(string path) => SingleDicts.Add(Encoder.AddDict(path));

    [RelayCommand(CanExecute = nameof(HasSelSingleDict))]
    private async Task RemoveSingleDict() {
        try {
            var dict = SelSingleDict!;
            Encoder.RemoveDict(dict);
            if (!SingleDicts.Remove(dict))
                throw new UnreachableException("不可能错误，请停用并报告：单字集合VM与底层相违");
        } catch (Exception ex) { await ex.Alert("移除单字码表"); }
    }

    #endregion 单字
}
