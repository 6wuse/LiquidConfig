# 液冷组态上位机 · AI 开发文档（机器可读规格书）

> 文档版本：D1.0 ｜ 生成日期：2026-11-12 ｜ 配套计划书：`液冷组态上位机_项目计划书_V1.1.md`
> 读者：AI 编码助手 / 未来的自己 / 接手评审的人
> 使用方法：把本文档作为**唯一事实源**（Single Source of Truth）注入对话。任何与本文档冲突的临时口头需求，以本文档为准，除非用户显式声明"覆盖第 X 节"。

---

## 0. 给 AI 的元指令（META）

```
ROLE: 你是本项目的 C# / WPF 高级工程师，负责按任务卡增量交付可编译、可运行的代码。
CONSTRAINTS:
  1. 先读第 1 节"工程铁律"，违反任何一条的输出视为无效，必须自行重写。
  2. 每次只交付一张任务卡（第 9 节）范围内的代码；不要顺手重构其他模块，不要新增未列出的 NuGet 依赖。
  3. 所有公共类型签名必须与第 5 节保持一致；如确需改动，先输出"变更提案"，等确认后再改。
  4. 代码必须能 `dotnet build` 通过；Core 层改动必须同时给出对应 xUnit 测试。
  5. 不确定的地方写 TODO 注释并列出待确认项，不要凭空发明协议字段或寄存器地址。
```

---

## 1. 工程铁律（Hard Rules，违反即返工）

| # | 规则 | 反例（禁止出现） |
|---|---|---|
| R1 | `LiquidConfig.Core` **不得引用** `System.Windows` / `PresentationFramework` / `System.Windows.Forms` | 在 Core 里 `Application.Current.Dispatcher` |
| R2 | 任何 I/O（串口、文件、SQLite）必须是 `async` 且带 `CancellationToken`；禁止裸 `Thread.Sleep`、禁止 `new Thread` | `while(true){ Thread.Sleep(200); }` 在主线程 |
| R3 | UI 更新只能通过 `Dispatcher` 且**批量刷新 ≤ 10 Hz**；采集/轮询不得在 UI 线程执行 | 轮询线程里直接改 `ObservableCollection` |
| R4 | 通信/协议/序列化的边界**只能有一个实现**：`Crc16`、`FloatCodec`、`RtuCodec`、`RegisterMap` 是唯一定义处，禁止在别处重算 CRC 或手拼浮点字节 | VM 里 `BitConverter.GetBytes(v)` 拼帧 |
| R5 | 上位机不得出现任何通道数量/通道语义的硬编码常量；一切来自 `ChannelDesc` 表 | `if (tagId == 0) 显示温度` |
| R6 | 数据源切换只允许通过 `ITagSource` 实现替换 + 配置项；禁止在 View / VM 里写 `if (isSimulator)` 分支 | `if (AppConfig.Mode == "Sim") {...} else {...}` |
| R7 | 异常只在"边界"（服务入口、UI 命令入口）`catch`；内部一律抛出并附带上文，禁止 `catch {}` 吞异常 | `try{...}catch{}` |
| R8 | 线程安全容器只允许 `ConcurrentDictionary` / `lock` 保护的小段临界区；禁止跨线程共享 `ObservableCollection` | 后台线程 `Tags.Add(...)` |
| R9 | 日志统一走 `ILogger`（自写 `FileLogger` + 内存环形缓冲）；禁止 `Console.WriteLine` / `MessageBox` 做日志 | `Console.WriteLine("收到帧")` |
| R10 | 新增第三方包需在第 2 节白名单内；白名单外一律拒绝，改为手写实现 | 引入 `Prism`、`Autofac`、`EF Core`、`Newtonsoft.Json` |
| R11 | 命名与目录遵循第 3 节；一个文件一个 public 类型（record/enum 可合并同族） | 一个文件塞 6 个类 |
| R12 | 协议常量（功能码、地址、命令码）只能定义在 `RegisterMap` 静态类，禁止字面量散落 | `master.ReadHolding(1, 768, 4)` |

---

## 2. 技术栈白名单

| 用途 | 允许 | 版本/说明 | 禁止替代 |
|---|---|---|---|
| 运行时 | .NET 10（`net10.0` / `net10.0-windows`） | LTS | .NET Framework、非 LTS 版本 |
| MVVM | `CommunityToolkit.Mvvm` | 8.x，仅用 `ObservableObject` / `[ObservableProperty]` / `[RelayCommand]` | Prism、Caliburn.Micro、ReactiveUI |
| 曲线 | `ScottPlot.WPF` | 5.x | LiveCharts、OxyPlot（作为备选可提，但需先确认） |
| 数据库 | `Microsoft.Data.Sqlite` | 10.x，手写 SQL | EF Core、LiteDB |
| JSON | `System.Text.Json`（内置） | `JsonSerializer` + 自定义 `JsonConverter` | Newtonsoft.Json |
| 串口 | `System.IO.Ports`（内置） | `SerialPort` 封装在 `SerialTransport` 内 | 第三方串口库 |
| 测试 | xUnit | `[Fact]` / `[Theory]` | MSTest |
| DI | `Microsoft.Extensions.Hosting` | 仅用 `ServiceCollection` 注册/解析（CP-02） | Prism、Autofac |
| 日志 | 自写 `FileLogger` | 按天滚动，UTF-8 | Serilog、NLog |

