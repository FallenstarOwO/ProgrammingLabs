namespace ProgrammingLabs.Ui;

internal static class Theme
{
    public static readonly Font BaseFont = new("Microsoft YaHei UI", 10F);
    public static readonly Font SmallFont = new("Microsoft YaHei UI", 9F);
    public static readonly Font HeaderFont = new("Microsoft YaHei UI", 20F, FontStyle.Bold);
    public static readonly Font SectionFont = new("Microsoft YaHei UI", 11F, FontStyle.Bold);
    public static readonly Font MonoFont = new("Consolas", 10F);

    public static readonly Color Background = Color.FromArgb(246, 247, 249);
    public static readonly Color Card = Color.White;
    public static readonly Color Accent = Color.FromArgb(47, 111, 237);
    public static readonly Color AccentDark = Color.FromArgb(31, 86, 200);
    public static readonly Color Text = Color.FromArgb(28, 32, 38);
    public static readonly Color Muted = Color.FromArgb(96, 102, 112);
    public static readonly Color Border = Color.FromArgb(220, 224, 230);
    public static readonly Color GridHeader = Color.FromArgb(242, 244, 248);
    public static readonly Color Selection = Color.FromArgb(226, 236, 255);
    public static readonly Color Success = Color.FromArgb(22, 140, 90);
    public static readonly Color Warning = Color.FromArgb(190, 120, 20);
    public static readonly Color Error = Color.FromArgb(200, 50, 50);

    public static Label Title(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = HeaderFont,
        ForeColor = Text,
        Margin = new Padding(0, 0, 0, 2)
    };

    public static Label Subtitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = SmallFont,
        ForeColor = Muted,
        Margin = new Padding(2, 0, 0, 0)
    };

    public static Label SectionLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = SectionFont,
        ForeColor = Text,
        Margin = new Padding(0, 2, 0, 6)
    };

    public static Button PrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(112, 36),
            Padding = new Padding(14, 0, 14, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Accent,
            ForeColor = Color.White,
            Font = BaseFont,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = AccentDark;
        return button;
    }

    public static Button SecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(96, 36),
            Padding = new Padding(12, 0, 12, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Card,
            ForeColor = Text,
            Font = BaseFont,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 242, 249);
        return button;
    }

    public static TextBox InputBox(int width = 180) => new()
    {
        Width = width,
        Font = BaseFont,
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 4, 12, 4)
    };

    public static RichTextBox OutputBox()
    {
        var box = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = Color.White,
            ForeColor = Text,
            BorderStyle = BorderStyle.FixedSingle,
            Font = MonoFont,
            DetectUrls = false,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both
        };
        return box;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            EditMode = DataGridViewEditMode.EditOnEnter,
            Font = BaseFont,
            EnableHeadersVisualStyles = false,
            GridColor = Border,
            ColumnHeadersHeight = 34,
            RowTemplate = { Height = 30 },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridHeader,
                ForeColor = Text,
                Font = new Font(BaseFont, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = GridHeader,
                SelectionForeColor = Text
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Selection,
                SelectionForeColor = Text
            }
        };
        return grid;
    }

    public static Panel CardPanel(Control content, Padding? padding = null)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Card,
            Padding = padding ?? new Padding(16),
            Margin = new Padding(0)
        };
        panel.Controls.Add(content);
        return panel;
    }

    public static void AppendLine(RichTextBox box, string text = "")
    {
        box.AppendText(text + Environment.NewLine);
    }

    public static void AppendHeader(RichTextBox box, params string[] columns)
    {
        box.AppendText(string.Join("  ", columns) + Environment.NewLine);
        box.AppendText(new string('-', 78) + Environment.NewLine);
    }

    public static string TimeText(int totalMinutes)
    {
        totalMinutes = ((totalMinutes % 1440) + 1440) % 1440;
        return $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
    }

    public static int ParseInt(string text, string fieldName, int min = int.MinValue)
    {
        if (!int.TryParse(text.Trim(), out int value))
            throw new ArgumentException($"{fieldName} 必须是整数。");
        if (value < min)
            throw new ArgumentException($"{fieldName} 不能小于 {min}。");
        return value;
    }

    public static int ReadGridInt(DataGridViewRow row, int columnIndex, string fieldName)
    {
        object? value = row.Cells[columnIndex].Value;
        if (value is null || !int.TryParse(value.ToString(), out int result))
            throw new ArgumentException($"第 {row.Index + 1} 行：{fieldName} 必须是整数。");
        return result;
    }

    public static string ReadGridText(DataGridViewRow row, int columnIndex, string fieldName)
    {
        object? value = row.Cells[columnIndex].Value;
        string text = value?.ToString()?.Trim() ?? string.Empty;
        if (text.Length == 0)
            throw new ArgumentException($"第 {row.Index + 1} 行：{fieldName} 不能为空。");
        return text;
    }
}