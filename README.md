# RIME 词库管理器

一个基于 WPF 的轻量级 GUI 工具，用于交互式维护 RIME 输入法
的词库。提供自动编码功能以减少麻烦和错误，支持导出操作日志。

[![平台 .NET 10.0](https://img.shields.io/badge/平台-.NET%20%2010.0-blueviolet)](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)
[![语言 C# 14.0](https://img.shields.io/badge/语言-C%23%2014.0-navy.svg)](https://github.com/dotnet/csharplang)
[![许可 MIT](https://img.shields.io/badge/许可-MIT-brown)](https://mit-license.org)
[![平台 Windows](https://img.shields.io/badge/平台-Windows-orange.svg)](https://github.com/GarthTB/RimeDictManager)
[![版本 2.0.0](https://img.shields.io/badge/版本-2.0.0-brightgreen)](https://github.com/GarthTB/RimeDictManager/releases/latest)

## ✨ 特性

- 🔒 安全
    - 完全遵循 [RIME 官方词库规范](https://github.com/rime/home/wiki/RimeWithSchemata)
    - 丰富风险提示，避免出错
- 🧠 智能
    - 提供单字，可按两笔、五笔等规则为词组自动编码
    - 可按星空键道6规则截短长码，并自动加长占位的短码
- ⚡ 高性能：按编码前缀搜索、按字词精确搜索，百万条目迅速响应
- 📝 可追溯：完整记录所有修改操作，支持导出

## 📥 安装与使用

### 系统要求

- 操作系统：Windows 10 或更高版本
- 运行依赖：[.NET 10.0 运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)

### 使用步骤

1. 下载 [最新版本压缩包](https://github.com/GarthTB/RimeDictManager/releases/latest) 并解压
2. 运行 `Rime Dict Manager.exe`
3. 打开RIME词库文件（.dict.yaml）
4. 选择编码方案，加载单字词库，以启用自动编码
5. 添加、删除、修改、截短编码
6. 查看并保存日志
7. 保存或另存词库

## 🛠 技术栈

- **.NET 10.0**：最新的 .NET 平台
- **WPF**：Windows 原生 GUI 框架
- **C# 14.0**：最新语言特性
- **CommunityToolkit.Mvvm**：MVVM 模式支持

## 📜 开源信息

- **作者**：Garth TB | 天卜 <g-art-h@outlook.com>
- **许可证**：[MIT 许可证](https://mit-license.org/)
- **项目地址**：https://github.com/GarthTB/RimeDictManager

## ⚠️ 免责声明

本工具为个人项目，不提供商业支持。使用前请备份词库文件。
开发者不对因使用本工具造成的任何数据损失负责。

## 📝 更新日志

### v2.0.0 (20260110)

**（严重）全面重构，纠正对非官方格式的错误支持。** 旧源码已封存至
[此处](https://github.com/GarthTB/RimeDictManager/tree/master/Legacy)，
所有发布文件已销毁。现严格遵循官方规范。

- 改进：不再强制无重复项，支持无编码条目
- 新增：保存时的两种排序策略

### v1.1.0 (20250723)

- 修复：排序搜索结果导致选中项识别错误的问题
- 新增：自动处理词库文件头
- 新增：有多个自动编码可选时，编码变红
- 新增：截短编码时引入编码空位的风险提示

### v1.0.0 (20250720)

- 使用 .NET 10.0 WPF 框架，专注 Windows 单平台
- 使用 MVVM 架构，提升性能与可维护性
- 完善对其他输入法方案的支持

## ⏳ 前身（已弃用或不再维护）

### [跨平台Rime词库管理器](https://github.com/GarthTB/RimeTyrant) (20240910 - 20240914)

- 使用 MAUI 框架，支持 Windows 和 Android 平台
- 初步尝试支持其他输入法方案

### [词器清单版](https://github.com/GarthTB/RimeLibrarian) (20240622 - 20241110)

- 重构界面，提供表格以直接修改条目属性

### [词器v2](https://github.com/GarthTB/JDLibManager) (20240605 - 20240620)

- 改用 .NET 6.0 WPF 框架

### [词器](https://github.com/GarthTB/CiQi) (20230513 - 20231223)

- 使用 WinForms 框架，专用于星空键道6词库
