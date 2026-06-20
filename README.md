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
- 🔗 **唤起**：支持 `rime-dict://` 冷启动，快速定位词库

## 📥 使用

[下载特定平台的最新版本发布包](https://github.com/GarthTB/RimeDictManager/releases/latest)，解压即用。

在 `词库窗口` 中添加词库和单字码表，方能解锁各项功能。

### 词库窗口

上部为待维护的词库。
在表格中选中词库，可以移除/保存/设为加词目标，只能为一个词库加词。
有变更的词库显示 `●`，无变更则显示 `○`。无变更不允许保存。

下部为单字码表。
单字码表仅供自动编码使用，一次性加载，不同步变更。
`编码方案` 仅影响自动编码，不干涉词库维护。

### 主窗口

**添加词条**：输入 `文本` `权重` `造词码`，输入 `手动编码` 或选择 `自动编码`，点击按钮即可。
新词条会出现在表格中，行号显示为 `0`。

**删除词条**：搜索词条，选中一项，点击按钮即可。

**截短编码**：搜索词条，选中码长过长的一项，点击按钮即可。
含义：将选中词条的编码截短为搜索框中的编码；若有词条占用该短码，则试图延长其编码。
条件：启用不定长编码方案的自动编码功能、按 `编码前缀` 搜索。

**应用修改**：在表格中直接修改后，点击按钮即可生效。

- `文本` `权重` `手动编码` `造词码` 框的可用性依赖于加词目标词库的定义
- 有多项候选时，自动编码变红
- 自动编码右边的手柄为不定长编码方案的码长
- 不定长编码方案目前只有 `星空键道`

### URL Scheme

支持通过 `rime-dict://open?dir=<词库目录>` 协议冷启动，自动打开词库窗口并直达指定目录。

**注册方式**：

- **Windows**：每次启动自动写入当前用户注册表，无需额外配置
- **macOS**：应用包 `Info.plist` 已声明协议，开箱即用
- **Linux**：按以下步骤手动安装 `.desktop` 文件

```bash
# 1. 可执行文件放入 PATH
mkdir -p ~/.local/bin
cp RimeDictManager ~/.local/bin/

# 2. 安装图标
mkdir -p ~/.local/share/icons/hicolor/48x48/apps
cp Assets/icon_48.png ~/.local/share/icons/hicolor/48x48/apps/rimedictmanager.png
mkdir -p ~/.local/share/icons/hicolor/256x256/apps
cp Assets/icon_256.png ~/.local/share/icons/hicolor/256x256/apps/rimedictmanager.png

# 3. 安装 .desktop 文件
mkdir -p ~/.local/share/applications
cp Assets/Linux/rimedictmanager.desktop ~/.local/share/applications/

# 4. 更新桌面与 MIME 数据库
update-desktop-database ~/.local/share/applications
update-mime-database ~/.local/share/mime
```

## 📜 关于

- 地址：https://github.com/GarthTB/RimeDictManager
- 作者：Garth TB | 天卜 <g-art-h@outlook.com>
- 版权：Copyright (c) 2026 Garth TB | 天卜
- 声明：本项目基于作者自用需求，不确保完备。使用前请备份词库文件。作者不对因使用本程序而造成的任何数据损失负责。
- 历史：[CHANGELOG](https://github.com/GarthTB/RimeDictManager/blob/master/CHANGELOG.md)