> `System.IO.Ports` 在 .NET 10 中是独立包 `System.IO.Ports`（NuGet），需显式添加；Windows 下可用。

---

## 3. 目录结构与命名

```
LiquidConfig.sln
src/
  LiquidConfig.Core/                 net10.0
    Abstractions/  ITransport.cs, IModbusClient.cs, ITagSource.cs, ILogger.cs, IClock.cs
    Models/        ChannelDesc.cs, TagVariable.cs, WidgetConfig.cs, ProjectDocument.cs,
                   DeviceInfo.cs, ChannelSnapshot.cs, ConnectionState.cs, TagQuality.cs
    Protocol/      Crc16.cs, FloatCodec.cs, RtuCodec.cs, RegisterMap.cs, ModbusException.cs
    Services/      SerialTransport.cs, ModbusClient.cs, ModbusTagSource.cs, PollScheduler.cs,
                   TagRepository.cs, ProjectSerializer.cs, ArchiveService.cs, CsvExporter.cs,
                   ReplayService.cs, FileLogger.cs, DiagnosticsLog.cs
    Simulator/     SimulatorTagSource.cs, SignalGenerator.cs, FaultInjector.cs, SimChannelConfig.cs
  LiquidConfig.App/                  net10.0-windows (WPF)
    App.xaml(.cs)
    Views/         MainWindow.xaml, CanvasView.xaml, TrendWindow.xaml, AlarmPanel.xaml,
                   ChannelParamView.xaml, DiagnosticsWindow.xaml
    ViewModels/    MainViewModel.cs, CanvasViewModel.cs, VariablePoolViewModel.cs,
                   PlotViewModel.cs, AlarmViewModel.cs, CommunicationViewModel.cs,
                   ArchiveViewModel.cs, WidgetPropertyViewModel.cs
    Controls/      DigitalGauge.cs, AlarmLamp.cs, TrendPlotControl.cs, CanvasHost.cs
    Behaviors/     DragMoveBehavior.cs, SelectionBehavior.cs
    Converters/    QualityToBrushConverter.cs, FloatFormatConverter.cs
tests/
  LiquidConfig.Core.Tests/           net10.0
docs/
```

命名：类型 `PascalCase`；私有字段 `_camelCase`；异步方法后缀 `Async`；事件 `On` 前缀处理；WPF 自定义控件文件与类同名。

---

## 4. 术语表（AI 必须统一用词，禁止同义混用）

| 术语 | 英文/类型 | 定义 |
|---|---|---|
| 通道 | Channel / `ChannelDesc` | 一路物理或虚拟测点，由网关定义，含类型、单位、量程、阈值、采样周期 |
| 变量 | Tag / `TagVariable` | 上位机变量池中的运行时对象，一一对应一个通道，携带实时值、质量位、时间戳 |
| 通道描述表 | Channel Table | 网关上报的 `ChannelDesc` 列表，自适应机制的唯一来源 |
| 质量位 | `TagQuality` | `Good=0, AlarmHigh=1, AlarmLow=2, CommLost=3, Invalid=4, Stale=5` |
| 数据源 | `ITagSource` | 上位机获取数据的唯一抽象，实现有 `ModbusTagSource`(M1) 与 `SimulatorTagSource`(M0) |
| 组态工程 | Project / `.lcproj` | 画布布局 + 控件绑定 + 通信配置的 JSON 文件 |
| 图元 / 控件 | Widget | 画布上可拖拽的元素，当前仅 3 类：`DigitalGauge`/`TrendPlot`/`AlarmLamp` |
| 运行模式 | M0 / M1 / M2 | 纯仿真 / 板卡虚拟信号源 / 真实传感器 |
| 报文 | Frame | Modbus-RTU 一帧：地址(1)+功能码(1)+数据(n)+CRC(2) |

---

## 5. 核心接口与类型契约（签名即合同）

### 5.1 Abstractions

