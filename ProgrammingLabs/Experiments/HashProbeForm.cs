using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class HashProbeForm : ExperimentFormBase
{
    private const int HashSize = 51;
    private const int MaxNameLength = 19;

    private static readonly string[] SampleNames =
    {
        "Chen Xuqing", "Chen Zehan", "Chen ZuoQi", "Guo Xu", "He Huaxian",
        "He Zhuoqian", "Huang Xiaobin", "Huang Yucheng", "HuangZikai", "Li Jiajie",
        "Li Shu", "Li Zhiheng", "Liang Huachao", "Lin Jiancheng", "Lin Qikai",
        "Liu Haigang", "Lu Minkai", "Lu Zichuang", "Shao Weiyuan", "Shen Chunlin",
        "Wang Kai", "Wei Zhifeng", "Wu Tanghuang", "Xia Xiang", "Yang ZhaoXiong",
        "Zhang Bingkun", "Zhang Guojun", "Zhen Zhuoan", "Zhong Citing", "Guo Pengyan"
    };

    private readonly Node?[] _table = new Node?[HashSize];
    private readonly TextBox _nameBox;

    public HashProbeForm()
        : base("开放定址哈希表", "内置 30 个姓名，使用线性探测处理冲突；统计每个节点的查找长度和平均查找长度。")
    {
        _nameBox = Theme.InputBox(260);
        _nameBox.PlaceholderText = "输入姓名，例如 Zhang San";
        AddLabeledRow("姓名", _nameBox, 70);

        var hint = new Label
        {
            Text = "哈希函数：跳过空格累加字符编码，除以 3 后对 51 取余；冲突时向后线性探测。删除节点使用墓碑标记。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 4, 0, 0)
        };
        AddInput(hint);

        var insert = Theme.PrimaryButton("插入");
        var delete = Theme.SecondaryButton("删除");
        var search = Theme.SecondaryButton("查找");
        var show = Theme.SecondaryButton("显示全部");
        var sample = Theme.SecondaryButton("重建示例");
        var clear = Theme.SecondaryButton("清空");
        insert.Click += (_, _) => Insert();
        delete.Click += (_, _) => Delete();
        search.Click += (_, _) => Search();
        show.Click += (_, _) => ShowTable();
        sample.Click += (_, _) => LoadSample();
        clear.Click += (_, _) => ClearAll();
        AddToolbar(insert, delete, search, show, sample, clear);

        LoadSample();
    }

    protected override int InitialInputHeight => 170;

    private static int Hash(string name)
    {
        int sum = 0;
        int i = 0;
        while (i < name.Length)
        {
            if (name[i] == ' ')
            {
                i += 2;
                continue;
            }
            sum += name[i];
            i += 2;
        }
        return (sum / 3) % HashSize;
    }

    private (int Index, int Length)? Find(string name)
    {
        int start = Hash(name);
        for (int step = 0; step < HashSize; step++)
        {
            int index = (start + step) % HashSize;
            Node? node = _table[index];
            if (node is null)
                return null;
            if (node.Active && string.Equals(node.Name, name, StringComparison.Ordinal))
                return (index, step + 1);
        }
        return null;
    }

    private string ReadName()
    {
        string name = _nameBox.Text.Trim();
        if (name.Length == 0)
            throw new ArgumentException("请输入姓名。");
        if (name.Length > MaxNameLength)
            name = name[..MaxNameLength];
        return name;
    }

    private void Insert()
    {
        try
        {
            string name = ReadName();
            if (Find(name) is not null)
            {
                Theme.AppendLine(Output, $"“{name}”已经存在。");
                return;
            }

            int start = Hash(name);
            for (int step = 0; step < HashSize; step++)
            {
                int index = (start + step) % HashSize;
                Node? node = _table[index];
                if (node is null || !node.Active)
                {
                    _table[index] = new Node { Name = name, Active = true };
                    Theme.AppendLine(Output, $"插入“{name}” → 下标 {index}（探测 {step + 1} 次）。");
                    return;
                }
            }

            Theme.AppendLine(Output, "哈希表已满，插入失败。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Delete()
    {
        try
        {
            string name = ReadName();
            var found = Find(name);
            if (found is null)
            {
                Theme.AppendLine(Output, $"没有找到“{name}”。");
                return;
            }

            _table[found.Value.Index]!.Active = false;
            Theme.AppendLine(Output,
                $"删除“{name}”：下标 {found.Value.Index}，查找长度 {found.Value.Length}。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Search()
    {
        try
        {
            string name = ReadName();
            var found = Find(name);
            Theme.AppendLine(Output, found is null
                ? $"没有找到“{name}”。"
                : $"找到“{name}”：下标 {found.Value.Index}，查找长度 {found.Value.Length}。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadSample()
    {
        Array.Clear(_table);
        foreach (string name in SampleNames)
        {
            int start = Hash(name);
            for (int step = 0; step < HashSize; step++)
            {
                int index = (start + step) % HashSize;
                if (_table[index] is null)
                {
                    _table[index] = new Node { Name = name, Active = true };
                    break;
                }
            }
        }

        Theme.AppendLine(Output, $"已重建示例哈希表（{SampleNames.Length} 个姓名）。");
        ShowTable();
    }

    private void ClearAll()
    {
        Array.Clear(_table);
        Output.Clear();
        Theme.AppendLine(Output, "哈希表已清空。");
    }

    private void ShowTable()
    {
        int active = 0;
        int deleted = 0;
        int totalSearchLength = 0;

        Theme.AppendLine(Output);
        Theme.AppendHeader(Output, "下标", "姓名", "查找长度");
        for (int i = 0; i < HashSize; i++)
        {
            Node? node = _table[i];
            if (node is null)
                continue;
            if (!node.Active)
            {
                deleted++;
                continue;
            }

            var found = Find(node.Name);
            int length = found?.Length ?? 0;
            active++;
            totalSearchLength += length;
            Theme.AppendLine(Output, $"{i,4}  {node.Name,-22} {length,8}");
        }

        Theme.AppendLine(Output);
        Theme.AppendLine(Output, $"有效节点：{active}    墓碑节点：{deleted}    表长：{HashSize}");
        if (active > 0)
            Theme.AppendLine(Output, $"平均查找长度：{(double)totalSearchLength / active:F3}（仅统计有效节点）");
        Theme.AppendLine(Output);
    }

    private sealed class Node
    {
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}