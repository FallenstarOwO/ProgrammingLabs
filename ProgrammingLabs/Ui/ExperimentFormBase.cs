namespace ProgrammingLabs.Ui;

internal abstract class ExperimentFormBase : Form
{
    private CancellationTokenSource? _cancellation;
    private int _inputRow;

    protected TableLayoutPanel InputArea { get; }
    protected RichTextBox Output { get; }
    protected FlowLayoutPanel Toolbar { get; }
    private readonly SplitContainer _bodySplit;

    protected ExperimentFormBase(string title, string subtitle)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1040, 720);
        MinimumSize = new Size(860, 600);
        BackColor = Theme.Background;
        Font = Theme.BaseFont;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18),
            BackColor = Theme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(title, subtitle), 0, 0);

        Toolbar = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.Background,
            Margin = new Padding(0)
        };
        root.Controls.Add(Toolbar, 0, 1);

        _bodySplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 6,
            Panel1MinSize = 0,
            Panel2MinSize = 0,
            BackColor = Theme.Background
        };

        InputArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Theme.Background,
            Padding = new Padding(0, 0, 0, 6),
            Margin = new Padding(0)
        };
        InputArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _bodySplit.Panel1.Controls.Add(InputArea);

        Output = Theme.OutputBox();
        Output.Margin = new Padding(0);
        _bodySplit.Panel2.Controls.Add(Output);

        root.Controls.Add(_bodySplit, 0, 2);
        Controls.Add(root);
    }

    protected virtual int InitialInputHeight => 240;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // 等窗口完成第一次布局后再设置分割位置，避免 Panel1MinSize/Panel2MinSize 校验失败。
        BeginInvoke(new Action(ApplyInitialSplitterDistance));
    }

    private void ApplyInitialSplitterDistance()
    {
        if (IsDisposed || _bodySplit.IsDisposed)
            return;

        try
        {
            const int panel1Min = 90;
            const int panel2Min = 120;
            int height = _bodySplit.Height;
            int max = height - panel2Min - _bodySplit.SplitterWidth;
            if (max <= panel1Min)
                return;

            // 先设置分割位置（此时最小尺寸仍为 0），再设置最小尺寸，避免控件校验冲突。
            _bodySplit.SplitterDistance = Math.Clamp(InitialInputHeight, panel1Min, max);
            _bodySplit.Panel1MinSize = panel1Min;
            _bodySplit.Panel2MinSize = panel2Min;
        }
        catch (ArgumentException)
        {
            // 窗口尺寸过小或高 DPI 下暂时无法设置，保留默认分割位置。
        }
        catch (InvalidOperationException)
        {
            // 控件尚未完成布局时忽略。
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellation?.Cancel();
        base.OnFormClosing(e);
    }

    protected void AddInput(Control control, int height = 0)
    {
        if (_inputRow >= InputArea.RowCount)
            InputArea.RowCount = _inputRow + 1;

        InputArea.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 3, 0, 3);
        if (height > 0)
        {
            control.Height = height;
            control.MinimumSize = new Size(0, height);
        }
        InputArea.Controls.Add(control, 0, _inputRow++);
    }

    protected void AddLabeledRow(string label, Control control, int labelWidth = 120)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            BackColor = Theme.Background,
            Margin = new Padding(0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var labelControl = new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 7, 8, 0)
        };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 4, 0, 4);

        row.Controls.Add(labelControl, 0, 0);
        row.Controls.Add(control, 1, 0);
        AddInput(row);
    }

    protected void AddToolbar(params Control[] controls)
    {
        Toolbar.Controls.AddRange(controls);
    }

    protected async Task RunWithCancellationAsync(Button button, Func<CancellationToken, Task> action)
    {
        if (_cancellation is not null)
        {
            _cancellation.Cancel();
            return;
        }

        using var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;
        string originalText = button.Text;
        button.Text = "停止";

        try
        {
            await action(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            Theme.AppendLine(Output, "已停止。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "运行出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cancellation = null;
            button.Text = originalText;
        }
    }

    protected static Control BuildHeader(string title, string subtitle)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            BackColor = Theme.Background,
            Margin = new Padding(0, 0, 0, 4)
        };
        panel.Controls.Add(Theme.Title(title), 0, 0);
        panel.Controls.Add(Theme.Subtitle(subtitle), 0, 1);
        return panel;
    }

    protected static FlowLayoutPanel Row(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = true,
            BackColor = Theme.Background,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        panel.Controls.AddRange(controls);
        return panel;
    }
}