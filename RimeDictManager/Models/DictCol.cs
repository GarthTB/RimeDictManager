namespace RimeDictManager.Models;

using static DictCol;

/// <summary> 词库列 </summary>
public enum DictCol: byte { Text, Code, Weight, Stem }

public static class DictCols {
    public const byte EnumCnt = 4;
    public static readonly DictCol[] Default = [Text, Code, Weight];
}
