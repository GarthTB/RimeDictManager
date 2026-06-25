namespace RimeDictManager;

using Common;
using ZLinq;

public static class UrlActivation {
    private static string? _dir;

    public static bool ParseArgs(string[] args) {
        var arg = args.AsValueEnumerable()
            .FirstOrDefault(static x =>
                x.AsSpan().StartsWith($"{Meta.UrlScheme}://", StringComparison.OrdinalIgnoreCase));
        if (arg is {}) ParseUrl(arg);
        return _dir is {};
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void ParseUrl(string url) {
        try {
            Uri uri = new(url);
            if (uri is not { Scheme: Meta.UrlScheme, Host: "open" }) return;
            foreach (var part in uri.Query.TrimStart('?').Split('&')) {
                if (part.Split('=', 2) is not ["dir", var dir]) continue;
                _dir = Uri.UnescapeDataString(dir);
                break;
            }
        } catch (Exception) { Log.Err("URL 解析失败，无法直达词库目录"); }
    }

    public static string? ConsumeDir() {
        var dir = _dir;
        _dir = null;
        return dir;
    }
}
