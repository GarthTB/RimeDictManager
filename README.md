# RIME 词库管理器

[![Latest Release](https://img.shields.io/github/v/release/GarthTB/RimeDictManager?color=0FBF3E&label=Latest%20Release&logo=github)](https://github.com/GarthTB/RimeDictManager/releases/latest)
[![Source GitHub](https://img.shields.io/badge/Source-GitHub-0FBF3E?logo=github)](https://github.com/GarthTB/RimeDictManager)
[![Source Gitee](https://img.shields.io/badge/Source-Gitee-C71D23?logo=gitee)](https://gitee.com/tb0/RimeDictManager)

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![AvaloniaUI 12.0](https://img.shields.io/badge/AvaloniaUI-12.0-3F8DF2?logo=avaloniaui)](https://avaloniaui.net)
[![C# 14.0](https://img.shields.io/badge/C%23-14.0-682A7A)](https://github.com/dotnet/csharplang)
[![License MIT](https://img.shields.io/badge/License-MIT-750014)](https://mit-license.org)

一个轻量级 GUI 应用程序，用于维护 [RIME 输入法](https://rime.im) 的词库。
旨在利用严格的格式约束和直观的表格界面，减少错误并提高效率。
提供基于单字码表的词组自动编码功能，提供可导出的操作日志。

## ✨ 特性

- 🔒 **安全**：严格遵循 [RIME 官方词库格式规范](https://github.com/rime/home/wiki/RimeWithSchemata)
- 🔄 **协同**：多文件协同 CRUD，全局搜索词条
- 🚀 **高效**：编码前缀搜索、字词精确搜索，百万词条迅速响应
- 🤖 **自动**：基于单字码表，按两笔、五笔等规则为词组编码
- 📝 **日志**：记录所有 CRUD 操作及异常信息，支持导出

## 📥 使用

[下载特定平台的最新版本发布包](https://github.com/GarthTB/RimeDictManager/releases/latest)，解压即用。

需在 `词库窗口` 添加词库和单字码表，方能解锁各项功能。

### 词库窗口

上部为待维护的词库。在表格中选中词库，可以移除/保存/设为加词目标。加词目标只能为其中之一。有变更的词库显示 `●`，无变更则显示
`○`。无变更不允许保存。

下部为单字码表。仅供自动编码使用，一次性加载，不同步变更。`编码方案` 仅影响编码生成逻辑，不干涉词库维护工作。

### 主窗口

添加词条时，输入 `文本` `权重` `造词码`，输入 `手动编码` 或选择 `自动编码`，点击 `添加词条` 即可，完成后会出现在表格中。

删除词条时，搜索词条，选中一项，点击 `删除词条` 即可。

截短编码：将选中词条的编码截短为搜索框中的编码；若有词条占用该短码，则试图延长其编码。仅用于不定长编码方案（目前只有 `星空键道`
方案），需要启用自动编码，搜索模式需为 `编码前缀`。

应用修改：在表格中直接修改后，点击 `应用修改` 即可生效。

- `文本` `权重` `手动编码` `造词码` 框的可用性依赖于加词目标词库的定义
- 自动编码变红，代表有多项候选
- 自动编码右边的手柄为不定长编码方案的码长

## ℹ 关于

- 地址：https://github.com/GarthTB/RimeDictManager
- 作者：Garth TB | 天卜 <g-art-h@outlook.com>
- 版权：Copyright (c) 2026 Garth TB | 天卜
- 声明：本项目基于作者自用需求，不确保完备。使用前请备份词库文件。作者不对因使用本程序而造成的任何数据损失负责。

## 📝 版本

### v4.0.0 (20260618)

- 改用 AvaloniaUI，大幅调整界面，支持多种桌面平台
- 支持多词库协同维护

### v3.* (20260518 - 20260617)

- 重构核心，恢复支持 `造词码` 字段
- 支持词库文件头中的不同列定义
- 支持省略除 `文本` 外的所有字段
- 大幅调整界面布局

### v2.* (20260110 - 20260517)

- 取消支持 `造词码` 字段
- 支持省略 `编码` 字段
- 不再强制无重复词条
- 新增：保存排序策略

### v1.* (20250720 - 20260109)

- 使用 .NET 10.0 WPF，专注 Windows 单平台
- 使用 MVVM 架构，提升性能与可维护性
- 支持处理词库文件头
- 完善对其他输入法编码方案的支持

### 🕰️ 前身（废弃）

- **[跨平台 RIME 词库管理器](https://github.com/GarthTB/RimeTyrant)** (20240910 - 20240914)
    - 使用 MAUI，支持 Windows 和 Android
    - 初步尝试支持其他输入法编码方案
- **[词器清单版](https://github.com/GarthTB/RimeLibrarian)** (20240622 - 20241110)
    - 重构界面，提供表格以简化操作
- **[词器v2](https://github.com/GarthTB/JDLibManager)** (20240605 - 20240620)
    - 改用 .NET 6.0 WPF
- **[词器](https://github.com/GarthTB/CiQi)** (20230513 - 20231223)
    - 使用 WinForms，专用于 `星空键道6` 输入法方案
