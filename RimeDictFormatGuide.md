# RIME 词库格式指南

## 文件结构

一个完整的 RIME 词库文件由 **YAML 头 + TSV 码表** 组成：

```yaml
---
YAML 配置区
...
TSV 词条区
```

- `---`：YAML 开始标记
- `...`：YAML 结束标记，词条解析始于此
- **文件名**：`<词典名>.dict.yaml`
- **换行符**：建议 `\n`（UNIX/LF）
- **文件编码**：必须 UTF-8

## YAML 配置

| 必需字段      | 类型     | 说明                |
|-----------|--------|-------------------|
| `name`    | string | 词典内部标识名，需与文件名主体一致 |
| `version` | string | 版本号，修改词库后应更新      |

| 可选字段      | 类型   | 缺失时的默认值                | 说明           |
|-----------|------|------------------------|--------------|
| `columns` | list | `[text, code, weight]` | 词条区中各列的定义和顺序 |
| ...       | ...  | ...                    | ...          |

## `columns` 列定义

### `text`：文本

- 可以是单字或词组
- **是词条的核心，`columns` 数组和词条都不允许缺失此项**

### `code`：编码

内部允许以 ` `（空格）分隔

### `weight`：权重

缺失时默认为0。支持两种格式：

- **绝对值**：非负整数，如 `100` `243881`
- **百分比**：相对于预设权重的比例，如 `99%` `1%`

### `stem`：造词码

仅用于单字，自动造词据此提取字根

## TSV 词条

- 每行一条，字段（列）以 `\t`（制表符/Tab） 分隔
- `text` 不允许省略
- `code` 单字不允许省略，词组可在满足以下所有条件时省略：
    - 每个单字均有编码定义
    - 词组中不包含多音字，或多音字在该词组中存在权值超过 5% 的读音
- `weight` 和 `stem` 允许无条件省略

有效词条示例（列定义为 `[text, code, weight]`）：

```tsv
滚滚	gun gun	123
长江	chang jiang
东	dong	99%
逝水		456
```

---

## 隐式规则与边界行为

> 以下内容多未在官方文档中明确，只在 `librime` 源码中，不保证正确和稳定

### 空行处理

1. `trim_right()` 去掉行尾所有空白符（空格、Tab 等）
2. 跳过空串

| 内容             | 行为       |
|----------------|----------|
| （空行，无任何字符）     | ✅ 是空行，跳过 |
| `   `（仅空格）     | ✅ 是空行，跳过 |
| `\t\t`（仅 Tab）  | ✅ 是空行，跳过 |
| `  abc`（行首有空格） | ❌ 当作词条   |

### 注释行处理

只有第一个字符是 `#` 的才是注释行

| 内容          | 行为       |
|-------------|----------|
| `# 这是注释`    | ✅ 是注释，跳过 |
| `  # 前面有空格` | ❌ 当作词条   |
| `文本 # 行尾注释` | ❌ 当作词条   |

#### 特性：`# no comment`

若某行内容为 `# no comment`，其后所有行首为 `#` 的行都不再是注释，会被当作词条：

```yaml
# 这是注释
# no comment
# 这是词条
```

该行可以有尾随空格，不能有前导空格，`#` 和 `no` 之间一定要有空格

### 空格处理

保留字段首尾的空格（最后一个字段的末尾除外）

源码只对整行执行 `trim_right()`，每个字段不会单独 trim

示例（`·` 表示空格，列定义为 `[text, code, weight]`）：

```tsv
·文本·\t·编码·\t·权重·
```

解析到的值：

- `text` = `·文本·`
- `code` = `·编码·`
- `weight` = `·权重`

### 缺失文件头

| 状态                          | 行为                   |
|-----------------------------|----------------------|
| 无 `---` 无 `...`             | ❌ 整个文件被当作 YAML 解析    |
| 有 `---` 无 `...`             | ❌ 整个文件被当作 YAML 解析    |
| 有 `...` 无 `---`             | ❓ `...` 前被当作 YAML 解析 |
| 有起止标记但缺少 `name` 或 `version` | ❌ 报错并跳过整个词库文件        |

### 缺失 `text` 列

- **`columns` 中缺失**：报错并跳过整个词库文件
- **单个词条缺失**：警告并跳过该词条

### 词条字段数超出 `columns` 定义

静默忽略超出的字段

---

## 参考来源

### 官方文档

- [Rime 输入方案设计书（RimeWithSchemata）](https://github.com/rime/home/wiki/RimeWithSchemata)
- [Rime_description.md（雪齋）](https://github.com/LEOYoon-Tsaw/Rime_collections/blob/master/Rime_description.md)

### `librime` 源码

- [entry_collector.cc](https://github.com/rime/librime/blob/master/src/rime/dict/entry_collector.cc) - 词条收集与解析
- [dict_settings.cc](https://github.com/rime/librime/blob/master/src/rime/dict/dict_settings.cc) - 词库配置与列定义
- [strings.cc](https://github.com/rime/librime/blob/master/src/rime/algo/strings.cc) - 字符串分割实现
