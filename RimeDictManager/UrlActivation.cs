namespace RimeDictManager;

using ZLinq;

public static class UrlActivation {
    private static string? _dir;

    public static void ParseArgs(string[] args) {
        var arg = args.AsValueEnumerable()
            .FirstOrDefault(static x =>
                x.AsSpan().StartsWith("rime-dict://", StringComparison.OrdinalIgnoreCase));
        if (arg is {}) ParseUrl(arg);
    }

    public static void ParseUrl(string url) {
        try {
            Uri uri = new(url);
            if (uri.Scheme != "rime-dict") return;
            var parts = uri.Query.TrimStart('?').Split('&');
            foreach (var part in parts) {
                if (part.Split('=', 2) is not ["dir", var dir]) continue;
                _dir = Uri.UnescapeDataString(dir);
                break;
            }
        } catch (UriFormatException) {}
    }

    public static string? ConsumeDir() {
        var dir = _dir;
        _dir = null;
        return dir;
    }
}
