namespace RimeDictManager.Models;

using Str = string;

/// <summary> 解析的词条行 </summary>
/// <param name="Num"> 行号：1开始，0无效 </param>
/// <param name="Text"> 文本（字词）：非空 </param>
/// <param name="Code"> 编码：省略时null </param>
/// <param name="Weight"> 权重：省略时null </param>
/// <param name="Stem"> 造词码：省略时null </param>
/// <seealso cref="Column"/>
public readonly record struct EntryLine(uint Num, Str Text, Str? Code, Str? Weight, Str? Stem);

/// <summary> 除词条外的行 </summary>
/// <param name="Num"> 行号：1开始 </param>
/// <param name="Content"> 空行为null，注释行首为# </param>
/// <seealso href="https://github.com/rime/home/wiki/RimeWithSchemata"/>
public readonly record struct RawLine(uint Num, Str? Content);

/// <summary> 完整词条信息 </summary>
/// <param name="Dict"> 词库 </param>
/// <param name="Entry"> 词条行 </param>
public readonly record struct DictEntry(IDictInfo Dict, EntryLine Entry);
