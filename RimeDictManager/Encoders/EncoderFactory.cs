namespace RimeDictManager.Encoders;

using Impl;

/// <summary> 编码器工厂 </summary>
internal static class EncoderFactory
{
    /// <summary> 编码器的构造函数 </summary>
    private static readonly Dictionary<string, Func<string, IEncoder>> Encoders = new() {
        ["二笔 | 两笔"] = static path => new Erbi(path),
        ["虎码"] = static path => new Wubi(path),
        ["五笔"] = static path => new Wubi(path),
        ["小鹤音形"] = static path => new Wubi(path),
        ["星空键道6"] = static path => new Xkjd6(path)
    };

    /// <summary> 可用编码方案名 </summary>
    public static IReadOnlyCollection<string> Names => Encoders.Keys;

    /// <summary> 构造指定方案的编码器 </summary>
    /// <param name="name"> 编码方案名 </param>
    /// <param name="dictPath"> 单字词库路径 </param>
    /// <returns> 构造好的编码器，待检验覆盖字数 </returns>
    public static IEncoder Create(string name, string dictPath) => Encoders[name](dictPath);
}
