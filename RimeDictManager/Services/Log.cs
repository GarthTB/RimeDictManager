namespace RimeDictManager.Services;

public static class Log {
    private static readonly DateTime StartTime = DateTime.Now;
    private static readonly List<string> Logs = new(1024);
    public static IReadOnlyList<string> All => Logs;

    public static void Info(string msg) => Logs.Add(msg);

    /// <summary> DictManager专用 </summary>
    public static void Crud(string op, string entry) => Logs.Add($"{op}\t{entry}");

    /// <summary> MsgBox专用 </summary>
    public static void Err(Exception ex, string op) => Logs.Add($"{op}时：\n{ex}");

    public static async Task SaveAsync(Stream stream) {
        await using StreamWriter writer = new(stream);
        writer.NewLine = "\n";
        await writer.WriteLineAsync($"{StartTime:u} 日志开始");
        foreach (var l in Logs) await writer.WriteLineAsync(l);
        await writer.WriteLineAsync($"{DateTime.Now:u} 日志导出");
    }
}
