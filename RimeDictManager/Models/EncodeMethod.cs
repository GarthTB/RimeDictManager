namespace RimeDictManager.Models;

public abstract class EncodeMethod {
    public abstract string Name { get; }
    public abstract byte MinLen { get; }
    public abstract byte MaxLen { get; }
    public abstract byte StemLen { get; }

    public static IReadOnlyList<EncodeMethod> All { get; }
        = [new Erbi(), new FlyPyTigerWubi(), new KeyTao()];

    protected abstract string Encode2(string a, string b);
    protected abstract string Encode3(string a, string b, string c);
    protected abstract string EncodeLong(string a, string b, string c, string d);

    public IEnumerable<string> Encode(string s, IReadOnlyDictionary<char, string[]> dict) {
        // 各有效字的编码
        var codes = new string[4][];
        byte cnt = 0;
        for (var i = 0; i < s.Length && cnt < 4; i++)
            if (dict.TryGetValue(s[i], out var v))
                codes[cnt++] = v;

        // ReSharper disable once InvertIf
        if (cnt == 4)
            for (var i = s.Length - 1; i > 3; i--)
                if (dict.TryGetValue(s[i], out var v)) {
                    codes[3] = v;
                    break;
                }

        return cnt switch {
            2 =>
                from a in codes[0]
                from b in codes[1]
                select Encode2(a, b),
            3 =>
                from a in codes[0]
                from b in codes[1]
                from c in codes[2]
                select Encode3(a, b, c),
            4 =>
                from a in codes[0]
                from b in codes[1]
                from c in codes[2]
                from d in codes[3]
                select EncodeLong(a, b, c, d),
            _ => []
        };
    }
}

file sealed class Erbi: EncodeMethod {
    public override string Name => "二笔 | 两笔";
    public override byte MinLen => 4;
    public override byte MaxLen => 4;
    public override byte StemLen => 2;
    protected override string Encode2(string a, string b) => $"{a[..2]}{b[..2]}";
    protected override string Encode3(string a, string b, string c) => $"{a[..2]}{b[0]}{c[0]}";

    protected override string EncodeLong(string a, string b, string c, string d) =>
        $"{a[0]}{b[0]}{c[0]}{d[0]}";
}

file sealed class FlyPyTigerWubi: EncodeMethod {
    public override string Name => "小鹤音形 | 虎码 | 五笔";
    public override byte MinLen => 4;
    public override byte MaxLen => 4;
    public override byte StemLen => 2;
    protected override string Encode2(string a, string b) => $"{a[..2]}{b[..2]}";
    protected override string Encode3(string a, string b, string c) => $"{a[0]}{b[0]}{c[..2]}";

    protected override string EncodeLong(string a, string b, string c, string d) =>
        $"{a[0]}{b[0]}{c[0]}{d[0]}";
}

file sealed class KeyTao: EncodeMethod {
    public override string Name => "星空键道";
    public override byte MinLen => 3;
    public override byte MaxLen => 6;
    public override byte StemLen => 3;
    protected override string Encode2(string a, string b) => $"{a[..2]}{b[..2]}{a[2]}{b[2]}";

    protected override string Encode3(string a, string b, string c) =>
        $"{a[0]}{b[0]}{c[0]}{a[2]}{b[2]}{c[2]}";

    protected override string EncodeLong(string a, string b, string c, string d) =>
        $"{a[0]}{b[0]}{c[0]}{d[0]}{a[2]}{b[2]}";
}
