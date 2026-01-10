namespace RimeDictManager.Encoders;

/// <summary> 编码器 </summary>
internal interface IEncoder
{
    /// <summary> 覆盖字数和码长范围 </summary>
    (uint CharCnt, byte MaxLen, byte MinLen) Spec { get; }

    /// <summary> 给词组编码 </summary>
    /// <param name="word"> 待编码的词组 </param>
    /// <returns> 无重、无序的编码 </returns>
    IEnumerable<string> Encode(string word);
}
