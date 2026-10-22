using ProgrammingLabs.Experiments;
using ProgrammingLabs.Ui;

namespace ProgrammingLabs;

public sealed class MainForm : Form
{
    private readonly TreeView _tree = new();
    private readonly Label _detailTitle = new();
    private readonly Label _detailDescription = new();
    private readonly Button _openButton;
    private readonly Font _categoryFont;
    private ExperimentDescriptor? _selected;

    public MainForm()
    {
        Text = "大学实验合集 · C# 版";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1060, 700);
        MinimumSize = new Size(880, 580);
        BackColor = Theme.Background;
        Font = Theme.BaseFont;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        _openButton = Theme.PrimaryButton("打开实验 ▶");
        _openButton.Enabled = false;
        _openButton.Click += (_, _) => OpenSelected();
        _categoryFont = new Font(Theme.BaseFont, FontStyle.Bold);

        BuildLayout();
        PopulateTree();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(22),
            BackColor = Theme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            BackColor = Theme.Background,
            Margin = new Padding(0, 0, 0, 14)
        };
        header.Controls.Add(Theme.Title("大学实验合集"), 0, 0);
        header.Controls.Add(Theme.Subtitle("C# / WinForms 版 · 操作系统、数据结构与密码学 · 共 10 个实验"), 0, 1);
        root.Controls.Add(header, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Theme.Background,
            Margin = new Padding(0)
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _tree.Dock = DockStyle.Fill;
        _tree.BorderStyle = BorderStyle.None;
        _tree.BackColor = Theme.Card;
        _tree.ForeColor = Theme.Text;
        _tree.Font = new Font(Theme.BaseFont.FontFamily, 10.5F);
        _tree.FullRowSelect = true;
        _tree.HideSelection = false;
        _tree.ShowLines = false;
        _tree.ShowPlusMinus = true;
        _tree.ShowRootLines = false;
        _tree.ItemHeight = 30;
        _tree.AfterSelect += TreeAfterSelect;
        _tree.BeforeSelect += TreeBeforeSelect;
        _tree.NodeMouseDoubleClick += TreeDoubleClick;

        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Card
        };
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftLayout.Controls.Add(Theme.SectionLabel("实验分类"), 0, 0);
        leftLayout.Controls.Add(_tree, 0, 1);

        var leftCard = Theme.CardPanel(leftLayout, new Padding(14));
        leftCard.Margin = new Padding(0, 0, 14, 0);
        content.Controls.Add(leftCard, 0, 0);

        _detailTitle.Dock = DockStyle.Top;
        _detailTitle.AutoSize = true;
        _detailTitle.Font = new Font(Theme.BaseFont.FontFamily, 17F, FontStyle.Bold);
        _detailTitle.ForeColor = Theme.Text;
        _detailTitle.Text = "选择一个实验";
        _detailTitle.Margin = new Padding(0, 0, 0, 10);

        _detailDescription.Dock = DockStyle.Fill;
        _detailDescription.AutoSize = false;
        _detailDescription.Font = Theme.BaseFont;
        _detailDescription.ForeColor = Theme.Muted;
        _detailDescription.TextAlign = ContentAlignment.TopLeft;
        _detailDescription.Text = "左侧按“处理机调度 / 内存管理 / 哈希表 / 密码学与数论”分类。\r\n\r\n" +
                                  "选中一个实验后，这里会显示说明；点击右下角“打开实验”即可启动。\r\n" +
                                  "每个实验都在独立窗口中运行，可以同时打开多个实验进行对比。";

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Card,
            Margin = new Padding(0, 12, 0, 0)
        };
        buttonRow.Controls.Add(_openButton);

        var hint = new Label
        {
            Text = "提示：所有实验均为 C# 重写版，不依赖 C++ 编译器。",
            AutoSize = true,
            Font = Theme.SmallFont,
            ForeColor = Theme.Muted,
            Margin = new Padding(0, 10, 0, 0)
        };

        var rightLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Theme.Card
        };
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.Controls.Add(_detailTitle, 0, 0);
        rightLayout.Controls.Add(_detailDescription, 0, 1);
        rightLayout.Controls.Add(buttonRow, 0, 2);
        rightLayout.Controls.Add(hint, 0, 3);

        var rightCard = Theme.CardPanel(rightLayout, new Padding(26));
        content.Controls.Add(rightCard, 1, 0);

        root.Controls.Add(content, 0, 1);
        Controls.Add(root);
    }

    private void PopulateTree()
    {
        TreeNode? categoryNode = null;
        string? currentCategory = null;

        foreach (ExperimentDescriptor experiment in ExperimentCatalog.All)
        {
            if (currentCategory != experiment.Category)
            {
                categoryNode = new TreeNode(experiment.Category)
                {
                    NodeFont = _categoryFont,
                    ForeColor = Theme.Text
                };
                _tree.Nodes.Add(categoryNode);
                currentCategory = experiment.Category;
            }

            var experimentNode = new TreeNode(experiment.Name)
            {
                Tag = experiment,
                ForeColor = Theme.Text
            };
            categoryNode!.Nodes.Add(experimentNode);
        }

        _tree.ExpandAll();

        if (_tree.Nodes.Count > 0 && _tree.Nodes[0].Nodes.Count > 0)
            _tree.SelectedNode = _tree.Nodes[0].Nodes[0];
    }

    private void TreeAfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is ExperimentDescriptor experiment)
        {
            _selected = experiment;
            _detailTitle.Text = experiment.Name;
            _detailDescription.Text = experiment.Description;
            _openButton.Enabled = true;
        }
        else
        {
            _selected = null;
            _detailTitle.Text = "选择一个实验";
            _detailDescription.Text = "展开左侧分类，选择一个实验查看说明。";
            _openButton.Enabled = false;
        }
    }

    private void TreeBeforeSelect(object? sender, TreeViewCancelEventArgs e)
    {
        // 分类标题只用于分组，不允许被选中；点击标题不改变当前实验。
        if (e.Node?.Tag is not ExperimentDescriptor)
            e.Cancel = true;
    }

    private void TreeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is ExperimentDescriptor experiment)
        {
            _tree.SelectedNode = e.Node;
            OpenExperiment(experiment);
        }
    }

    private void OpenSelected()
    {
        if (_selected is not null)
            OpenExperiment(_selected);
    }

    private void OpenExperiment(ExperimentDescriptor experiment)
    {
        try
        {
            Form form = experiment.Factory();
            form.Show(this);
        }
        catch (Exception ex)
        {
            try
            {
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "open-error.log"), ex.ToString());
            }
            catch
            {
                // 日志写入失败时仍然弹出提示。
            }
            MessageBox.Show(this, ex.Message, "无法打开实验", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // 窗口显示后再确保默认选中第一个实验，避免 TreeView 首次获得焦点时选中分类标题。
        if (_tree.Nodes.Count > 0 && _tree.Nodes[0].Nodes.Count > 0)
            _tree.SelectedNode = _tree.Nodes[0].Nodes[0];
        _tree.Focus();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _categoryFont.Dispose();
        base.OnFormClosed(e);
    }
}