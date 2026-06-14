namespace RimeDictManager.Models;

public interface IDictInfo {
    string Name { get; }
    string Path { get; }
    IReadOnlyList<Col> Cols { get; }
    uint Cnt { get; }
    bool Mod { get; }
}
