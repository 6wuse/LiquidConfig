# 变更记录（CHANGELOG）

> 项目：液冷组态上位机 ｜ 依据 AI 开发文档 META 第 3 条：任何对文档的改动先记变更提案，经用户确认后落地于此。

## CP-01 目标框架 .NET 8 → .NET 10（2026-11-12，已确认）
- 原因：.NET 8 已于 2026-11-10 结束官方支持；本机已装 .NET SDK 10.0.400。
- 改动：全部项目目标 `net10.0` / `net10.0-windows`；`Microsoft.Data.Sqlite`、`System.IO.Ports` 版本对齐 10.x；两份文档中相关表述已同步修改。

## CP-02 白名单新增 Microsoft.Extensions.Hosting（2026-11-12，按模板沿用）
- 原因：模板已使用 MS 官方轻量 DI；铁律 R10 针对的是 Prism/Autofac 一类重型框架。
- 改动：开发文档 §2 白名单新增 DI 行，仅用于 `ServiceCollection` 注册/解析。

## CP-03 里程碑重排：纯软件/组态先行（2026-11-12，用户指示）
- 原因：硬件不齐（仅 STM32H743 核心板，无传感器/调理板/24V）。
- 改动：开发顺序 T1 → T10 → T11 → T12 → T13 → T14–T20；T2–T4（协议）与 T5–T9（固件/通信）推迟至硬件就绪后插回，上位机零改动。

## CP-04 开发基线改为用户模板仓库（2026-11-12，用户指示）
- 仓库：`E:\project\WPF\组态软件\6wuse`（用户自制 WPF 模板，git 已初始化并带 origin）。
- 改动：工程 `WpfApp1` → `LiquidConfig.App`；沿用模板的 slnx 方案格式与 MS DI 组合根。
