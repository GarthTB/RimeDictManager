namespace RimeDictManager.Services.Logging;

/// <summary> 只读日志接口 </summary>
internal interface ILogReader
{
    /// <returns> 只读日志内容 </returns>
    IReadOnlyList<string> Logs { get; }

    /// <summary>
    /// 将日志保存到文件。若文件存在，则覆盖原文件。
    /// </summary>
    Task SaveAsync(string path);
}
