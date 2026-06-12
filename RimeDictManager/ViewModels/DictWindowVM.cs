namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;
using FatalEx = System.Diagnostics.UnreachableException;

public sealed partial class DictWindowVM: ObservableObject {
    public DictWindowVM() {
        foreach (var dict in DictManager.DictInfos) DictInfos.Add(dict);
        if (DictInfos is [var first, ..]) first.SetTgt(true);
        foreach (var dict in Encoder.DictList) SingleDicts.Add(dict);
    }

    #region 词库

    public ObservableCollection<DictInfo> DictInfos { get; } = [];

    [ObservableProperty,
     NotifyCanExecuteChangedFor(nameof(RemoveDictCommand), nameof(SetTgtDictCommand)),
     NotifyPropertyChangedFor(nameof(SelDictInfoMod))]
    public partial DictInfo? SelDictInfo { get; set; }

    private bool HasSelDictInfo => SelDictInfo is {};
    public bool SelDictInfoMod => SelDictInfo is { Mod: true };
    private bool SelDictInfoNotTgt => SelDictInfo is { Tgt: false };

    public void AddDict(string path) {
        DictInfos.Add(DictManager.AddDict(path));
        if (DictInfos is [var dict]) dict.SetTgt(true);
    }

    [RelayCommand(CanExecute = nameof(HasSelDictInfo))]
    private async Task RemoveDict() {
        try {
            var dict = SelDictInfo!;
            if (dict.Mod && !await MsgBox.Ask<bool>("是否丢弃未保存的编辑？")) return;
            DictManager.RemoveDict(dict);
            if (!DictInfos.Remove(dict)) throw new FatalEx("严重错误：请停用并报告异常B");
        } catch (Exception ex) {
            Log.Err("移除词库", ex);
            await MsgBox.Err("移除词库", ex);
        }
    }

    [RelayCommand(CanExecute = nameof(SelDictInfoNotTgt))]
    private async Task SetTgtDict() {
        try {
            var dict = SelDictInfo!;
            var prevPath = DictManager.SetTgtDict(dict);
            dict.SetTgt(true);
            var prev = DictInfos.FirstOrDefault(x => x.Path == prevPath)
                    ?? throw new FatalEx("严重错误：请停用并报告异常C");
            prev.SetTgt(false);
        } catch (Exception ex) {
            Log.Err("设置加词目标", ex);
            await MsgBox.Err("设置加词目标", ex);
        }
    }

    #endregion 词库

    #region 单字和编码器

    public ObservableCollection<SingleDict> SingleDicts { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveSingleDictCommand))]
    public partial SingleDict? SelSingleDict { get; set; }

    private bool HasSelSingleDict => SelSingleDict is {};

    public static IReadOnlyList<EncodeMethod> EncodeMethods => EncodeMethod.All;
    [ObservableProperty] public partial EncodeMethod SelEncodeMethod { get; set; } = Encoder.Method;

    public void AddSingleDict(string path) => SingleDicts.Add(Encoder.AddDict(path));

    [RelayCommand(CanExecute = nameof(HasSelSingleDict))]
    private async Task RemoveSingleDict() {
        try {
            var dict = SelSingleDict!;
            Encoder.RemoveDict(dict);
            if (!SingleDicts.Remove(dict)) throw new FatalEx("严重错误：请停用并报告异常D");
        } catch (Exception ex) {
            Log.Err("移除单字码表", ex);
            await MsgBox.Err("移除单字码表", ex);
        }
    }

    #endregion 单字和编码器
}
