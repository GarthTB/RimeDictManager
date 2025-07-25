using RimeDictManager.Models;
using System.IO;

namespace RimeDictManager.Services.Logging;

/// <summary> 日志服务（静态单例） </summary>
internal sealed class Logger : ILogReader, ILogWriter
{
    /// <summary> 日志内容列表 </summary>
    private readonly List<string> _logs =
        [$"{DateTime.Now:yyMMdd-HHmmss} 开始记录日志"];

    /// <summary> 静态单例 </summary>
    private static readonly Logger _instance = new();

    /// <returns> 只写静态单例 </returns>
    public static ILogWriter Writer => _instance;

    /// <returns> 只读静态单例 </returns>
    public static ILogReader Reader => _instance;

    private Logger() { } // 防止外部实例化

    public IReadOnlyList<string> Logs => _logs.AsReadOnly();

    public void Log(string message, Entry? entry = null)
        => _logs.Add(entry is null ? message : $"{message}\t{entry}");

    public async Task SaveAsync(string path)
        => await File.WriteAllLinesAsync(path, _logs);
}
