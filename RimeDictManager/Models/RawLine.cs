namespace RimeDictManager.Models;

/// <summary> 除词条外的行 </summary>
/// <param name="Number"> 行号：1开始 </param>
/// <param name="Content"> 空行为null，注释行首为# </param>
/// <remarks> https://github.com/rime/home/wiki/RimeWithSchemata </remarks>
public readonly record struct RawLine(uint Number, string? Content);
