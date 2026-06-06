namespace RimeDictManager.Models;

/// <summary> 解析的词条行 </summary>
/// <param name="Number"> 行号：1开始，新词条为0 </param>
/// <param name="Text"> 文本（字词）：非空 </param>
/// <param name="Code"> 编码：省略时null </param>
/// <param name="Weight"> 权重：省略时null </param>
/// <param name="Stem"> 造词码：省略时null </param>
/// <remarks> https://github.com/LEOYoon-Tsaw/Rime_collections/blob/master/Rime_description.md </remarks>
public readonly record struct EntryLine(
    uint Number,
    string Text,
    string? Code,
    string? Weight,
    string? Stem);