```csharp
public interface ITransport : IAsyncDisposable
{
    string Name { get; }                       // "COM7" 或 "SIM"
    bool IsOpen { get; }
    Task OpenAsync(CancellationToken ct);
    Task CloseAsync();
    // 一次请求—应答：写入后等待一帧响应；超时抛 TimeoutException
    Task<byte[]> TransactAsync(byte[] request, TimeSpan timeout, CancellationToken ct);
    event EventHandler<bool>? OpenStateChanged;   // true=已打开
}

public interface IModbusClient
{
    Task<ushort[]> ReadHoldingAsync(byte slaveId, ushort startAddr, ushort count, CancellationToken ct);
    Task WriteSingleAsync(byte slaveId, ushort addr, ushort value, CancellationToken ct);
    Task WriteMultipleAsync(byte slaveId, ushort startAddr, ReadOnlyMemory<ushort> values, CancellationToken ct);
}

public interface ITagSource : IAsyncDisposable
{
    ConnectionState State { get; }
    Task<DeviceInfo> OpenAsync(CancellationToken ct);                       // 建立链路 + 读设备信息
    Task<IReadOnlyList<ChannelDesc>> ReadChannelTableAsync(CancellationToken ct);
    Task<ChannelSnapshot> ReadRealtimeAsync(CancellationToken ct);          // 一批通道值快照
    Task WriteChannelAsync(ChannelDesc desc, CancellationToken ct);         // 参数下发 + 回读校验
    Task<ushort> ExecuteCommandAsync(ushort code, CancellationToken ct);    // 控制区命令，返回 ack
    event EventHandler<ConnectionState>? StateChanged;
}

public interface ILogger
{
    void Info(string tag, string msg);
    void Warn(string tag, string msg);
    void Error(string tag, string msg, Exception? ex = null);
}
```

### 5.2 Models

```csharp
public enum ChannelType : ushort { Temperature = 0, Pressure = 1, Flow = 2 }

public sealed record ChannelDesc(
    int Id,                     // 0 起连续
    ChannelType Type,
    bool Enabled,
    string Unit,                // "℃" / "kPa" / "L/min"，来自网关 ASCII
    float RangeLow, float RangeHigh,
    float AlarmLow, float AlarmHigh,
    ushort SamplePeriodMs);

public sealed record TagVariable(
    int Id, string Name, ChannelType Type, string Unit,
    float RangeLow, float RangeHigh, float AlarmLow, float AlarmHigh,
    float Value, TagQuality Quality, DateTime TimestampUtc);

public sealed record ChannelSnapshot(
    DateTime TimestampUtc,
    IReadOnlyList<TagVariable> Values);

public sealed record DeviceInfo(
    ushort DeviceId, string FirmwareVersion, ushort ChannelCount,
    ushort DeviceState, ushort SimMode, uint UptimeSeconds);

public enum ConnectionState { Idle, Connecting, Online, Reconnecting, Fault }
public enum TagQuality : ushort { Good = 0, AlarmHigh = 1, AlarmLow = 2, CommLost = 3, Invalid = 4, Stale = 5 }

public enum WidgetType { DigitalGauge, TrendPlot, AlarmLamp }

public sealed class WidgetConfig      // 序列化用，扁平字段便于 JSON
{
    public string Id { get; set; } = "";
    public WidgetType Type { get; set; }
    public int? TagId { get; set; }              // DigitalGauge / AlarmLamp
    public int[] TagIds { get; set; } = [];      // TrendPlot（1~4）
    public double X { get; set; } public double Y { get; set; }
    public double W { get; set; } public double H { get; set; }
    public int TimeWindowMin { get; set; } = 30;
}

public sealed class ProjectDocument
{
    public int SchemaVersion { get; set; } = 1;
    public CanvasConfig Canvas { get; set; } = new();
    public CommConfig Communication { get; set; } = new();
    public List<WidgetConfig> Widgets { get; set; } = [];
}
```

### 5.3 协议层契约

