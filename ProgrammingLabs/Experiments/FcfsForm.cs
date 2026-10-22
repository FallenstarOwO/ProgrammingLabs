using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class FcfsForm : ExperimentFormBase
{
    private readonly DataGridView _grid;

    public FcfsForm()
        : base("先来先服务 FCFS", "按进程进入就绪队列的先后顺序调度，每个时间片推进 1 秒。")
    {
        _grid = Theme.Grid();
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "进程名",
            FillWeight = 45
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Total",
            HeaderText = "总需时间（秒）",
            FillWeight = 55
        });
        ResetRows();

        var hint = new Label
        {
            Text = "可以直接编辑表格，也可以在最后一行新增进程。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 0, 0, 4)
        };
        AddInput(hint);

        var gridPanel = new Panel { Dock = DockStyle.Fill, Height = 190 };
        gridPanel.Controls.Add(_grid);
        AddInput(gridPanel);

        var start = Theme.PrimaryButton("开始调度");
        var reset = Theme.SecondaryButton("重置数据");
        var clear = Theme.SecondaryButton("清空输出");
        start.Click += async (_, _) => await RunWithCancellationAsync(start, SimulateAsync);
        reset.Click += (_, _) =>
        {
            ResetRows();
            Output.Clear();
        };
        clear.Click += (_, _) => Output.Clear();
        AddToolbar(start, reset, clear);
    }

    protected override int InitialInputHeight => 230;

    private void ResetRows()
    {
        _grid.Rows.Clear();
        _grid.Rows.Add("P1", 5);
        _grid.Rows.Add("P2", 3);
        _grid.Rows.Add("P3", 2);
    }

    private List<ProcessState> ReadProcesses()
    {
        var processes = new List<ProcessState>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.IsNewRow)
                continue;

            string name = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
            string totalText = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty;
            if (name.Length == 0 && totalText.Length == 0)
                continue;
            if (name.Length == 0)
                throw new ArgumentException($"第 {row.Index + 1} 行：进程名不能为空。");
            if (!int.TryParse(totalText, out int total) || total <= 0)
                throw new ArgumentException($"第 {row.Index + 1} 行：总需时间必须是正整数。");

            processes.Add(new ProcessState(name, total));
        }

        if (processes.Count == 0)
            throw new ArgumentException("至少需要一个进程。");
        return processes;
    }

    private async Task SimulateAsync(CancellationToken token)
    {
        List<ProcessState> processes = ReadProcesses();
        var queue = new Queue<ProcessState>(processes);

        Output.Clear();
        Theme.AppendLine(Output, $"时间片：1 秒    进程数：{processes.Count}");
        Theme.AppendLine(Output);
        Dump(queue, 0);

        int time = 0;
        while (queue.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            time++;

            ProcessState current = queue.Peek();
            current.State = "运行";
            current.RunTime++;
            current.NeedTime--;

            if (current.NeedTime > 0)
            {
                current.State = "等待";
                Dump(queue, time);
            }
            else
            {
                current.NeedTime = 0;
                current.State = "完成";
                Dump(queue, time);
                Theme.AppendLine(Output, $"{current.Name} 已经完成。");
                Theme.AppendLine(Output);
                queue.Dequeue();
            }

            await Task.Delay(220, token);
        }

        Theme.AppendLine(Output, $"进程全部完成，共使用 {time} 秒。");
    }

    private void Dump(IEnumerable<ProcessState> processes, int time)
    {
        Theme.AppendLine(Output, $"当前时间：{time} 秒");
        Theme.AppendHeader(Output, "进程名称", "总需时间", "运行时间", "剩余时间", "状态");
        foreach (ProcessState process in processes)
        {
            Theme.AppendLine(Output,
                $"{process.Name,-12} {process.TotalTime,8} {process.RunTime,8} {process.NeedTime,8}  {process.State}");
        }
        Theme.AppendLine(Output);
    }

    private sealed class ProcessState
    {
        public ProcessState(string name, int totalTime)
        {
            Name = name;
            TotalTime = totalTime;
            NeedTime = totalTime;
            State = "等待";
        }

        public string Name { get; }
        public int TotalTime { get; }
        public int RunTime { get; set; }
        public int NeedTime { get; set; }
        public string State { get; set; }
    }
}