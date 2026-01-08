namespace RimeDictManager.Encoders;

using Impl;

/// <summary> 编码器工厂 </summary>
internal static class EncoderFactory
{
    /// <summary> 编码器的构造函数 </summary>
    private static readonly Dictionary<string, Func<string, IEncoder>> Encoders = new() {
        ["二笔 | 两笔"] = static dictPath => new Erbi(dictPath),
        ["虎码"] = static dictPath => new Wubi(dictPath),
        ["五笔"] = static dictPath => new Wubi(dictPath),
        ["小鹤音形"] = static dictPath => new Wubi(dictPath),
        ["星空键道6"] = static dictPath => new Xkjd6(dictPath)
    };

    /// <summary> 可用编码方案名 </summary>
    public static IReadOnlyList<string> Names => Encoders.Keys.ToList();

    /// <summary> 构造指定方案的编码器 </summary>
    /// <param name="name"> 编码方案名 </param>
    /// <param name="dictPath"> 单字词库路径 </param>
    /// <returns> 构造好的编码器，待检验覆盖字数 </returns>
    public static IEncoder Create(string name, string dictPath) =>
        Encoders.TryGetValue(name, out var create)
            ? create(dictPath)
            : throw new KeyNotFoundException($"不支持\"{name}\"的编码器");
}