```csharp
public static class RegisterMap
{
    // 设备信息区（只读）
    public const ushort DEV_BASE = 0x0000;
    public const ushort DEV_ID = 0x0000;
    public const ushort DEV_FW = 0x0001;      // 高字节主版本，低字节次版本
    public const ushort DEV_STATE = 0x0002;   // 0=Init 1=Running 2=Fault
    public const ushort DEV_CH_COUNT = 0x0003;
    public const ushort DEV_SIM_MODE = 0x0004;
    public const ushort DEV_UPTIME_L = 0x0005; // uint32 低字
    public const ushort DEV_UPTIME_H = 0x0006;
    public const ushort DEV_ERR_COUNT = 0x0007; // CRC/帧错误累计

    // 通道元描述区（只读，stride = 16，最大 32 通道）
    public const ushort META_BASE = 0x0100;
    public const ushort META_STRIDE = 16;
    public const int META_MAX_CHANNELS = 32;
    // 相对偏移：
    //  +0 chId(u16) | +1 chType(u16) | +2 enabled(u16) | +3..+4 unit(4×u8 ASCII)
    //  +5..+6 rangeLow(f32) | +7..+8 rangeHigh(f32) | +9..+10 alarmLow(f32)
    //  +11..+12 alarmHigh(f32) | +13 samplePeriodMs(u16) | +14..+15 reserved

    // 实时工程量区（stride = 4，最大 64 通道）
    public const ushort RT_BASE = 0x0300;
    public const ushort RT_STRIDE = 4;
    //  +0..+1 value(f32) | +2 quality(u16) | +3 seq(u16)

    // 报警状态区（stride = 2）
    public const ushort ALARM_BASE = 0x0400;
    public const ushort ALARM_STRIDE = 2;
    //  +0 flags(u16: bit0 高报 bit1 低报 bit2 断线 bit3 传感器故障) | +1 faultCode(u16)

    // 控制区（读写）
    public const ushort CTRL_CMD = 0x0500;        // 命令码
    public const ushort CTRL_ACK = 0x0501;        // 网关回写：0=成功，非 0=错误码
    public const ushort CTRL_PARAM_WIN = 0x0510;  // 参数下发窗口，stride 16，镜像一条 ChannelDesc
    public const ushort CTRL_SIM_MODE = 0x0520;   // 0 Static 1 Sine 2 Ramp 3 Step 4 Fault
    public const ushort CTRL_SIM_AMP = 0x0521;    // 振幅 ×100（float 本体用仿真通道配置）
    public const ushort CTRL_SIM_PERIOD_S = 0x0522;
    public const ushort CTRL_SIM_NOISE = 0x0523;  // 噪声 ×100

    public const ushort CMD_NONE = 0x0000;
    public const ushort CMD_CLEAR_ALARM = 0x0001;
    public const ushort CMD_SAVE_PARAM = 0x0002;   // 提交参数窗口 → 生效
    public const ushort CMD_RESTORE_DEFAULT = 0x0003;
}
```

```csharp
public static class Crc16
{   // Modbus CRC16/IBM，多项式 0xA001，初值 0xFFFF
    public static ushort Compute(ReadOnlySpan<byte> data);
    public static byte[] Append(ReadOnlySpan<byte> payload);     // data + crc(lo,hi)
    public static bool Verify(ReadOnlySpan<byte> frameWithCrc);
}

public static class FloatCodec
{   // IEEE754 单精度 ↔ 2×u16。默认字序 LowHigh（低字在前），可通过静态属性切换
    public static WordOrder Order { get; set; } = WordOrder.LowFirst;
    public static (ushort lo, ushort hi) ToRegs(float v);
    public static float FromRegs(ReadOnlySpan<ushort> twoRegs);
}

public static class RtuCodec
{   // 组帧/解帧（不含串口 IO）
    public static byte[] BuildReadRequest(byte slaveId, ushort start, ushort count);
    public static byte[] BuildWriteSingleRequest(byte slaveId, ushort addr, ushort value);
    public static byte[] BuildWriteMultipleRequest(byte slaveId, ushort start, ushort[] values);
    public static ushort[] ParseReadResponse(byte[] frame, ushort expectedCount);
    public static bool IsException(byte[] frame, out byte exceptionCode);
}
```

**常量约束**：单次 FC03 读取寄存器数 ≤ 64（留余量，Modbus 上限 125）；写多个同；所有地址常量只能出自 `RegisterMap`。

### 5.4 轮询与调度契约

```csharp
public sealed class PollScheduler
{
    // 输入：ITagSource + TagRepository + ILogger
    // 行为：三种周期任务 + 状态机 + 指数退避
    public IReadOnlyList<PollTaskConfig> Tasks { get; }
    public Task StartAsync(CancellationToken ct);
    public Task StopAsync();
    public event EventHandler<PollStat>? StatUpdated;   // 成功率、平均耗时、最后错误
}
// 默认周期：Realtime = 200ms，Alarm = 2000ms，DeviceInfo = 1000ms
// 退避：连续失败 5 次 → Reconnecting → 延迟 1s/2s/5s/10s（封顶）后重试
// 断线时：TagRepository.MarkAllCommLost()，保留最后值和时间戳
```

---

## 6. 固件侧规格（H7 最小虚拟网关）

### 6.1 约束
- 波特率固定 **115200 8N1**，从站地址默认 **1**（可改 1~247）。
- 只实现 FC `0x03` / `0x06` / `0x10`；FC `0x08`（诊断）本期不实现，遇到返回异常码 `0x01`（非法功能）。
- CRC 校验失败 / 地址不匹配 / 帧长非法 → **静默丢弃，不回应**（符合 Modbus 规范，也方便上位机测超时）。
- 帧间隔：RTU 3.5 字符时间后判帧结束；VCP 下以"字节流 + 3.5 T 静默"判定。

