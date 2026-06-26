namespace RimeDictManager.Models;

using static DictCol;

public enum DictCol: byte { Text, Code, Weight, Stem }

public static class DictCols {
    public static readonly DictCol[] Default = [Text, Code, Weight];
}
