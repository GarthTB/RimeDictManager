namespace RimeDictManager.Models;

/// <summary> 词库行 </summary>
/// <param name="Num"> 行号：1开头，新词条为0 </param>
/// <param name="Raw"> 内容：词条为null </param>
internal record Line(uint Num, string? Raw) {
    public override string? ToString() => Raw;
}