### 6.2 默认通道表（固件上电即生效，4 通道）
| chId | 类型 | 单位 | 量程 | 报警低 | 报警高 | 周期(ms) | 仿真波形 |
|---|---|---|---|---|---|---|---|
| 0 | Temp(0) | ℃ | 0–120 | 5 | 85 | 200 | Sine, base 45, amp 8, T 60s |
| 1 | Temp(0) | ℃ | 0–120 | 5 | 85 | 200 | Sine, base 52, amp 6, T 90s，相位 +1/4 |
| 2 | Press(1) | kPa | 0–1000 | 20 | 800 | 200 | Ramp 100→600 折返，slope 5 kPa/s |
| 3 | Flow(2) | L/min | 0–30 | 2 | 25 | 500 | Static 12.5 + 噪声 ±2% |
> 所有通道叠加 ±0.5% 高斯噪声与 0.1 量化，避免"过于完美"掩盖问题。

### 6.3 固件主循环（无 RTOS，先跑通再谈多任务）
```
init: HAL / 时钟 / USB Device(VCP) / GPIO(LED) / 默认通道表
loop:
  if VCP 收到完整帧:
      校验 CRC → 校验地址 → 按功能码分派 → 组回帧 → 发送
      出错: errCount++, 丢弃
  每 100ms:  更新合成信号值（按各通道 period 节流）→ 写入 RT 区
  每 1ms:    HAL_GetTick 递增 → 更新 uptime
  每 500ms:  LED 翻转（心跳）
  看门狗喂狗（IWDG 1s）
```

### 6.4 后期接入真实硬件的挂载点（本期留空函数）
```c
/* 预留，本期 return; 直接返回合成值 */
static float SampleReal(int chId);   // 后期：ADC 采样 / TIM 捕获 / PT100 换算
```
> 只允许改 `SampleReal` 与通道表初始化函数；Modbus 层、寄存器映射层**禁止修改**。

---

## 7. 上位机实现要点（按模块）

### 7.1 SerialTransport
- 一个实例绑定一个串口，`Open` 后启动异步读事件（`DataReceived` 或 `BaseStream.ReadAsync` 循环，二选一并说明理由）。
- `TransactAsync` 语义：写请求 → 等待 ≥ 期望长度的响应 → 按功能码校验 → 返回原始字节；**整个方法内加 `SemaphoreSlim(1)`**，杜绝并发写。
- 关闭顺序：取消读循环 → 关闭端口 → 释放信号量；`Close` 后不得再抛未捕获异常。

### 7.2 ModbusClient
- 职责：把 `ReadHoldingAsync` 转成 `RtuCodec.BuildReadRequest` → `TransactAsync` → `ParseReadResponse`。
- 异常归一：`TimeoutException`（超时）、`ModbusException(code)`（异常码）、`IOException`（链路断）。
- 重试策略在 `PollScheduler`，不在 `ModbusClient`（保持单次语义，便于测试）。

### 7.3 ModbusTagSource
- `OpenAsync`：打开链路 → 读 `DEV_ID..DEV_ERR_COUNT`（一次 8 寄存器）→ 读 `DEV_CH_COUNT` → 分包读 META 区（每请求 4 个通道 = 64 寄存器）→ 返回 `DeviceInfo`。
- `ReadRealtimeAsync`：按 `channelCount` 计算需读寄存器数，单请求 ≤ 64 寄存器（16 通道/包），分包后拼装为 `ChannelSnapshot`。
- `WriteChannelAsync`：写 `CTRL_PARAM_WIN`（16 寄存器，一条描述）→ 写 `CTRL_CMD = CMD_SAVE_PARAM` → 轮询 `CTRL_ACK`（≤ 500 ms）→ 回读该通道 META 校验一致，不一致抛 `ModbusException(0x04)`。

### 7.4 TagRepository
```csharp
public sealed class TagRepository
{
    public IReadOnlyList<TagVariable> Tags { get; }        // 供 UI 结构绑定（只读视图）
    public void Rebuild(IEnumerable<ChannelDesc> descs);   // 保留同 Id 的既有值与绑定关系
    public void ApplySnapshot(ChannelSnapshot snap);       // 后台线程调用，线程安全
    public void MarkAllCommLost();
    public TagVariable? Get(int id);
    public event EventHandler<TagsChangedEventArgs>? Changed;  // UI 10Hz 批量派发
}
```
实现要求：值写入走 `ConcurrentDictionary`（或 `lock` 数组）；`Changed` 事件由 `DispatcherTimer(100ms)` 统一触发一次，UI 侧只刷新可见控件。

