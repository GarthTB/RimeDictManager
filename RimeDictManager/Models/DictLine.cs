namespace RimeDictManager.Models;

using System.Diagnostics;
using Str = string;

/// <summary> 解析的词条行 </summary>
/// <param name="Num"> 行号：1开始，0无效 </param>
/// <param name="Text"> 文本（字词）：非空 </param>
/// <param name="Code"> 编码：空即省略 </param>
/// <param name="Weight"> 权重：空即省略 </param>
/// <param name="Stem"> 造词码：空即省略 </param>
public readonly record struct EntryLine(uint Num, Str Text, Str Code, Str Weight, Str Stem) {
    public Str this[DictCol col] =>
        col switch {
            DictCol.Text => Text,
            DictCol.Code => Code,
            DictCol.Weight => Weight,
            DictCol.Stem => Stem,
            _ => throw new UnreachableException()
        };
}

/// <summary> 除词条外的行 </summary>
/// <param name="Num"> 行号：1开始 </param>
/// <param name="Content"> 原始内容 </param>
public readonly record struct RawLine(uint Num, Str Content);

/// <summary> 完整词条信息 </summary>
/// <param name="Dict"> 词库 </param>
/// <param name="Entry"> 词条行 </param>
public readonly record struct DictEntry(IDictInfo Dict, EntryLine Entry);
