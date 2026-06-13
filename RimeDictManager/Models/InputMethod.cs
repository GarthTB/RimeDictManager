namespace RimeDictManager.Models;

using ZLinq;
using Str = string;

public abstract record InputMethod(Str Name, byte MinLen, byte MaxLen, byte StemLen) {
    public static IReadOnlyList<InputMethod> All { get; }
        = [new Erbi(), new FlyPyTigerWubi(), new KeyTao()];

    protected abstract Str Encode2(Str a, Str b);
    protected abstract Str Encode3(Str a, Str b, Str c);
    protected abstract Str EncodeLong(Str a, Str b, Str c, Str d);

    public Str[] Encode(Str s, IReadOnlyDictionary<char, Str[]> dict) {
        var codes = new Str[4][]; // 各有效字的编码
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
            2 => codes[0]
                .AsValueEnumerable()
                .SelectMany(_ => codes[1], Encode2)
                .Distinct()
                .ToArray(),
            3 => codes[0]
                .AsValueEnumerable()
                .SelectMany(_ => codes[1], static (a, b) => (a, b))
                .SelectMany(_ => codes[2], (t, c) => Encode3(t.a, t.b, c))
                .Distinct()
                .ToArray(),
            4 => codes[0]
                .AsValueEnumerable()
                .SelectMany(_ => codes[1], static (a, b) => (a, b))
                .SelectMany(_ => codes[2], static (t, c) => (t.a, t.b, c))
                .SelectMany(_ => codes[3], (t, d) => EncodeLong(t.a, t.b, t.c, d))
                .Distinct()
                .ToArray(),
            _ => []
        };
    }
}

file sealed record Erbi(): InputMethod("二笔 | 两笔", 4, 4, 2) {
    protected override Str Encode2(Str a, Str b) => $"{a[..2]}{b[..2]}";
    protected override Str Encode3(Str a, Str b, Str c) => $"{a[..2]}{b[0]}{c[0]}";
    protected override Str EncodeLong(Str a, Str b, Str c, Str d) => $"{a[0]}{b[0]}{c[0]}{d[0]}";
}

file sealed record FlyPyTigerWubi(): InputMethod("小鹤音形 | 虎码 | 五笔", 4, 4, 2) {
    protected override Str Encode2(Str a, Str b) => $"{a[..2]}{b[..2]}";
    protected override Str Encode3(Str a, Str b, Str c) => $"{a[0]}{b[0]}{c[..2]}";
    protected override Str EncodeLong(Str a, Str b, Str c, Str d) => $"{a[0]}{b[0]}{c[0]}{d[0]}";
}

file sealed record KeyTao(): InputMethod("星空键道", 3, 6, 3) {
    protected override Str Encode2(Str a, Str b) => $"{a[..2]}{b[..2]}{a[2]}{b[2]}";
    protected override Str Encode3(Str a, Str b, Str c) => $"{a[0]}{b[0]}{c[0]}{a[2]}{b[2]}{c[2]}";

    protected override Str EncodeLong(Str a, Str b, Str c, Str d) =>
        $"{a[0]}{b[0]}{c[0]}{d[0]}{a[2]}{b[2]}";
}