### 7.5 画布（CanvasHost + 3 控件）
- 容器：`ItemsControl` + `ItemsPanel = Canvas`，`ItemContainerStyle` 绑定 `Canvas.Left/Top/Width/Height`。
- `DigitalGauge` / `AlarmLamp`：继承 `FrameworkElement`，重写 `OnRender(DrawingContext)` 自绘（不依赖图片资源）。
- `TrendPlotControl`：内部承载 `ScottPlot.WPF.WpfPlot`，暴露 `BindTags(int[])` 与 `AppendBatch(ChannelSnapshot)`；**曲线只在 10 Hz 刷新**，采样点进环形缓冲（每通道 3600 点）。
- 拖拽：`DragMoveBehavior`（附加属性）——`MouseLeftButtonDown` 记录起点与原始坐标 → `MouseMove` 换算 `Canvas` 坐标（用 `TranslatePoint`，不用鼠标屏幕坐标直接减）→ `MouseLeftButtonUp` 吸附 8 px 网格 → 写回 `WidgetConfig.X/Y` 并触发保存脏标记。
- 选中：`SelectionBehavior` 用一个 `Adorner` 画虚线框；`Delete` 键删除选中控件（先 `Remove` 再 `Refresh`，避免空引用）。
- 缩放/平移：`LayoutTransform = ScaleTransform`（缩放在布局层，避免鼠标命中测试错位），上限 3.0/下限 0.4。

### 7.6 报警
- 判定来源双通道：网关 `alarmFlags`（权威）+ 上位机本地阈值复核（防御）。
- 分级：`Warn`（接近阈值，±5% 死区外）/ `Fault`（超限）；带**回差**（恢复阈值 = 阈值 ∓ 2% 量程），防抖。
- 报警日志：内存 `ObservableCollection` 保留 1000 条 + 落 `alarms.log`；同通道同级别 30 s 内不重复记录（抑制）。
- UI：状态条红/橙点 + 报警面板列表（时间、通道、级别、值、阈值、确认按钮）。

### 7.7 归档
```sql
CREATE TABLE IF NOT EXISTS samples(
  ts INTEGER NOT NULL,      -- Unix ms
  tag_id INTEGER NOT NULL,
  value REAL NOT NULL,
  quality INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_samples_ts_tag ON samples(ts, tag_id);
```
- 写入批量：攒 500 条或 5 s 触发一次 `INSERT`（多值一条语句，参数数组）；归档默认 5 s 一次快照，非每次都存。
- 查询：`WHERE ts BETWEEN ? AND ? AND tag_id IN (...)`，上限 10 万行，超过提示缩窄范围。
- CSV：UTF-8 **带 BOM**，表头 `time,CH0_℃,CH1_℃,...`，时间 `yyyy-MM-dd HH:mm:ss.fff`。

### 7.8 状态机与 Diagnostics
- `ConnectionState` 变更必须同时：写日志、更新状态条、清/置 `CommLost`、在诊断窗口插入一条事件。
- `DiagnosticsLog`：`ConcurrentQueue<FrameRecord>`，容量 5000，字段 `{Time, Dir(↑/↓), SlaveId, FuncCode, StartAddr, Count, RawHex, ElapsedMs, Result}`；导出 txt。
- 统计：总帧数、成功/超时/CRC 错/异常码次数、平均 RTT、最大 RTT，状态条显示成功率。

---

## 8. 组态工程与配置文件

### 8.1 `.lcproj`（组态工程）
见任务卡 T12；`schemaVersion=1`，字段名 `camelCase`，枚举用字符串（`"DigitalGauge"`），浮点保留 2 位。
加载宽容策略：未知 `type` → 跳过并日志告警，不抛异常；缺失 `tagId` → 控件显示"未绑定"。

### 8.2 `appsettings.json`（运行配置）
```json
{
  "dataSource": "Simulator",
  "transport": { "port": "COM7", "baud": 115200, "slaveId": 1, "timeoutMs": 200, "retries": 2 },
  "simulator": { "channelCount": 4, "faultInject": null, "noiseRatio": 0.005 },
  "poll": { "realtimeMs": 200, "alarmMs": 2000, "deviceMs": 1000 },
  "archive": { "enabled": true, "intervalSec": 5, "dbPath": "data/liquid.db" },
  "ui": { "refreshHz": 10, "gridSize": 8 }
}
```

---

## 9. 任务卡（Task Cards，按序执行，一次一张）

> 每张卡格式：`目的 / 产出 / 依赖 / 验收(DoD)`。完成后必须 `dotnet build` 通过并提交一次。

**T1 解决方案骨架**
目的：把目录与项目建起来。产出：`LiquidConfig.sln` + 3 个项目（Core / App / Tests）+ 白名单包引用。依赖：无。DoD：`dotnet build` 通过；Core 中 `dotnet list package` 无 WPF 相关包。

**T2 CRC16 + 单元测试**
产出：`Protocol/Crc16.cs` + `Crc16Tests.cs`。DoD：`03 00 00 00 02` 的 CRC 为 `C4 0B`（即附加后 `... 02 C4 0B`）通过；空输入、单字节、奇数长度均有用例；表驱动 10 组已知向量全通过。

