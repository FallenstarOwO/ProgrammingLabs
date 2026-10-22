namespace ProgrammingLabs.Experiments;

internal sealed record ExperimentDescriptor(
    string Category,
    string Name,
    string Description,
    Func<Form> Factory);

internal static class ExperimentCatalog
{
    public static IReadOnlyList<ExperimentDescriptor> All { get; } = new List<ExperimentDescriptor>
    {
        new(
            "操作系统 · 处理机调度",
            "先来先服务 FCFS",
            "按进程进入就绪队列的先后顺序调度。程序按时间片逐个推进，实时显示进程的总需时间、已运行时间、剩余时间和状态。",
            () => new FcfsForm()),
        new(
            "操作系统 · 处理机调度",
            "短作业优先 SJF",
            "每次从后备作业中选择运行时间最短、且内存与磁带机资源都满足的作业投入运行；模拟资源分配、回收和作业完成过程。",
            () => new SjfForm()),
        new(
            "操作系统 · 处理机调度",
            "高响应比优先 HRRN",
            "按响应比 =（等待时间 + 要求服务时间）/ 要求服务时间 选择作业，兼顾短作业和等待较久的作业；结束后统计平均周转时间。",
            () => new HrrnForm()),
        new(
            "操作系统 · 处理机调度",
            "简单轮转法 RR",
            "时间片轮转调度。进程用完时间片后回到队尾，直到全部完成；时间片长度可调整。",
            () => new RrForm()),

        new(
            "操作系统 · 内存管理",
            "首次适应算法 First Fit",
            "从低地址开始查找第一个能容纳作业的空闲分区；支持申请、释放以及相邻空闲分区的合并。",
            () => new FirstFitForm()),
        new(
            "操作系统 · 内存管理",
            "最佳适应算法 Best Fit",
            "在所有能满足作业大小的空闲分区中选择最小的一块，尽量保留大分区；支持申请、释放和合并。",
            () => new BestFitForm()),

        new(
            "数据结构 · 哈希表",
            "链地址法哈希表",
            "按姓名首字母散列到 26 个桶，冲突数据挂成链表；支持插入、删除、查找和显示整张哈希表。",
            () => new HashChainForm()),
        new(
            "数据结构 · 哈希表",
            "开放定址哈希表",
            "内置 30 个姓名，使用线性探测处理冲突；显示每个节点的查找长度和平均查找长度。",
            () => new HashProbeForm()),

        new(
            "密码学与数论",
            "DES 加密",
            "输入 8 字节密钥和 8 字节明文，执行标准 DES 的 IP 置换、16 轮 Feistel 迭代和逆置换，以 16 进制输出密文。",
            () => new DesForm()),
        new(
            "密码学与数论",
            "数论算法工具集",
            "素数判断、最大公约数、扩展欧几里得、欧拉函数和快速模幂运算。",
            () => new NumberTheoryForm())
    };
}