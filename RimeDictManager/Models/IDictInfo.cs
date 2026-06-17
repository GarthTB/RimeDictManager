namespace RimeDictManager.Models;

public interface IDictInfo {
    string Name { get; }
    string Path { get; }
    IReadOnlyList<Column> Cols { get; }
    uint Cnt { get; }
    bool Modified { get; }
}