**T3 FloatCodec + RtuCodec**
产出：浮点编解码（双字序）、组帧/解帧（读/写单/写多/异常帧）。DoD：`45.0℃` → `[0x0000,0x4234]`（LowFirst）往返一致；异常帧 `83 02` 能被 `IsException` 识别；乱长度帧返回 `false` 不抛异常。

**T4 RegisterMap 与文档一致性**
产出：`RegisterMap.cs` + `docs/register-map.md`（由常量生成的表）。DoD：本文件第 5.3 节所有地址在代码中一一对应，编译期常量，无重复字面量（用 `grep` 自查通过）。

**T5 固件：VCP 回环（M1 前提）**
产出：CubeMX 工程 + `main.c` 回显。DoD：插 USB 后设备管理器出现 `STMicroelectronics Virtual COM Port`；SSCOM 发送任意字符能回显；LED 1 Hz 闪烁。

**T6 固件：Modbus 从站（FC03 只读，默认通道表）**
产出：`modbus_slave.c/h`、`channel_table.c/h`。DoD：Modbus Poll 读 `0x0000` 8 个寄存器得到 `deviceId/fw/state/count=4/simMode`；读 `0x0100` 16 个寄存器能还原通道 0 描述；错误地址返回静默无响应；错误 CRC 无响应且 `DEV_ERR_COUNT` 递增。

**T7 固件：合成信号 + FC06/FC16 写入**
DoD：读 `0x0300` 4 个寄存器可见浮点值随时间正弦变化；写 `0x0520=1` 后波形切换生效；上位机能通过 FC16 写参数窗口并 commit。

**T8 SerialTransport + IModbusClient**
产出：`SerialTransport.cs`、`ModbusClient.cs`。DoD：接 T6 固件能读通；拔线 200 ms 内抛 `TimeoutException` 且不卡死；连续 100 次读无内存增长异常；`TransactAsync` 并发调用被串行化（用测试并发 20 次验证）。

**T9 ModbusTagSource（M1 打通）**
产出：`ModbusTagSource.cs`。DoD：`OpenAsync` 返回 4 通道 `DeviceInfo`；`ReadRealtimeAsync` 一次返回 4 个 `TagVariable` 且值与 Modbus Poll 一致；`WriteChannelAsync` 改报警上限后回读一致。

**T10 SimulatorTagSource（M0 打通）**
产出：`Simulator/` 四个文件。DoD：与 T9 同一接口、同一调用序列下 UI 表现一致；支持 `channelCount` 任意 1~32；`FaultInject` 可注入 `Timeout`/`BadCrc`/`Freeze`/`OverRange` 四种。

**T11 TagRepository + PollScheduler + 状态机**
产出：`TagRepository.cs`、`PollScheduler.cs`、`DiagnosticsLog.cs`。DoD：M0 下 200 ms 刷新稳定；注入断线后 3 s 内 `State=Reconnecting` 并全 Tag 置 `CommLost`；恢复后 5 s 内回 `Online` 且值恢复；10 Hz 内 UI 通知次数不超过 12 次/秒（用日志计数验证）。

**T12 主窗口 + 通信状态条 + 报文调试窗口**
产出：`MainWindow` + `CommunicationViewModel` + `DiagnosticsWindow`。DoD：能切换数据源、开关链路、看实时帧表；关闭窗口时链路优雅释放（无 `ObjectDisposedException`）。

**T13 变量池面板**
DoD：表格列出全部 Tag（名称/类型/单位/值/质量/时间），M0/M1 一致；质量色正确（Good 深灰 / AlarmHigh 红 / CommLost 灰）。

**T14 画布骨架 + 拖拽**
产出：`CanvasHost`、`DragMoveBehavior`、`SelectionBehavior`。DoD：从工具箱拖入 3 类控件到任意位置；拖动跟手（无明显延迟）；吸附 8 px；Delete 删除；Ctrl+滚轮缩放命中测试仍正确。

**T15 三类控件自绘**
产出：`DigitalGauge.cs`、`AlarmLamp.cs`、`TrendPlotControl.cs`。DoD：仪表显示 `45.3 ℃` 且质量变色；指示灯随 `alarmFlags` 红/黄/灰；曲线 4 通道叠加、30 min 窗口无内存膨胀（连续 30 min 观察）。

**T16 属性面板 + 控件绑定**
DoD：选中控件后右侧可改 `tagId`/`tagIds`/尺寸/时间窗；改 `tagId` 后立即生效；绑定不存在的 `tagId` 显示"未绑定"不崩溃。

