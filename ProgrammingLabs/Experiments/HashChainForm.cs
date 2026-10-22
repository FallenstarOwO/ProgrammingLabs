using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class HashChainForm : ExperimentFormBase
{
    private const int HashSize = 26;
    private readonly List<string>[] _buckets;
    private readonly TextBox _nameBox;

    public HashChainForm()
        : base("链地址法哈希表", "按姓名首字母散列到 26 个桶，冲突数据挂成链表；支持插入、删除、查找和显示。")
    {
        _buckets = new List<string>[HashSize];
        for (int i = 0; i < HashSize; i++)
            _buckets[i] = new List<string>();

        _nameBox = Theme.InputBox(260);
        _nameBox.PlaceholderText = "输入姓名，例如 Zhang San";
        AddLabeledRow("姓名", _nameBox, 70);

        var hint = new Label
        {
            Text = "哈希函数：取姓名首字母，A–Z/a–z 映射到 0–25，其他字符按字符编码取模。新节点插入到链表头部。",
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
        var sample = Theme.SecondaryButton("载入示例");
        var clear = Theme.SecondaryButton("清空");
        insert.Click += (_, _) => Insert();
        delete.Click += (_, _) => Delete();
        search.Click += (_, _) => Search();
        show.Click += (_, _) => ShowTable();
        sample.Click += (_, _) => LoadSample();
        clear.Click += (_, _) => ClearAll();
        AddToolbar(insert, delete, search, show, sample, clear);
    }

    protected override int InitialInputHeight => 170;

    private string ReadName()
    {
        string name = _nameBox.Text.Trim();
        if (name.Length == 0)
            throw new ArgumentException("请输入姓名。");
        return name;
    }

    private static int Hash(string name)
    {
        char first = name[0];
        if (first is >= 'A' and <= 'Z')
            return (first - 'A') % HashSize;
        if (first is >= 'a' and <= 'z')
            return (first - 'a') % HashSize;
        return first % HashSize;
    }

    private void Insert()
    {
        try
        {
            string name = ReadName();
            int bucket = Hash(name);
            if (_buckets[bucket].Contains(name))
            {
                Theme.AppendLine(Output, $"“{name}”已经存在（桶 {bucket}）。");
                return;
            }

            _buckets[bucket].Insert(0, name);
            Theme.AppendLine(Output, $"插入“{name}” → 桶 {bucket}。");
            ShowTable();
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
            int bucket = Hash(name);
            if (_buckets[bucket].Remove(name))
            {
                Theme.AppendLine(Output, $"删除“{name}”（桶 {bucket}）。");
                ShowTable();
            }
            else
            {
                Theme.AppendLine(Output, $"没有找到“{name}”。");
            }
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
            int bucket = Hash(name);
            int index = _buckets[bucket].IndexOf(name);
            if (index < 0)
            {
                Theme.AppendLine(Output, $"没有找到“{name}”。");
            }
            else
            {
                Theme.AppendLine(Output, $"找到“{name}”：桶 {bucket}，链表第 {index + 1} 个节点。");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadSample()
    {
        foreach (List<string> bucket in _buckets)
            bucket.Clear();

        string[] names =
        {
            "Chen Xuqing", "Li Jiajie", "Wang Kai", "Zhang Bingkun",
            "Zhao Lei", "He Huaxian", "Shen Chunlin", "Guo Xu",
            "Alice", "Bob", "Grace", "Huang Zikai"
        };
        foreach (string name in names)
            _buckets[Hash(name)].Insert(0, name);

        Theme.AppendLine(Output, $"已载入 {names.Length} 个示例姓名。");
        ShowTable();
    }

    private void ClearAll()
    {
        foreach (List<string> bucket in _buckets)
            bucket.Clear();
        Output.Clear();
        Theme.AppendLine(Output, "哈希表已清空。");
    }

    private void ShowTable()
    {
        Theme.AppendLine(Output);
        Theme.AppendHeader(Output, "桶", "数量", "链表（从表头开始）");
        for (int i = 0; i < HashSize; i++)
        {
            string content = _buckets[i].Count == 0 ? "(空)" : string.Join(" -> ", _buckets[i]);
            Theme.AppendLine(Output, $"{i,2}  {_buckets[i].Count,4}  {content}");
        }
        Theme.AppendLine(Output);
    }
}