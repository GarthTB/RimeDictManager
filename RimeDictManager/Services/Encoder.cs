namespace RimeDictManager.Services;

using System.Collections.Frozen;
using System.IO;
using EncoderInfo
    = (byte MinCodeLen, byte MaxCodeLen, Func<string, IEnumerable<string>> Encode, uint CharsCount);

internal static class Encoder {
    public static IReadOnlyList<string> Methods => ["二笔 | 两笔", "虎码", "五笔", "小鹤音形", "星空键道6"];

    public static EncoderInfo Create(string method, string charsDictPath) {
        var stemLen = method == "星空键道6"
            ? 3 // 键道6单字前3码参与词组编码
            : 2; // 其他方案单字前2码参与词组编码
        var charsDict = File.ReadLines(charsDictPath)
            .Select(static l => l.Split('\t', 3, StringSplitOptions.TrimEntries))
            .Where(p => p is [[not '#'], var code, ..] && code.Length >= stemLen) // 是单字且码长够
            .GroupBy(static p => p[0][0], static p => p[1])
            .ToFrozenDictionary(static g => g.Key, static g => g.Distinct().ToArray());

        return method switch {
            "二笔 | 两笔" => (4, 4, Encode2B, (uint)charsDict.Count),
            "虎码" or "五笔" => (4, 4, Encode5B, (uint)charsDict.Count),
            "小鹤音形" => (3, 4, Encode5B, (uint)charsDict.Count),
            _ => (3, 6, EncodeJD6, (uint)charsDict.Count)
        };

        byte CodesOfF3L1Chars(string word, out string[][] codes) {
            codes = new string[4][];

            byte cnt = 0;
            for (var i = 0; i < word.Length && cnt < 4; i++)
                if (charsDict.TryGetValue(word[i], out var charCodes))
                    codes[cnt++] = charCodes;
            if (cnt < 4) return cnt;

            for (var (i, found) = (word.Length - 1, false); i > 3 && !found; i--)
                if (charsDict.TryGetValue(word[i], out var charCodes))
                    (codes[3], found) = (charCodes, true);
            return 4;
        }

        IEnumerable<string> Encode2B(string word) =>
            (CodesOfF3L1Chars(word, out var codes) switch {
                2 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    select $"{s1[..2]}{s2[..2]}",
                3 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    select $"{s1[..2]}{s2[0]}{s3[0]}",
                4 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    from s4 in codes[3]
                    select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}",
                _ => []
            }).Distinct();

        IEnumerable<string> Encode5B(string word) =>
            (CodesOfF3L1Chars(word, out var codes) switch {
                2 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    select $"{s1[..2]}{s2[..2]}",
                3 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    select $"{s1[0]}{s2[0]}{s3[..2]}",
                4 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    from s4 in codes[3]
                    select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}",
                _ => []
            }).Distinct();

        IEnumerable<string> EncodeJD6(string word) =>
            (CodesOfF3L1Chars(word, out var codes) switch {
                2 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    select $"{s1[..2]}{s2[..2]}{s1[2]}{s2[2]}",
                3 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    select $"{s1[0]}{s2[0]}{s3[0]}{s1[2]}{s2[2]}{s3[2]}",
                4 =>
                    from s1 in codes[0]
                    from s2 in codes[1]
                    from s3 in codes[2]
                    from s4 in codes[3]
                    select $"{s1[0]}{s2[0]}{s3[0]}{s4[0]}{s1[2]}{s2[2]}",
                _ => []
            }).Distinct();
    }
}
