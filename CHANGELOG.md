# 更新日志

本文基于 [Keep a Changelog]。项目遵循 [语义化版本]。

## [开发中]

## [4.2.0] - 2026-07-01

### Added

- 加载整个词库目录的功能
- **RIME 词库格式指南**：作为 RIME 的注解和本应用的守则

### Changed

- 禁止单字缺失编码
- 会移除所有行尾的空白符
- **缺省列定义**：按 RIME 规则改为 `[text, code, weight]`
- **注释识别**：按 RIME 规则，详见格式指南的 `注释行处理` 部分
- **空码判据**：所有词库中仅有一条该编码，且存在任一以其为前缀的更长编码
- **安装**：macOS 打包为 `.dmg`，Linux 提供安装脚本
- **UI**：码长控件改为带箭头的文本框

### Fixed

- 手动修改词条后，以非重排模式保存时顺序错乱的问题
- 新词条行号比实际大 1 的问题

## [4.1.1] - 2026-06-21

- **Fixed**: 未定义 `WINDOWS` `MACOS` 常量，导致条件编译永不触发
- **Fixed**: Linux 安装指南仅拷贝可执行文件
- **Fixed**: 空码判据遗漏当前编码词条的唯一性
- **Changed**: 新词条编号不再显示为 `0`

## [4.1.0] - 2026-06-21 [YANKED]

**Added**: 支持 `rime-dict://` 冷启动，直达指定词库目录

## [4.0.1] - 2026-06-19

**Changed**: 提升词条搜索性能

## [4.0.0] - 2026-06-18

- **Breaking**: 使用 AvaloniaUI 框架与 Semi.Avalonia 主题重构
- Added: `linux-x64` `win-x64` `osx-arm64` 跨平台支持
- Added: 多词库协同维护

## [3.0.1] - 2026-05-19

修复手动编码功能

## [3.0.0] - 2026-05-18 [YANKED]

- 支持 `columns` 定义
- 调整界面布局

## [2.1.2] - 2026-05-07

修复：修改词条后删除和截码操作不可用的问题

## [2.1.1] - 2026-05-06 [YANKED]

将换行符从 `\r\n` 改为 `\n`

## [2.1.0] - 2026-03-26 [YANKED]

优化界面，完善风险提示

## [2.0.1] - 2026-01-23

优化自动编码性能

## [2.0.0] - 2026-01-10

- **Breaking**: 取消支持 `造词码` 字段
- 支持省略 `编码` 字段
- 不再强制无重复词条

## [1.0.0]

- 使用 .NET 10.0 WPF，专注 Windows 单平台
- 使用 MVVM 架构，提升性能与可维护性
- 支持处理词库文件头
- 增强对其他输入法编码方案的支持

## [RimeTyrant]

- 使用 MAUI，支持 Windows 和 Android
- 初步支持其他输入法编码方案

## [RimeLibrarian]

引入表格，支持直接修改词条

## [JDLibManager]

改用 .NET 6.0 WPF

## [CiQi]

使用 WinForms，专用于 `星空键道6` 输入法方案

[Keep a Changelog]: https://keepachangelog.com/en/2.0.0

[语义化版本]: https://semver.org

[开发中]: https://github.com/GarthTB/RimeDictManager/compare/v4.2.0...HEAD

[4.2.0]: https://github.com/GarthTB/RimeDictManager/compare/v4.1.1...v4.2.0

[4.1.1]: https://github.com/GarthTB/RimeDictManager/compare/v4.1.0...v4.1.1

[4.1.0]: https://github.com/GarthTB/RimeDictManager/compare/v4.0.1...v4.1.0

[4.0.1]: https://github.com/GarthTB/RimeDictManager/compare/v4.0.0...v4.0.1

[4.0.0]: https://github.com/GarthTB/RimeDictManager/compare/v3.0.1...v4.0.0

[3.0.1]: https://github.com/GarthTB/RimeDictManager/compare/v3.0.0...v3.0.1

[3.0.0]: https://github.com/GarthTB/RimeDictManager/compare/v2.1.2...v3.0.0

[2.1.2]: https://github.com/GarthTB/RimeDictManager/compare/v2.1.1...v2.1.2

[2.1.1]: https://github.com/GarthTB/RimeDictManager/compare/v2.1.0...v2.1.1

[2.1.0]: https://github.com/GarthTB/RimeDictManager/compare/v2.0.1...v2.1.0

[2.0.1]: https://github.com/GarthTB/RimeDictManager/compare/v2.0.0...v2.0.1

[2.0.0]: https://github.com/GarthTB/RimeDictManager/releases/tag/v2.0.0

[1.0.0]: https://github.com/GarthTB/RimeDictManager/commit/06380ac126f6888e8ee7725e11138c7b38290646

[RimeTyrant]: https://github.com/GarthTB/RimeTyrant

[RimeLibrarian]: https://github.com/GarthTB/RimeLibrarian

[JDLibManager]: https://github.com/GarthTB/JDLibManager

[CiQi]: https://github.com/GarthTB/CiQi
