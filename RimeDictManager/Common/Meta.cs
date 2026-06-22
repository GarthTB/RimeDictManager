namespace RimeDictManager.Common;

using System.Reflection;

public static class Meta {
    public const string Name = "RIME 词库管理器";

    public static string? Version { get; } = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;
}
