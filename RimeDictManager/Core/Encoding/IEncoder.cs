namespace RimeDictManager.Core.Encoding;

/// <summary> 编码器 </summary>
internal interface IEncoder
{
    /// <summary> 覆盖字数 </summary>
    uint Chars { get; }

    /// <summary> 给词组编码 </summary>
    /// <param name="word"> 待编码的词组 </param>
    /// <returns> 无重、无序的编码 </returns>
    IEnumerable<string> Encode(string word);
}
