namespace RimeDictManager.Services;

using System.IO;
using Models;
using static DateTime;

internal static class Logger {
    private static readonly List<string> Logs = new(64) { $"{Now:u} 日志开始" };
    public static IReadOnlyList<string> LogView => Logs;

    /// <summary> 添加日志 </summary>
    /// <param name="msg"> 信息 </param>
    /// <param name="line"> 涉及的词库行 </param>
    public static void Log(string msg, Line? line) =>
        Logs.Add(
            line is {}
                ? $"{msg}\t{line}"
                : msg);

    public static void Save(string path) => File.WriteAllLines(path, Logs.Append($"{Now:u} 日志保存"));
}
