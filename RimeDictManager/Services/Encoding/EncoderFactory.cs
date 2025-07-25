using RimeDictManager.Services.Encoding.Encoders;

namespace RimeDictManager.Services.Encoding;

/// <summary> 编码器工厂 </summary>
internal static class EncoderFactory
{
    /// <summary> 所有编码器的构造函数 </summary>
    private static readonly
        Dictionary<string, Func<string, IEncoder>> _encoders = new()
        {
            ["二笔 | 两笔"] = (dictPath) => new Erbi(dictPath),
            ["虎码"] = (dictPath) => new Wubi(dictPath),
            ["86五笔"] = (dictPath) => new Wubi(dictPath),
            ["小鹤音形"] = (dictPath) => new Wubi(dictPath),
            ["星空键道6"] = (dictPath) => new Xkjd6(dictPath)
        };

    /// <returns> 所有可用的编码器名称 </returns>
    public static IEnumerable<string> EncoderNames => _encoders.Keys;

    /// <summary> 创建指定名称的编码器 </summary>
    public static IEncoder CreateEncoder(string name, string dictPath)
        => _encoders.TryGetValue(name, out var creator)
        ? creator(dictPath)
        : throw new KeyNotFoundException($"{name} 编码器不存在！");
}
