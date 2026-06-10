namespace RimeDictManager.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Services;
using Views;

public sealed partial class DictWindowVM: ObservableObject {
    public DictWindowVM() {
        foreach (var dict in DictManager.DictList) DictInfos.Add(new(dict));
        if (DictInfos.Count > 0) DictInfos[0].SetTgt(true);
    }

    public ObservableCollection<DictInfo> DictInfos { get; } = [];

    [ObservableProperty,
     NotifyCanExecuteChangedFor(nameof(RemoveCommand), nameof(SaveCommand), nameof(SetTgtCommand))]
    public partial DictInfo? SelDictInfo { get; set; }

    private bool Sel => SelDictInfo is {};
    private bool SelMod => SelDictInfo is { Mod: true };
    private bool SelNotTgt => SelDictInfo is { Tgt: false };

    public void Add(string path) {
        if (DictInfos.Any(d => d.Path == path)) throw new InvalidOperationException("词库重复");
        Dict dict = new(path);
        DictManager.AddDict(dict);
        DictInfos.Add(new(dict));
        if (DictInfos.Count == 1) DictInfos[0].SetTgt(true);
    }

    [RelayCommand(CanExecute = nameof(Sel))]
    private async Task Remove() {
        try {
            var sel = SelDictInfo!;
            if (sel.Mod && !await MsgBox.Ask("是否丢弃未保存的编辑？")) return;
            if (!DictManager.RemoveDict(sel.Src)) throw new InvalidOperationException("删除失败");
            if (!DictInfos.Remove(sel)) throw new UnreachableException("严重错误：请停用并报告异常B");
        } catch (Exception ex) { await MsgBox.Ex("移除词库", ex); }
    }

    [RelayCommand(CanExecute = nameof(SelMod))]
    private async Task Save() {
        try {
            var sel = SelDictInfo!;
            sel.NotifySaved();
            throw new NotImplementedException();
        } catch (Exception ex) { await MsgBox.Ex("保存词库", ex); }
    }

    [RelayCommand(CanExecute = nameof(SelNotTgt))]
    private async Task SetTgt() {
        try {
            var sel = SelDictInfo!;
            var prev = DictManager.SetTgtDict(sel.Src);
            sel.SetTgt(true);
            DictInfos.First(x => x.Src == prev).SetTgt(false);
        } catch (Exception ex) { await MsgBox.Ex("设置加词目标", ex); }
    }
}
