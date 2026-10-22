using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal abstract class MemoryAllocationFormBase : ExperimentFormBase
{
    private const int TotalSize = 600;
    private const int BaseAddress = 40;
    private const int MinBlockSize = 40;

    private readonly List<Block> _blocks = new();
    private readonly List<Operation> _operations;
    private int _step;

    protected MemoryAllocationFormBase(string title, string subtitle)
        : base(title, subtitle)
    {
        _operations = new List<Operation>
        {
            new(1, OperationType.Allocate, 130),
            new(2, OperationType.Allocate, 60),
            new(3, OperationType.Allocate, 100),
            new(2, OperationType.Free, 0),
            new(4, OperationType.Allocate, 200),
            new(3, OperationType.Free, 0),
            new(1, OperationType.Free, 0),
            new(5, OperationType.Allocate, 140),
            new(6, OperationType.Allocate, 60),
            new(7, OperationType.Allocate, 50)
        };

        var hint = new Label
        {
            Text = "作业序列固定为教材中的 10 个操作（总内存 600，起始地址 40，最小分区 40）。\r\n" +
                   "“执行下一步”观察单次分配/释放，“自动演示”按顺序连续执行。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 0, 0, 6)
        };
        AddInput(hint);

        var grid = Theme.Grid();
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Work",
            HeaderText = "作业号",
            FillWeight = 20
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Type",
            HeaderText = "操作",
            FillWeight = 25
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Size",
            HeaderText = "申请大小",
            FillWeight = 25
        });
        foreach (Operation operation in _operations)
            grid.Rows.Add(operation.Work, operation.Type == OperationType.Allocate ? "申请" : "释放",
                operation.Type == OperationType.Allocate ? operation.Size : "-");

        var gridPanel = new Panel { Dock = DockStyle.Fill, Height = 190 };
        gridPanel.Controls.Add(grid);
        AddInput(gridPanel);

        var step = Theme.PrimaryButton("执行下一步");
        var auto = Theme.SecondaryButton("自动演示");
        var reset = Theme.SecondaryButton("重置");
        var clear = Theme.SecondaryButton("清空输出");
        step.Click += (_, _) => ExecuteNextStep();
        auto.Click += async (_, _) => await RunWithCancellationAsync(auto, AutoRunAsync);
        reset.Click += (_, _) => ResetAll();
        clear.Click += (_, _) => Output.Clear();
        AddToolbar(step, auto, reset, clear);

        ResetMemory();
        Theme.AppendLine(Output, "内存初始状态（总大小 600，起始地址 40）：");
        DumpMemory();
    }

    protected abstract int FindBlock(List<Block> blocks, int size);

    protected override int InitialInputHeight => 300;

    private void ResetAll()
    {
        _step = 0;
        ResetMemory();
        Output.Clear();
        Theme.AppendLine(Output, "已重置。内存初始状态：一个 600 大小的空闲分区。");
        Theme.AppendLine(Output);
        DumpMemory();
    }

    private void ResetMemory()
    {
        _blocks.Clear();
        _blocks.Add(new Block
        {
            Number = 0,
            Address = BaseAddress,
            Size = TotalSize,
            Allocated = false,
            WorkNumber = 0
        });
    }

    private void ExecuteNextStep()
    {
        if (_step >= _operations.Count)
        {
            Theme.AppendLine(Output, "作业序列已完成。");
            return;
        }

        Operation operation = _operations[_step];
        _step++;

        if (operation.Type == OperationType.Allocate)
            Allocate(operation.Work, operation.Size);
        else
            Release(operation.Work);

        DumpMemory();
        if (_step >= _operations.Count)
        {
            Theme.AppendLine(Output, "作业序列完成。");
            Theme.AppendLine(Output);
        }
    }

    private async Task AutoRunAsync(CancellationToken token)
    {
        if (_step >= _operations.Count)
        {
            Theme.AppendLine(Output, "作业序列已完成，请先点击“重置”。");
            return;
        }

        while (_step < _operations.Count)
        {
            token.ThrowIfCancellationRequested();
            ExecuteNextStep();
            await Task.Delay(650, token);
        }
    }

    private void Allocate(int work, int size)
    {
        int index = FindBlock(_blocks, size);
        Theme.AppendLine(Output, $"作业 {work} 申请 {size} 单位内存。");

        if (index < 0)
        {
            Theme.AppendLine(Output, "没有足够大的空闲分区，申请失败。");
            Theme.AppendLine(Output);
            return;
        }

        Block block = _blocks[index];
        if (block.Size - size > MinBlockSize)
        {
            var allocated = new Block
            {
                Address = block.Address,
                Size = size,
                Allocated = true,
                WorkNumber = work
            };
            var remainder = new Block
            {
                Address = block.Address + size,
                Size = block.Size - size,
                Allocated = false,
                WorkNumber = 0
            };
            _blocks[index] = allocated;
            _blocks.Insert(index + 1, remainder);
        }
        else
        {
            block.Allocated = true;
            block.WorkNumber = work;
        }

        Renumber();
        Theme.AppendLine(Output);
    }

    private void Release(int work)
    {
        int index = _blocks.FindIndex(b => b.Allocated && b.WorkNumber == work);
        Theme.AppendLine(Output, $"作业 {work} 释放内存。");

        if (index < 0)
        {
            Theme.AppendLine(Output, "未找到该作业占用的分区，释放失败。");
            Theme.AppendLine(Output);
            return;
        }

        _blocks[index].Allocated = false;
        _blocks[index].WorkNumber = 0;

        if (index + 1 < _blocks.Count && !_blocks[index + 1].Allocated)
        {
            _blocks[index].Size += _blocks[index + 1].Size;
            _blocks.RemoveAt(index + 1);
        }

        if (index - 1 >= 0 && !_blocks[index - 1].Allocated)
        {
            _blocks[index - 1].Size += _blocks[index].Size;
            _blocks.RemoveAt(index);
        }

        Renumber();
        Theme.AppendLine(Output);
    }

    private void Renumber()
    {
        for (int i = 0; i < _blocks.Count; i++)
            _blocks[i].Number = i;
    }

    private void DumpMemory()
    {
        int free = _blocks.Where(b => !b.Allocated).Sum(b => b.Size);
        Theme.AppendLine(Output, $"步骤 {_step} / {_operations.Count}    剩余可用内存：{free}");
        Theme.AppendHeader(Output, "分区号", "起始地址", "分区大小", "分区状态", "作业号");
        foreach (Block block in _blocks)
        {
            string state = block.Allocated ? "已分配" : "空闲";
            string work = block.Allocated ? block.WorkNumber.ToString() : "-";
            Theme.AppendLine(Output,
                $"{block.Number,6} {block.Address,10} {block.Size,10}  {state,-6} {work,6}");
        }
        Theme.AppendLine(Output);
    }

    protected sealed class Block
    {
        public int Number { get; set; }
        public int Address { get; set; }
        public int Size { get; set; }
        public bool Allocated { get; set; }
        public int WorkNumber { get; set; }
    }

    private sealed record Operation(int Work, OperationType Type, int Size);

    private enum OperationType
    {
        Allocate,
        Free
    }
}