namespace RimeDictManager.Models;

using System.Diagnostics;
using Str = string;

/// <summary> 解析的词条行 </summary>
/// <param name="Num"> 行号：1开始，0无效 </param>
/// <param name="Text"> 文本：非空 </param>
/// <param name="Code"> 编码：条件可空 </param>
/// <param name="Weight"> 权重：无条件可空 </param>
/// <param name="Stem"> 造词码：无条件可空 </param>
public readonly record struct EntryLine(uint Num, Str Text, Str Code, Str Weight, Str Stem) {
    private Str this[DictCol col] =>
        col switch {
            DictCol.Text => Text,
            DictCol.Code => Code,
            DictCol.Weight => Weight,
            DictCol.Stem => Stem,
            _ => throw new UnreachableException()
        };

    public Str Format(IReadOnlyList<DictCol> cols) {
        var cnt = cols.Count;
        while (this[cols[cnt - 1]].Length == 0) cnt--;

        var vals = new Str[cnt];
        for (var i = 0; i < cnt; i++) vals[i] = this[cols[i]];
        return Str.Join('\t', vals);
    }

    public static bool TryNew(
        uint num,
        Str text,
        Str code,
        Str weight,
        Str stem,
        IReadOnlyList<DictCol> cols,
        out EntryLine e) {
        if (Str.IsNullOrWhiteSpace(text)) goto Fail;

        var mask = cols.Aggregate(0, static (x, c) => x | (1 << (int)c));
        if ((Str.IsNullOrWhiteSpace(code) || (mask & (1 << (int)DictCol.Code)) != 0)
         && (Str.IsNullOrWhiteSpace(weight) || (mask & (1 << (int)DictCol.Weight)) != 0)
         && (Str.IsNullOrWhiteSpace(stem) || (mask & (1 << (int)DictCol.Stem)) != 0)) {
            text = text.Trim();
            code = code.Trim();
            if (text.Length != 1 || code.Length > 0) {
                e = new(num, text, code, weight.Trim(), stem.Trim());
                return true;
            }
        }

    Fail:
        e = default;
        return false;
    }
}

/// <summary> 除词条外的行 </summary>
/// <param name="Num"> 行号：1开始 </param>
/// <param name="Content"> 原始内容 </param>
public readonly record struct RawLine(uint Num, Str Content);

/// <summary> 带有词库归属的完整词条信息 </summary>
public readonly record struct DictEntry(IDictInfo Dict, EntryLine Entry);
