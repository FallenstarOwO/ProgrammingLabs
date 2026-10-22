namespace ProgrammingLabs.Experiments;

internal sealed class BestFitForm : MemoryAllocationFormBase
{
    public BestFitForm()
        : base("最佳适应算法 Best Fit", "在所有满足大小的空闲分区中选择最小的一块，尽量保留较大的连续空闲区。")
    {
    }

    protected override int FindBlock(List<Block> blocks, int size)
    {
        int bestIndex = -1;
        int bestSize = int.MaxValue;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].Allocated || blocks[i].Size < size)
                continue;
            if (blocks[i].Size < bestSize)
            {
                bestSize = blocks[i].Size;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}