namespace RimeDictManager.Models;

public interface IDictInfo {
    string Path { get; }
    string Name { get; }
    IReadOnlyList<DictCol> Cols { get; }
    uint Cnt { get; }
    bool Modified { get; }
}
