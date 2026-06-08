namespace RimeDictManager.Models;

using System.Collections.Frozen;
using static Method;
using Codes = IEnumerable<string>;

public enum Method: byte {
    二笔,
    虎码,
    五笔,
    小鹤音形,
    星空键道6
}

public sealed class Encoder {
    private readonly FrozenDictionary<char, string[]> _singleDict;

    public Encoder(Method method, string singleDictPath) {
        var stemLen = method == 星空键道6
            ? 3 // 键道6单字前3码参与词组编码
            : 2; // 其他方案单字前2码参与词组编码
        _singleDict = File.ReadLines(singleDictPath)
            .Select(static l => l.Split('\t', 3, StringSplitOptions.TrimEntries))
            .Where(p => p is [[not '#'], var code, ..] && code.Length >= stemLen) // 是单字且码长够
            .GroupBy(static p => p[0][0], p => p[1][..stemLen])
            .ToFrozenDictionary(static g => g.Key, static g => g.Distinct().ToArray());
        CharCnt = (uint)_singleDict.Count;
        if (CharCnt == 0) throw new FormatException("单字码表为空");

        (MinLen, MaxLen, Encode) = method switch {
            二笔 => (4u, 4u, (Func<string, Codes>)Encode2B),
            虎码 or 五笔 => (4, 4, Encode5B),
            小鹤音形 => (3, 4, Encode5B),
            _ => (3, 6, EncodeJD6)
        };
    }

    public uint CharCnt { get; }
    public uint MinLen { get; }
    public uint MaxLen { get; }
    public Func<string, Codes> Encode { get; }

    private Codes Encode2B(string word) =>
        EncodeCore(
            word,
            static (a, b) => $"{a[..2]}{b[..2]}",
            static (a, b, c) => $"{a[..2]}{b[0]}{c[0]}",
            static (a, b, c, d) => $"{a[0]}{b[0]}{c[0]}{d[0]}");

    private Codes Encode5B(string word) =>
        EncodeCore(
            word,
            static (a, b) => $"{a[..2]}{b[..2]}",
            static (a, b, c) => $"{a[0]}{b[0]}{c[..2]}",
            static (a, b, c, d) => $"{a[0]}{b[0]}{c[0]}{d[0]}");

    private Codes EncodeJD6(string word) =>
        EncodeCore(
            word,
            static (a, b) => $"{a[..2]}{b[..2]}{a[2]}{b[2]}",
            static (a, b, c) => $"{a[0]}{b[0]}{c[0]}{a[2]}{b[2]}{c[2]}",
            static (a, b, c, d) => $"{a[0]}{b[0]}{c[0]}{d[0]}{a[2]}{b[2]}");

    /// <summary> 根据前3个和末个有效字，为词组编码 </summary>
    /// <param name="word"> 词组 </param>
    /// <param name="f2"> 2个有效字的编码逻辑 </param>
    /// <param name="f3"> 3个有效字的编码逻辑 </param>
    /// <param name="f4"> 4个有效字的编码逻辑 </param>
    /// <returns> 无重、无序的所有编码 </returns>
    private Codes EncodeCore(
        string word,
        Func<string, string, string> f2,
        Func<string, string, string, string> f3,
        Func<string, string, string, string, string> f4) {
        // 各有效字的编码
        var codes = new string[4][];
        byte cnt = 0;
        for (var i = 0; i < word.Length && cnt < 4; i++)
            if (_singleDict.TryGetValue(word[i], out var v))
                codes[cnt++] = v;

        // ReSharper disable once InvertIf
        if (cnt == 4)
            for (var i = word.Length - 1; i > 3; i--)
                if (_singleDict.TryGetValue(word[i], out var v)) {
                    codes[3] = v;
                    break;
                }

        return (cnt switch {
            2 =>
                from a in codes[0]
                from b in codes[1]
                select f2(a, b),
            3 =>
                from a in codes[0]
                from b in codes[1]
                from c in codes[2]
                select f3(a, b, c),
            4 =>
                from a in codes[0]
                from b in codes[1]
                from c in codes[2]
                from d in codes[3]
                select f4(a, b, c, d),
            _ => []
        }).Distinct();
    }
}
