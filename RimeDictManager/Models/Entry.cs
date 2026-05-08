namespace RimeDictManager.Models;

/// <summary> 解析的词条行 </summary>
/// <param name="Num"> 行号：1开始，新词条为0 </param>
/// <param name="Text"> 文本（字词）：非空 </param>
/// <param name="Code"> 编码：省略时null </param>
/// <param name="Weight"> 权重（越大越优先）：省略时null，'非负整数'或'浮点数%' </param>
/// <param name="Stem"> 造词码（单字编码中参与词组编码的部分）：省略时null </param>
/// <remarks> https://github.com/LEOYoon-Tsaw/Rime_collections/blob/master/Rime_description.md </remarks>
internal readonly record struct Entry(
    uint Num,
    string Text,
    string? Code,
    string? Weight,
    string? Stem);
