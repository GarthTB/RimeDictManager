namespace RimeDictManager.Services;

using System.IO;
using Models;

/// <summary> 日志：记录CRUD操作 </summary>
internal static class AuditLogger
{
    /// <summary> 日志内容 </summary>
    private static readonly List<string> Logs = [$"{DateTime.Now:yyMMdd-HHmmss} 开始记录日志"];

    /// <summary> 只读日志内容 </summary>
    public static IReadOnlyList<string> Entries => Logs;

    /// <summary> 记录日志 </summary>
    /// <param name="operation"> 操作内容 </param>
    /// <param name="line"> 相关词库行 </param>
    public static void Log(string operation, Line? line) =>
        Logs.Add(
            line is {}
                ? $"{operation}\t{line}"
                : operation);

    /// <summary> 保存日志到文件 </summary>
    /// <param name="path"> 保存路径 </param>
    public static void Save(string path) => File.WriteAllLines(path, Logs);
}
