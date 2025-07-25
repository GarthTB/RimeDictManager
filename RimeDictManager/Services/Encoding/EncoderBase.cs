namespace RimeDictManager.Services.Encoding;

/// <summary> 编码器基类 </summary>
internal abstract class EncoderBase : IEncoder
{
    /// <summary> 参与词组编码的单字编码片段（唯一） </summary>
    protected readonly Dictionary<char, string[]> CharCodes;

    public int CharCount => CharCodes.Count;

    protected EncoderBase(string dictPath)
        => CharCodes = LoadCharCodes(dictPath);

    /// <summary>
    /// 从Rime词库文件（.dict.yaml）加载用于词组编码的单字字典
    /// </summary>
    protected abstract Dictionary<char, string[]>
        LoadCharCodes(string dictPath);

    public abstract IEnumerable<string> Encode(string word);
}
