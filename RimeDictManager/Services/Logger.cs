namespace RimeDictManager.Services;

using System.IO;

/// <summary> 日志 </summary>
internal static class Logger
{
    /// <summary> 日志内容 </summary>
    private static readonly List<string> Logs = [$"{DateTime.Now:yyMMdd-HHmmss} 开始记录日志"];

    /// <summary> 只读日志内容 </summary>
    public static IReadOnlyList<string> Entries => Logs;

    /// <summary> 记录日志 </summary>
    /// <param name="msg"> 日志内容 </param>
    public static void Log(string msg) => Logs.Add(msg);

    /// <summary> 保存日志到文件 </summary>
    /// <param name="path"> 保存路径 </param>
    public static void Save(string path) => File.WriteAllLines(path, Logs);
}
