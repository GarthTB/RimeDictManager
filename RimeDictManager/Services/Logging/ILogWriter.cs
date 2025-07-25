namespace RimeDictManager.Services.Logging;

/// <summary> 只写日志接口 </summary>
internal interface ILogWriter
{
    /// <summary> 记录日志消息和涉及的词条 </summary>
    void Log(string message, Models.Entry? entry = null);
}
