# RIME 词库管理器

![Windows x64](https://img.shields.io/badge/Windows-x64-0078D4)
![macOS arm64](https://img.shields.io/badge/macOS-arm64-000?logo=macos)
![Linux x64](https://img.shields.io/badge/Linux-x64-F4BC00?logo=linux)

[![Latest Release](https://img.shields.io/github/v/release/GarthTB/RimeDictManager?color=0FBF3E&label=Latest%20Release&logo=github)](https://github.com/GarthTB/RimeDictManager/releases/latest)
![Downloads](https://img.shields.io/github/downloads/GarthTB/RimeDictManager/total?color=0FBF3E&label=Downloads&logo=github)
[![MIT License](https://img.shields.io/badge/License-MIT-750014)](https://mit-license.org)

[![GitHub](https://img.shields.io/badge/GitHub-0FBF3E?logo=github)](https://github.com/GarthTB/RimeDictManager)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?logo=gitee)](https://gitee.com/tb0/RimeDictManager)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![C# 14](https://img.shields.io/badge/C%23-14-682A7A)](https://github.com/dotnet/csharplang)
[![AvaloniaUI 12.0](https://img.shields.io/badge/AvaloniaUI-12.0-3F8DF2?logo=avaloniaui)](https://avaloniaui.net)

一个轻量 GUI 应用，用于维护 [RIME 输入法](https://rime.im) 的词库。
利用严谨的格式约束和直观的表格界面，减少错误并提高效率。
可利用单字码表为新增词组自动编码。可导出 CRUD 操作日志。

## ✨ 特性

- 🔒 **安全**：遵循 [RIME 词库格式指南]，确保词库可用
- 🔄 **协同**：多文件协同 CRUD，全局搜索词条
- 🚀 **高效**：编码前缀搜索、字词精确搜索，百万词条迅速响应
- 🤖 **自动**：基于单字码表，按两笔、五笔等规则为词组编码
- 📝 **日志**：记录并导出所有 CRUD 操作及异常信息
- 🔗 **唤起**：支持 `rime-dict://` 协议冷启动，直达词库目录

## 📥 安装

[下载最新版本发布包](https://github.com/GarthTB/RimeDictManager/releases/latest)，根据平台执行对应步骤。

### Windows

下载 .7z 后解压即用。

### macOS

下载 .dmg 后双击挂载，将 `RimeDictManager.app` 拖拽到 `Applications` 文件夹即可。
首次打开需绕过系统限制（未签名应用）：

```bash
xattr -dr com.apple.quarantine /Applications/RimeDictManager.app
```

### Linux

下载 .tar.xz 后解压，进入目录执行安装脚本即可：

```bash
tar xJf RimeDictManager-*.tar.xz
cd RimeDictManager-*
./install.sh
```

## 💻 使用

在 `词库窗口` 中添加词库和单字码表，即可解锁各项功能。

### 词库窗口

上部为待维护的词库。只能为一个词库加词。
选中表格中的词库，可以移除/保存/设为加词目标。
有变更的词库显示 `●`，无变更则显示 `○`。无变更则不允许保存。

下部为单字码表。
仅供自动编码使用，一次性读取，不同步变更。
`编码方案` 仅影响自动编码，不干涉词库维护。

### 主窗口

- **添加词条**：输入 `文本` `权重` `造词码`，输入 `手动编码` 或选择 `自动编码`，点击按钮即可。
    - 新词条会自动出现在表格中，暂时按追加到词库末尾来显示行号
- **删除词条**：搜索词条，选中一项，点击按钮即可。
- **截短编码**：搜索词条，选中码长过长的一项，点击按钮即可。
    - 含义：将选中词条的编码截短为搜索框中的编码；若有词条占用该短码，则试图延长其编码。
    - 条件：启用不定长编码方案的自动编码功能、按 `编码前缀` 搜索。
- **应用修改**：在表格中直接修改后，点击按钮即可生效。

> - `文本` `权重` `手动编码` `造词码` 框的可用性按加词目标词库的定义
> - 有多项候选时，自动编码变红
> - 自动编码右边的手柄为不定长编码方案的码长
> - 不定长编码方案目前只有 `星空键道`

### URL Scheme

支持通过 `rime-dict://open?dir=<词库目录>` 协议冷启动，自动打开词库窗口并直达指定目录。

Windows 在每次启动时自动写入当前用户注册表。
macOS 和 Linux 在安装时自动注册。

## ☢️ 风险

本应用遵循 [RIME 词库格式指南]，但边界情况处理**不一致**。

对于某些与词条维护无关的细节，**不会全面检查**：

- **不会**：检查词库版本号的存在性
- **不会**：检查权重的合法性
- **不会**：检查词组省略编码的合法性

某些行为比 RIME 的行为更严格，**可能损坏原词库**：

- **会**：移除字段首尾的空白符
- **会**：移除所有行的尾随空白符
- **会**：词条列数超出定义时报错

## 📜 关于

- 地址：https://github.com/GarthTB/RimeDictManager
- 作者：Garth TB | 天卜 <g-art-h@outlook.com>
- 版权：Copyright (c) 2026 Garth TB | 天卜
- 声明：使用前请备份词库文件。作者不对因使用本应用而造成的任何数据损失负责。
- 历史：[CHANGELOG](https://github.com/GarthTB/RimeDictManager/blob/master/CHANGELOG.md)

[RIME 词库格式指南]: https://github.com/GarthTB/RimeDictManager/blob/master/RimeDictFormatGuide.md
