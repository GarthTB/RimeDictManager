namespace RimeDictManager.Models;

using static Column;

/// <summary> 词库列 </summary>
public enum Column: byte { Text, Code, Weight, Stem }

public static class Columns {
    public const byte Cnt = 4;
    public static readonly Column[] Default = [Text, Code, Weight, Stem];
}
