namespace RimeDictManager.Services;

public static class Log {
    private static readonly List<string> Logs = new(1024);
    private static readonly DateTime StartTime = DateTime.Now;

    public static void Add(string msg, string? entry) {
        if (entry is {}) msg += $"\t{entry}";
        Logs.Add(msg);
    }

    public static string ReadAll() => string.Join('\n', Logs);

    public static async Task SaveAsync(Stream stream) {
        await using StreamWriter writer = new(stream);
        writer.NewLine = "\n";
        await writer.WriteLineAsync($"{StartTime:u} 日志开始");
        foreach (var l in Logs) await writer.WriteLineAsync(l);
        await writer.WriteLineAsync($"{DateTime.Now:u} 日志导出");
    }
}
