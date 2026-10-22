namespace ProgrammingLabs.Experiments;

internal sealed class FirstFitForm : MemoryAllocationFormBase
{
    public FirstFitForm()
        : base("首次适应算法 First Fit", "从低地址开始查找第一个能容纳作业的空闲分区，优先使用低地址空间。")
    {
    }

    protected override int FindBlock(List<Block> blocks, int size)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (!blocks[i].Allocated && blocks[i].Size >= size)
                return i;
        }
        return -1;
    }
}