using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class RrForm : ExperimentFormBase
{
    private readonly DataGridView _grid;
    private readonly NumericUpDown _slice;

    public RrForm()
        : base("简单轮转法 RR", "时间片轮转调度：进程用完时间片后回到队尾，直到全部完成。")
    {
        _slice = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 20,
            Value = 2,
            Width = 80,
            Font = Theme.BaseFont,
            Margin = new Padding(0, 4, 12, 4)
        };
        AddLabeledRow("时间片（秒）", _slice, 120);

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
            Margin = new Padding(0, 4, 0, 4)
        };
        AddInput(hint);

        var gridPanel = new Panel { Dock = DockStyle.Fill, Height = 170 };
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

    protected override int InitialInputHeight => 260;

    private void ResetRows()
    {
        _grid.Rows.Clear();
        _grid.Rows.Add("P1", 5);
        _grid.Rows.Add("P2", 4);
        _grid.Rows.Add("P3", 3);
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
        List<ProcessState> all = ReadProcesses();
        var queue = new Queue<ProcessState>(all);
        int slice = (int)_slice.Value;
        int time = 0;

        Output.Clear();
        Theme.AppendLine(Output, $"时间片：{slice} 秒    进程数：{all.Count}");
        Theme.AppendLine(Output);
        Dump(all, time, null);

        while (queue.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            ProcessState current = queue.Dequeue();
            int run = Math.Min(slice, current.NeedTime);

            current.State = "运行";
            current.RunTime += run;
            current.NeedTime -= run;
            time += run;

            if (current.NeedTime > 0)
            {
                current.State = "等待";
                queue.Enqueue(current);
                Dump(all, time, current);
            }
            else
            {
                current.NeedTime = 0;
                current.State = "完成";
                Dump(all, time, current);
                Theme.AppendLine(Output, $"{current.Name} 已经完成。");
                Theme.AppendLine(Output);
            }

            await Task.Delay(260, token);
        }

        Theme.AppendLine(Output, $"进程全部完成，共使用 {time} 秒。");
    }

    private void Dump(IEnumerable<ProcessState> processes, int time, ProcessState? current)
    {
        Theme.AppendLine(Output, current is null
            ? $"初始状态    当前时间：{time} 秒"
            : $"当前运行：{current.Name}    当前时间：{time} 秒");
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