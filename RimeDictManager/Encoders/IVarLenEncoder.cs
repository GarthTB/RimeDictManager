namespace RimeDictManager.Encoders;

/// <summary> 变长编码器 </summary>
internal interface IVarLenEncoder: IEncoder
{
    /// <summary> 码长范围 </summary>
    (byte Min, byte Max) LenRange { get; }

    /// <summary> 根据全码获取指定长度的简码 </summary>
    /// <param name="fullCodes"> 全码 </param>
    /// <param name="len"> 简码长度 </param>
    /// <returns> 无重、无序的简码 </returns>
    IEnumerable<string> Shorten(IEnumerable<string> fullCodes, byte len);
}
