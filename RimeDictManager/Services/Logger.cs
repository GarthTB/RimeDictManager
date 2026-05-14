namespace RimeDictManager.Services;

using System.IO;
using Models;

internal static class Logger {
    private static readonly List<string> Logs = new(1024) { $"{DateTime.Now:u} 日志开始" };
    public static uint Count { get; private set; }
    public static string Dump() => string.Join('\n', Logs);

    public static void Log(string msg, Entry? e) {
        if (e is {} v) msg += $"\t{LineCodec.Serialize(v)}";
        Logs.Add(msg);
        Count++;
    }

    public static void Save(string path) {
        using StreamWriter writer = new(path, true);
        writer.NewLine = "\n";
        Logs.Add($"{DateTime.Now:u} 日志导出");
        foreach (var l in Logs) writer.WriteLine(l);
    }
}
