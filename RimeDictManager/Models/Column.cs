namespace RimeDictManager.Models;

using Serde;

/// <summary> 词库列：缺省集合见 <see cref="DictParser.DefaultCols"/> </summary>
/// <seealso href="https://github.com/LEOYoon-Tsaw/Rime_collections/blob/master/Rime_description.md"/>
public enum Column: byte { Text, Code, Weight, Stem }
