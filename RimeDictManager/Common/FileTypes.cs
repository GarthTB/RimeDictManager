namespace RimeDictManager.Common;

using FileType = Avalonia.Platform.Storage.FilePickerFileType;

public static class FileTypes {
    public static readonly FileType Log = new("日志") {
        Patterns = ["*.log"],
        MimeTypes = ["text/plain"],
        AppleUniformTypeIdentifiers = ["public.plain-text"]
    };

    public static readonly FileType RimeDict = new("RIME 词库") {
        Patterns = ["*.dict.yaml"],
        MimeTypes = ["application/yaml"],
        AppleUniformTypeIdentifiers = ["public.yaml"]
    };
}
