namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;
using ZLinq;
using OpEx = InvalidOperationException;

public sealed partial class DictWindowVM: ObservableObject {
    public DictWindowVM() => RefreshState();

    [ObservableProperty] public partial InputMethod SelInputMethod { get; set; } = Encoder.Method;

    private void RefreshState() {
        foreach (var dict in DictManager.AllDicts) Dicts.Add(new(dict));
        Dicts.FirstOrDefault()?.SetTgt(true);
        foreach (var dict in Encoder.AllDicts) SingleDicts.Add(dict);
        OnPropertyChanged(nameof(CanSelectMethod));
    }

    #region 添加目录

    public async Task<(uint, uint)> LoadDirAsync(string dir) {
        var dict = await DictManager.LoadDirAsync(dir);
        var single = await Encoder.LoadDirAsync(dir);
        if (dict + single == 0) return (dict, single);
        Dicts.Clear();
        SingleDicts.Clear();
        RefreshState();
        return (dict, single);
    }

    #endregion 添加目录

    #region 词库

    public ObservableCollection<DictVM> Dicts { get; } = [];

    [ObservableProperty,
     NotifyCanExecuteChangedFor(nameof(RemoveDictCommand), nameof(SetTgtDictCommand)),
     NotifyPropertyChangedFor(nameof(SelDictModified))]
    public partial DictVM? SelDict { get; set; }

    private bool HasSelDict => SelDict is {};
    public bool SelDictModified => SelDict is { Modified: true };
    private bool SelDictNotTgt => SelDict is { Tgt: false };

    public async Task AddDictAsync(string path) {
        var dict = await DictManager.AddDictAsync(path);
        DictVM dictVM = new(dict);
        Dicts.Add(dictVM);
        if (Dicts.Count == 1) dictVM.SetTgt(true);
    }

    [RelayCommand(CanExecute = nameof(HasSelDict))]
    private async Task RemoveDictAsync() {
        try {
            var dictVM = SelDict ?? throw new OpEx("UI 错误");
            var (done, tgt) = await DictManager.RemoveDictAsync(dictVM.Src);
            if (!done) return;
            if (!Dicts.Remove(dictVM)) throw new OpEx("UI 移除词库失败");
            if (tgt is {}) Dicts.AsValueEnumerable().First(x => x.Src == tgt).SetTgt(true);
        } catch (Exception ex) { await ex.AlertAsync("移除词库"); }
    }

    public async Task SaveAsync(DictVM dictVM, string? path, bool reorder) {
        await DictManager.SaveAsync(dictVM.Src, path, reorder);
        dictVM.NotifySaved();
        OnPropertyChanged(nameof(SelDictModified));
    }

    [RelayCommand(CanExecute = nameof(SelDictNotTgt))]
    private async Task SetTgtDictAsync() {
        try {
            var dictVM = SelDict ?? throw new OpEx("UI 错误");
            var prev = DictManager.SetTgtDict(dictVM.Src);
            dictVM.SetTgt(true);
            Dicts.AsValueEnumerable().First(x => x.Src == prev).SetTgt(false);
        } catch (Exception ex) { await ex.AlertAsync("设置加词目标"); }
    }

    #endregion 词库

    #region 单字码表

    public ObservableCollection<SingleDict> SingleDicts { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveSingleDictCommand))]
    public partial SingleDict? SelSingleDict { get; set; }

    public bool CanSelectMethod => SingleDicts.Count > 0;
    private bool HasSelSingleDict => SelSingleDict is {};

    public async Task AddSingleDictAsync(string path) {
        var dict = await Encoder.AddDictAsync(path);
        SingleDicts.Add(dict);
        OnPropertyChanged(nameof(CanSelectMethod));
    }

    [RelayCommand(CanExecute = nameof(HasSelSingleDict))]
    private async Task RemoveSingleDictAsync() {
        try {
            var dict = SelSingleDict ?? throw new OpEx("UI 错误");
            Encoder.RemoveDict(dict);
            if (!SingleDicts.Remove(dict)) throw new OpEx("UI 移除单字码表失败");
            OnPropertyChanged(nameof(CanSelectMethod));
        } catch (Exception ex) { await ex.AlertAsync("移除单字码表"); }
    }

    #endregion 单字码表
}
