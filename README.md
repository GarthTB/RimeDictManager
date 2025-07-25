# RimeDictManager - Rime输入法词库管理器

[![用前必读 README.md](https://img.shields.io/badge/用前必读-README.md-red)](https://github.com/GarthTB/RimeDictManager/blob/master/README.md)
[![开发框架 .NET 10.0](https://img.shields.io/badge/开发框架-.NET%2010.0-blueviolet)](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)
[![最新版本 1.1.0](https://img.shields.io/badge/最新版本-1.1.0-brightgreen)](https://github.com/GarthTB/RimeDictManager/releases/latest)
[![开源协议 MIT](https://img.shields.io/badge/开源协议-MIT-brown)](https://mit-license.org/)

## 📖 项目简介

RimeDictManager 是一款专为 Rime 输入法设计的 Windows GUI 词库管理工具，
支持对 `.dict.yaml` 词库文件进行高效编辑与维护。
通过直观的界面操作，用户可以轻松完成词条增删改查、编码优化等复杂操作，
同时严格保持 Rime 官方词库格式规范。

## ✨ 核心功能

- ✏️ **词库修改**
    - 添加、删除词条，以及直接修改已有词条
    - 截短、腾让编码（用于变长编码方案，如星空键道6）
- 🔍 **高效检索** 按词组或按编码前缀实时搜索
- 🧠 **自动编码** 利用单字词库为词组自动编码
- ⚠️ **安全防护**
    - 提示词组、编码重复，禁止全同词条
    - 日志完整记录所有修改行为

## 📥 安装与使用

### 系统要求

- 操作系统：Windows 10 或更高版本
- 运行依赖：[.NET 10.0 运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)

### 使用步骤

1. 下载 [最新版本压缩包](https://github.com/GarthTB/RimeDictManager/releases/latest)
2. 解压后运行 `RimeDictManager.exe`
3. 载入词库文件，开始管理

### 操作示例

- **添加词条**
    1. 输入词组
    2. 输入手动编码；或启用自动编码并载入单字词库（仅首次启用时），然后选择一个自动生成的编码
    3. （可选）输入权重和造词码
    4. 点击“添加词条”按钮
- **删除词条**
    1. 按词组或按编码前缀搜索
    2. 在搜索结果中选中待删除的词条
    3. 点击“删除词条”按钮
- **截短编码**
    1. 启用自动编码并载入单字词库（仅首次启用时）
    2. 按编码前缀搜索
    3. 在搜索结果中选中待截短编码的词条
    4. 点击“截短编码”按钮
- **手动修改**
    1. 按词组或按编码前缀搜索
    2. 在搜索结果中直接修改目标词条
    3. 点击“应用修改”按钮

## 🛠 技术架构

- **语言**：C#
- **框架**：.NET 10.0 WPF
- **依赖**：[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)

## 📜 开源信息

- **作者**：GarthTB | 天卜 <g-art-h@outlook.com>
- **许可证**：[MIT 许可证](https://mit-license.org/)
    - 可以自由使用、修改和分发软件
    - 可以用于商业项目
    - 必须保留原始版权声明 `Copyright (c) 2025 GarthTB | 天卜`
- **项目地址**：https://github.com/GarthTB/RimeDictManager

## 📝 更新日志

### v1.1.0 (20250723)

- 修复：排序搜索结果导致选中项识别错误的问题
- 新增：自动处理词库文件头
- 新增：有多个自动编码可选时，编码变红
- 新增：截短编码时引入编码空位的风险提示

### v1.0.0 (20250720)

- 使用 .NET 10.0 WPF 框架，专注 Windows 单平台
- 使用 MVVM 架构，提升性能与可维护性
- 完善对其他输入法编码方案的支持

## ⏳ 前身（已弃用或不再维护）

### [跨平台Rime词库管理器](https://github.com/GarthTB/RimeTyrant) (20240910 - 20240914)

- 使用 MAUI 框架，支持 Windows 和 Android 平台
- 初步尝试支持其他输入法编码方案

### [词器清单版](https://github.com/GarthTB/RimeLibrarian) (20240622 - 20241110)

- 大幅调整并简化界面，提供表格以直接修改词条信息

### [词器v2](https://github.com/GarthTB/JDLibManager) (20240605 - 20240620)

- 改用 .NET 6.0 WPF 框架

### [词器](https://github.com/GarthTB/CiQi) (20230513 - 20231223)

- 原始雏形，使用 WinForms 框架，专用于星空键道6词库
