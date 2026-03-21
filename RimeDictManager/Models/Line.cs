namespace RimeDictManager.Models;

/// <summary> 词库行 </summary>
/// <param name="Num"> 行号：1开头 </param>
/// <param name="Raw"> 原始内容 </param>
internal record Line(uint Num, string? Raw) {
    public override string? ToString() => Raw;
}