**T17 `.lcproj` 存取**
产出：`ProjectSerializer.cs`。DoD：6 控件工程 → 保存 → 重启加载 → 位置/尺寸/绑定全部还原（像素级一致）；损坏 JSON 加载给出可读错误而非崩溃；未知控件类型被跳过并告警。

**T18 报警系统**
产出：`AlarmViewModel` + 回差判定 + 抑制逻辑 + `alarms.log`。DoD：注入超限 3 s 内报警上屏；回落后 5% 死区才恢复；同通道 30 s 内不重复刷屏；日志可导出。

**T19 SQLite 归档 + CSV 导出**
产出：`ArchiveService.cs`、`CsvExporter.cs`、`ArchiveViewModel.cs`。DoD：归档 10 min 后查库行数 = 通道数 × 采样次数（误差 < 2 行）；导出 CSV 在 Excel / Origin 均可直接打开；查询 1 万行 < 300 ms。

**T20 历史回放 + 烤机收口**
产出：`ReplayService.cs` + 测试记录表 + README。DoD：回放 1×/10×/50× 曲线与原始一致；M1 下连续 6 h 无崩溃、内存平稳（±20 MB 内）、成功率 > 99.5%；8 类故障注入全部记录在测试表。

---

## 10. 测试策略（AI 交付必须带的）

| 层 | 类型 | 覆盖目标 |
|---|---|---|
| `Protocol`（CRC/浮点/组解帧） | 纯单元测试 | 行覆盖 ≥ 90%，含异常帧、边界值、双字序 |
| `Framing + 分包` | 单元测试（假 Transport） | 通道数 1/4/32 的分包边界：16 通道/包、64 寄存器/包 |
| `Simulator` | 单元测试 + 集成 | 同一序列可复现（固定随机种子） |
| `TagRepository` | 并发单测 | 4 线程写入 + 1 线程读快照，无 `InvalidOperationException` |
| `ProjectSerializer` | 往返单测 | 序列化→反序列化→再序列化字节一致 |
| 端到端 | 手工 M0 / M1 | 见任务卡 DoD |

---

## 11. 已知易错点（AI 自查清单）

1. 跨线程改 `ObservableCollection` → 用 `Dispatcher.BeginInvoke` 或 10 Hz 批量事件。
2. `SerialPort.DataReceived` 在非 UI 线程触发 → 回调里只入队，不更新 UI。
3. `TransactAsync` 未加锁 → 并发写导致回帧错位（表现为"值偶尔跳到别的通道"）。
4. Modbus 浮点字序搞反 → 数值显示 `3.4e38` 或 `0`，先做字序自检。
5. 一个 FC03 读 100+ 寄存器 → 部分设备静默丢弃，本工程限制 ≤ 64。
6. 画布坐标用鼠标屏幕坐标相减 → 缩放后位移错误，必须 `TranslatePoint`。
7. `ItemsControl` 的 `Canvas.Left` 未通过 `ItemContainerStyle` 绑定 → 控件全叠在 (0,0)。
8. 断线时把值清零 → 曲线出现假"归零"，应保留最后值 + `CommLost`。
9. SQLite 每条 `INSERT` 都 commit → 磁盘 IO 打满，必须批量。
10. 异常码 `0x02`（非法地址）被当超时处理 → 状态机误判为断线，需区分异常码与超时。

---

## 12. 增量扩展约定（后期加硬件/加功能时）

1. **加一路传感器**：只改固件 `channel_table.c` 增一条 `ChannelDesc` → 上位机重连自动出现新 Tag，画布可绑定，**C# 代码零改动**。
2. **加控件类型**：新增 `WidgetType` 枚举值 + 一个自绘类 + `ProjectSerializer` 映射分支 + 属性面板条目，四处同步；旧工程不受影响。
3. **换通信介质（RS485 / 以太网 Modbus TCP）**：只实现新的 `ITransport`（TCP 版），`IModbusClient` 不动。Modbus TCP 需新增 MBAP 头封装类，不改寄存器映射。
4. **加闭环控制**：新增 `ControlLoopService`（PID），输入 `TagRepository` 快照，输出写 `CTRL` 区寄存器；不改通信与画布层。

---

## 13. 交付前自查表（Definition of Done，全局）

- [ ] `dotnet build -c Release` 零警告（至少零 error，警告逐条登记）
- [ ] `dotnet test` 全绿，Core 协议层覆盖 ≥ 90%
- [ ] 无 `catch {}`、无 `Thread.Sleep`、无 `Console.WriteLine`（用 grep 自查）
- [ ] 无硬编码通道语义（grep `"CH0"`、`== 0` 判断）
- [ ] M0 与 M1 同一操作序列表现一致（录屏对比）
- [ ] `.lcproj` 坏文件能优雅降级
- [ ] 6 h 烤机通过，附内存曲线截图
- [ ] 文档三件套齐：计划书、本开发文档、寄存器地址表
