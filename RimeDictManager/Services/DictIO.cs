namespace RimeDictManager.Services;

using Models;

public static class DictIo {
    /// <summary> 保存词库（不迁移路径） </summary>
    /// <param name="dict"> 词库 </param>
    /// <param name="path"> 目标路径：null 则覆写 </param>
    /// <param name="reorder"> true：词条先按 Code 升序再按 Num 升序重排，非词条行按原序排在末尾；false：保持原有行，新词条按插入顺序排在末尾 </param>
    public static async Task SaveAsync(Dict dict, string? path, bool reorder) {
        await using StreamWriter writer = new(path ?? dict.Path);
        writer.NewLine = "\n";
        await writer.WriteLineAsync(dict.Yaml);

        if (reorder) {
            foreach (var e in dict.Entries.OrderBy(static e => (e.Code, e.Num)))
                await writer.WriteLineAsync(e.Format(dict.Cols));
            foreach (var r in dict.RawLines) await writer.WriteLineAsync(r.Content);
        } else {
            using var entries = dict.Entries.GetEnumerator();
            using var rawLines = dict.RawLines.GetEnumerator();
            var anyE = entries.MoveNext();
            var anyR = rawLines.MoveNext();
            while (anyE || anyR)
                if (anyE && (!anyR || entries.Current.Num <= rawLines.Current.Num)) {
                    await writer.WriteLineAsync(entries.Current.Format(dict.Cols));
                    anyE = entries.MoveNext();
                } else {
                    await writer.WriteLineAsync(rawLines.Current.Content);
                    anyR = rawLines.MoveNext();
                }
        }

        dict.NotifySaved();
    }
}
