# 大学实验合集 · C# / WinForms 版

把大学时期的操作系统、数据结构、密码学实验统一迁移到 **C# + WinForms**：
一个窗口按分类选择实验，每个实验在独立窗口中运行，**不再需要安装 C++ 编译器**。

项目英文名：**ProgrammingLabs**（编程课实操合集）。

## 运行要求

- Windows 10 / 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)（本机已安装 10.0.203）
- 不需要 Visual Studio；用 `dotnet` 命令行即可编译运行

如果希望兼容旧环境，可以把 `ProgrammingLabs.csproj` 里的
`<TargetFramework>net10.0-windows</TargetFramework>` 改为
`net8.0-windows`，然后重新编译。

## 编译与运行

```powershell
cd d:\Git\ProgrammingLabs
dotnet build ProgrammingLabs.sln -c Debug
dotnet run --project .\ProgrammingLabs\ProgrammingLabs.csproj
```

也可以直接用 Visual Studio 打开 `ProgrammingLabs.sln`，把 `ProgrammingLabs` 设为启动项目后运行。

## 直接打开（推荐）

如果只是想运行，不需要打开命令行：

- 双击项目根目录的 **`ProgrammingLabs.lnk`**；
- 或双击 **`publish\ProgrammingLabs.exe`**；
- 或双击 **`start-ProgrammingLabs.cmd`**（找不到发布版时会自动编译 Release 版本）。

`publish\ProgrammingLabs.exe` 是自包含单文件版本，已经打包 .NET 运行时，**不需要另外安装 .NET**，直接拷贝到其他 Windows x64 电脑也能运行。文件约 116 MB，属于正常大小。

## Git

本目录已初始化为独立 Git 仓库（`main` 分支），位于 `D:\Git\ProgrammingLabs`，与 `D:\Git` 父仓库相互独立。

远程仓库：<https://github.com/FallenstarOwO/ProgrammingLabs>

推送后续更新：

```powershell
git push
```

`publish/`、`bin/`、`obj/`、`*.lnk` 已加入 `.gitignore`，不会提交 116 MB 的 exe 和本机快捷方式。

## 项目结构

```
ProgrammingLabs/
├─ ProgrammingLabs.sln
├─ ProgrammingLabs/
│  ├─ Program.cs                  ← 程序入口
│  ├─ MainForm.cs                 ← 主窗口：分类树 + 说明 + 打开实验
│  ├─ Ui/
│  │  ├─ Theme.cs                 ← 字体、颜色、按钮、表格、输出框样式
│  │  └─ ExperimentFormBase.cs    ← 实验窗口基类：标题、工具栏、输入区、输出区
│  └─ Experiments/
│     ├─ ExperimentCatalog.cs     ← 10 个实验的注册表（分类、名称、说明、窗体工厂）
│     ├─ FcfsForm.cs              ← 先来先服务
│     ├─ SjfForm.cs               ← 短作业优先
│     ├─ HrrnForm.cs              ← 高响应比优先
│     ├─ RrForm.cs                ← 简单轮转法
│     ├─ MemoryAllocationFormBase.cs
│     ├─ FirstFitForm.cs          ← 首次适应
│     ├─ BestFitForm.cs           ← 最佳适应
│     ├─ HashChainForm.cs         ← 链地址法哈希表
│     ├─ HashProbeForm.cs         ← 开放定址哈希表
│     ├─ DesCipher.cs             ← DES 算法实现
│     ├─ DesForm.cs               ← DES 界面
│     └─ NumberTheoryForm.cs      ← 数论工具集 + 算法实现
└─ README.md
```

## 已迁移的 10 个实验

| 分类 | 实验 | 功能 |
|---|---|---|
| 操作系统 · 处理机调度 | 先来先服务 FCFS | 按队列顺序逐秒推进，实时显示运行/等待/完成状态 |
| 操作系统 · 处理机调度 | 短作业优先 SJF | 选择剩余时间最短且内存/磁带机满足的作业，模拟资源分配与回收 |
| 操作系统 · 处理机调度 | 高响应比优先 HRRN | 按响应比选择作业，统计平均周转时间与带权周转时间 |
| 操作系统 · 处理机调度 | 简单轮转法 RR | 时间片轮转，时间片长度可调 |
| 操作系统 · 内存管理 | 首次适应 First Fit | 从低地址找到第一个足够大的空闲分区 |
| 操作系统 · 内存管理 | 最佳适应 Best Fit | 选择满足大小的最小空闲分区 |
| 数据结构 · 哈希表 | 链地址法哈希表 | 26 个字母桶 + 链表，支持插入/删除/查找/显示 |
| 数据结构 · 哈希表 | 开放定址哈希表 | 30 个姓名、线性探测、查找长度与平均查找长度 |
| 密码学与数论 | DES 加密 | IP 置换、16 轮 Feistel、逆置换，输出 16 进制密文 |
| 密码学与数论 | 数论算法工具集 | 素数、GCD、扩展欧几里得、欧拉函数、快速模幂 |

## 迁移时做的改进

- **异步动画**：调度类实验用 `async/await + Task.Delay` 逐步刷新，不阻塞 UI。
- **内存分配更健壮**：申请/释放后会正确合并左右相邻空闲分区，并处理“剩余空间不足一个最小分区”的情况。
- **哈希表修复**：
  - 链地址法删除时使用正确的表长计算桶下标（原 C++ 版本误用了 `LEN`）。
  - 开放定址插入时用字符串比较判断重复，删除采用墓碑标记。
- **DES 校验**：使用标准测试向量验证通过：
  - 密钥 `133457799BBCDFF1`
  - 明文 `0123456789ABCDEF`
  - 密文 `85e813540f0ab405`
- **模幂改进**：使用快速幂 + `Int128` 中间乘法，避免原 C++ 版本 `pow()` 的溢出问题。
- **输入表格可编辑**：FCFS、SJF、RR、HRRN 的进程/作业列表可以直接在 DataGridView 中增删改。
- **分类标题不可选中**：左侧分类只用于分组，只有具体实验条目可以点击。

## 其他说明

- 如果打开某个实验时出现错误，可在 exe 同目录查看 `open-error.log`，里面记录了完整异常信息。
- 本项目的 DES 仅用于教学演示，请勿用于真实数据加密。
