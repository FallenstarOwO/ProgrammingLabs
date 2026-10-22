using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class SjfForm : ExperimentFormBase
{
    private readonly DataGridView _grid;
    private readonly NumericUpDown _startHour;
    private readonly NumericUpDown _startMinute;
    private readonly NumericUpDown _memory;
    private readonly NumericUpDown _tapes;

    public SjfForm()
        : base("短作业优先 SJF", "选择运行时间最短且资源满足的后备作业投入运行，模拟内存与磁带机的分配和回收。")
    {
        _startHour = NumberBox(0, 23, 8, 60);
        _startMinute = NumberBox(0, 59, 0, 60);
        _memory = NumberBox(1, 1000, 100, 90);
        _tapes = NumberBox(1, 30, 5, 70);

        AddLabeledRow("当前时间", Row(
            _startHour, InlineLabel("时"),
            _startMinute, InlineLabel("分")), 110);
        AddLabeledRow("系统资源", Row(
            InlineLabel("内存"), _memory,
            InlineLabel("磁带机"), _tapes), 110);

        _grid = Theme.Grid();
        _grid.Columns.Add(TextColumn("User", "用户名", 18));
        _grid.Columns.Add(TextColumn("Name", "作业名", 18));
        _grid.Columns.Add(TextColumn("Hour", "提交时", 12));
        _grid.Columns.Add(TextColumn("Minute", "提交分", 12));
        _grid.Columns.Add(TextColumn("Total", "总用时(分)", 15));
        _grid.Columns.Add(TextColumn("Memory", "内存", 12));
        _grid.Columns.Add(TextColumn("Tapes", "磁带机", 13));
        ResetRows();

        var hint = new Label
        {
            Text = "作业在提交时间到达后进入后备队列；每一步只允许新投入一个作业，但多个作业可以同时执行。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 4, 0, 4)
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

    protected override int InitialInputHeight => 330;

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
        _grid.Rows.Add("A", "J1", 8, 0, 5, 30, 2);
        _grid.Rows.Add("B", "J2", 8, 2, 3, 20, 1);
        _grid.Rows.Add("C", "J3", 8, 4, 2, 50, 2);
        _grid.Rows.Add("D", "J4", 8, 6, 4, 25, 1);
    }

    private List<Job> ReadJobs(int totalMemory, int totalTapes)
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

            string user = Theme.ReadGridText(row, 0, "用户名");
            string name = Theme.ReadGridText(row, 1, "作业名");
            int hour = Theme.ReadGridInt(row, 2, "提交时");
            int minute = Theme.ReadGridInt(row, 3, "提交分");
            int total = Theme.ReadGridInt(row, 4, "总用时");
            int memory = Theme.ReadGridInt(row, 5, "内存");
            int tapes = Theme.ReadGridInt(row, 6, "磁带机");

            if (hour is < 0 or > 23)
                throw new ArgumentException($"第 {row.Index + 1} 行：提交时必须 0–23。");
            if (minute is < 0 or > 59)
                throw new ArgumentException($"第 {row.Index + 1} 行：提交分必须 0–59。");
            if (total <= 0)
                throw new ArgumentException($"第 {row.Index + 1} 行：总用时必须是正整数。");
            if (memory <= 0 || memory > totalMemory)
                throw new ArgumentException($"第 {row.Index + 1} 行：内存必须在 1–{totalMemory} 之间。");
            if (tapes < 0 || tapes > totalTapes)
                throw new ArgumentException($"第 {row.Index + 1} 行：磁带机必须在 0–{totalTapes} 之间。");

            jobs.Add(new Job(user, name, hour * 60 + minute, total, memory, tapes));
        }

        if (jobs.Count == 0)
            throw new ArgumentException("至少需要一个作业。");
        return jobs;
    }

    private async Task SimulateAsync(CancellationToken token)
    {
        int totalMemory = (int)_memory.Value;
        int totalTapes = (int)_tapes.Value;
        int current = (int)_startHour.Value * 60 + (int)_startMinute.Value;

        List<Job> jobs = ReadJobs(totalMemory, totalTapes);
        int finished = 0;
        int nextOrder = 1;

        Output.Clear();
        Theme.AppendLine(Output, $"当前时间：{Theme.TimeText(current)}    内存：{totalMemory}    磁带机：{totalTapes}");
        Theme.AppendLine(Output);

        MarkReady(jobs, current);
        Dump(jobs, current);

        while (finished < jobs.Count)
        {
            token.ThrowIfCancellationRequested();
            current++;
            MarkReady(jobs, current);

            int freeMemory = totalMemory - jobs.Where(j => j.State == "执行").Sum(j => j.Memory);
            int freeTapes = totalTapes - jobs.Where(j => j.State == "执行").Sum(j => j.Tapes);

            Job? candidate = jobs
                .Where(j => j.State == "后备" && j.Memory <= freeMemory && j.Tapes <= freeTapes)
                .OrderBy(j => j.NeedTime)
                .ThenBy(j => j.SubmitMinutes)
                .FirstOrDefault();

            if (candidate is not null)
            {
                candidate.State = "执行";
                if (candidate.Order == 0)
                    candidate.Order = nextOrder++;
            }

            var completed = new List<Job>();
            foreach (Job job in jobs.Where(j => j.State == "执行"))
            {
                job.NeedTime--;
                if (job.NeedTime <= 0)
                {
                    job.NeedTime = 0;
                    job.State = "完成";
                    finished++;
                    completed.Add(job);
                }
            }

            Dump(jobs, current);
            foreach (Job job in completed)
                Theme.AppendLine(Output, $"用户 {job.User} 的作业 {job.Name} 已完成。");
            if (completed.Count > 0)
                Theme.AppendLine(Output);

            await Task.Delay(220, token);

            if (finished < jobs.Count && !jobs.Any(j => j.State is "执行" or "后备"))
            {
                int nextSubmit = jobs.Where(j => j.State == "未进入").Min(j => j.SubmitMinutes);
                if (nextSubmit > current)
                {
                    current = nextSubmit;
                    MarkReady(jobs, current);
                }
            }
        }

        Theme.AppendLine(Output, "作业全部完成。");
    }

    private static void MarkReady(IEnumerable<Job> jobs, int current)
    {
        foreach (Job job in jobs)
        {
            if (job.State == "未进入" && job.SubmitMinutes <= current)
                job.State = "后备";
        }
    }

    private void Dump(IEnumerable<Job> jobs, int current)
    {
        Theme.AppendLine(Output, $"当前时间：{Theme.TimeText(current)}");
        Theme.AppendHeader(Output,
            "用户名", "作业名", "状态", "次序", "提交时间", "总需", "剩余", "内存", "磁带机");
        foreach (Job job in jobs)
        {
            Theme.AppendLine(Output,
                $"{job.User,-8} {job.Name,-8} {job.State,-6} {job.Order,4}  {Theme.TimeText(job.SubmitMinutes),8} " +
                $"{job.TotalTime,6} {job.NeedTime,6} {job.Memory,6} {job.Tapes,7}");
        }
        Theme.AppendLine(Output);
    }

    private sealed class Job
    {
        public Job(string user, string name, int submitMinutes, int totalTime, int memory, int tapes)
        {
            User = user;
            Name = name;
            SubmitMinutes = submitMinutes;
            TotalTime = totalTime;
            NeedTime = totalTime;
            Memory = memory;
            Tapes = tapes;
            State = "未进入";
        }

        public string User { get; }
        public string Name { get; }
        public int SubmitMinutes { get; }
        public int TotalTime { get; }
        public int NeedTime { get; set; }
        public int Memory { get; }
        public int Tapes { get; }
        public string State { get; set; }
        public int Order { get; set; }
    }
}