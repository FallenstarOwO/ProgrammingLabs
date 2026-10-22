using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class HrrnForm : ExperimentFormBase
{
    private readonly DataGridView _grid;
    private readonly NumericUpDown _startHour;
    private readonly NumericUpDown _startMinute;

    public HrrnForm()
        : base("高响应比优先 HRRN", "按响应比选择作业：响应比 =（等待时间 + 要求服务时间）/ 要求服务时间。")
    {
        _startHour = NumberBox(0, 23, 8, 60);
        _startMinute = NumberBox(0, 59, 0, 60);
        AddLabeledRow("当前时间", Row(
            _startHour, InlineLabel("时"),
            _startMinute, InlineLabel("分")), 110);

        _grid = Theme.Grid();
        _grid.Columns.Add(TextColumn("Name", "作业名", 28));
        _grid.Columns.Add(TextColumn("Hour", "提交时", 18));
        _grid.Columns.Add(TextColumn("Minute", "提交分", 18));
        _grid.Columns.Add(TextColumn("Total", "总用时(分)", 22));
        ResetRows();

        var hint = new Label
        {
            Text = "每次选择一个作业连续运行到完成；等待中的作业会随时间增加等待时间并重新计算响应比。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 4, 0, 4)
        };
        AddInput(hint);

        var gridPanel = new Panel { Dock = DockStyle.Fill, Height = 180 };
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

    protected override int InitialInputHeight => 280;

    private static NumericUpDown NumberBox(int min, int max, int value, int width) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        Width = width,
        Font = Theme.BaseFont,
        Margin = new Padding(0, 4, 8, 4)
    };

    private static Label InlineLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Theme.Muted,
        Margin = new Padding(0, 8, 8, 4)
    };

    private static DataGridViewTextBoxColumn TextColumn(string name, string header, int weight) => new()
    {
        Name = name,
        HeaderText = header,
        FillWeight = weight
    };

    private void ResetRows()
    {
        _grid.Rows.Clear();
        _grid.Rows.Add("J1", 8, 0, 5);
        _grid.Rows.Add("J2", 8, 1, 3);
        _grid.Rows.Add("J3", 8, 2, 2);
        _grid.Rows.Add("J4", 8, 3, 4);
    }

    private List<Job> ReadJobs()
    {
        var jobs = new List<Job>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.IsNewRow)
                continue;

            bool allEmpty = true;
            for (int i = 0; i < row.Cells.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(row.Cells[i].Value?.ToString()))
                {
                    allEmpty = false;
                    break;
                }
            }
            if (allEmpty)
                continue;

            string name = Theme.ReadGridText(row, 0, "作业名");
            int hour = Theme.ReadGridInt(row, 1, "提交时");
            int minute = Theme.ReadGridInt(row, 2, "提交分");
            int total = Theme.ReadGridInt(row, 3, "总用时");

            if (hour is < 0 or > 23)
                throw new ArgumentException($"第 {row.Index + 1} 行：提交时必须 0–23。");
            if (minute is < 0 or > 59)
                throw new ArgumentException($"第 {row.Index + 1} 行：提交分必须 0–59。");
            if (total <= 0)
                throw new ArgumentException($"第 {row.Index + 1} 行：总用时必须是正整数。");

            jobs.Add(new Job(name, hour * 60 + minute, total));
        }

        if (jobs.Count == 0)
            throw new ArgumentException("至少需要一个作业。");
        return jobs;
    }

    private async Task SimulateAsync(CancellationToken token)
    {
        int current = (int)_startHour.Value * 60 + (int)_startMinute.Value;
        List<Job> jobs = ReadJobs();
        int finished = 0;

        Output.Clear();
        Theme.AppendLine(Output, $"当前时间：{Theme.TimeText(current)}");
        Theme.AppendLine(Output);

        MarkReady(jobs, current);
        Dump(jobs, current);

        while (finished < jobs.Count)
        {
            token.ThrowIfCancellationRequested();

            if (!jobs.Any(j => j.State is "后备" or "执行"))
            {
                int nextSubmit = jobs.Where(j => j.State == "未进入").Min(j => j.SubmitMinutes);
                current = Math.Max(current + 1, nextSubmit);
                MarkReady(jobs, current);
                Dump(jobs, current);
            }

            Job? selected = jobs
                .Where(j => j.State == "后备")
                .OrderByDescending(j => j.Priority)
                .ThenBy(j => j.SubmitMinutes)
                .FirstOrDefault();

            if (selected is null)
            {
                current++;
                MarkReady(jobs, current);
                continue;
            }

            selected.State = "执行";
            Theme.AppendLine(Output, $">>> 选择 {selected.Name} 运行（响应比 {selected.Priority:F2}）");
            Theme.AppendLine(Output);

            while (selected.NeedTime > 0)
            {
                token.ThrowIfCancellationRequested();
                selected.NeedTime--;
                current++;
                MarkReady(jobs, current);

                foreach (Job waiting in jobs.Where(j => j.State == "后备"))
                {
                    waiting.WaitTime++;
                    waiting.Priority = waiting.NeedTime > 0
                        ? (waiting.WaitTime + waiting.NeedTime) / (double)waiting.NeedTime
                        : 0;
                }

                if (selected.NeedTime == 0)
                {
                    selected.State = "完成";
                    selected.FinishMinutes = current;
                    selected.Turnaround = selected.WaitTime + selected.TotalTime;
                    selected.WeightedTurnaround = (double)selected.Turnaround / selected.TotalTime;
                    selected.Priority = 0;
                    finished++;
                    Theme.AppendLine(Output,
                        $"{selected.Name} 已完成：完成时间 {Theme.TimeText(current)}，" +
                        $"周转时间 {selected.Turnaround} 分钟，带权周转时间 {selected.WeightedTurnaround:F2}");
                    Theme.AppendLine(Output);
                }

                Dump(jobs, current);
                await Task.Delay(220, token);
            }
        }

        double averageTurnaround = jobs.Average(j => j.Turnaround);
        double averageWeighted = jobs.Average(j => j.WeightedTurnaround);
        Theme.AppendLine(Output, "本次作业全部完成。");
        Theme.AppendLine(Output, $"平均周转时间：{averageTurnaround:F2} 分钟");
        Theme.AppendLine(Output, $"平均带权周转时间：{averageWeighted:F2}");
    }

    private static void MarkReady(IEnumerable<Job> jobs, int current)
    {
        foreach (Job job in jobs)
        {
            if (job.State == "未进入" && job.SubmitMinutes <= current)
            {
                job.State = "后备";
                job.Priority = 1;
            }
        }
    }

    private void Dump(IEnumerable<Job> jobs, int current)
    {
        Theme.AppendLine(Output, $"当前时间：{Theme.TimeText(current)}");
        Theme.AppendHeader(Output,
            "作业名称", "状态", "优先权", "提交时间", "完成时间", "已运行", "剩余");
        foreach (Job job in jobs)
        {
            string finish = job.FinishMinutes >= 0 ? Theme.TimeText(job.FinishMinutes) : "--:--";
            Theme.AppendLine(Output,
                $"{job.Name,-10} {job.State,-6} {job.Priority,8:F2}  {Theme.TimeText(job.SubmitMinutes),8} " +
                $"{finish,8} {job.TotalTime - job.NeedTime,7} {job.NeedTime,6}");
        }
        Theme.AppendLine(Output);
    }

    private sealed class Job
    {
        public Job(string name, int submitMinutes, int totalTime)
        {
            Name = name;
            SubmitMinutes = submitMinutes;
            TotalTime = totalTime;
            NeedTime = totalTime;
            State = "未进入";
            FinishMinutes = -1;
        }

        public string Name { get; }
        public int SubmitMinutes { get; }
        public int TotalTime { get; }
        public int NeedTime { get; set; }
        public int WaitTime { get; set; }
        public double Priority { get; set; }
        public string State { get; set; }
        public int FinishMinutes { get; set; }
        public int Turnaround { get; set; }
        public double WeightedTurnaround { get; set; }
    }
}