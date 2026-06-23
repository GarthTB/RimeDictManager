namespace RimeDictManager.Common;

using System.Reflection;

public static class Meta {
    public const string Name = "RIME 词库管理器";

    /// <summary> 协议：rime-dict://open?dir=词库目录 </summary>
    public const string UrlScheme = "rime-dict";

    public static string Version { get; } = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
}
