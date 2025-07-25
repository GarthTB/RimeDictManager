namespace RimeDictManager.Services.Encoding;

/// <summary> 变长编码器接口 </summary>
internal interface IVarLenEncoder : IEncoder
{
    /// <returns> 有效码长范围 </returns>
    (byte, byte) CodeLengthRange { get; }

    /// <returns>
    /// 全码对应的指定长度的所有短编码（唯一，无序）
    /// </returns>
    IEnumerable<string> ShortenCodes(
        IEnumerable<string> fullCodes, int length);
}
