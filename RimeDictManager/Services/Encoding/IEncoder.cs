namespace RimeDictManager.Services.Encoding;

/// <summary> 基础编码器接口 </summary>
internal interface IEncoder
{
    /// <returns> 编码器覆盖的单字数量 </returns>
    int CharCount { get; }

    /// <returns> 词组的所有编码（唯一，无序） </returns>
    IEnumerable<string> Encode(string word);
}
