using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class NumberTheoryForm : ExperimentFormBase
{
    private readonly TabControl _tabs = new();

    public NumberTheoryForm()
        : base("数论算法工具集", "素数判断、最大公约数、扩展欧几里得、欧拉函数和快速模幂。")
    {
        _tabs.Dock = DockStyle.Fill;
        _tabs.Font = Theme.BaseFont;
        _tabs.Padding = new Point(14, 6);

        _tabs.TabPages.Add(BuildPrimeTab());
        _tabs.TabPages.Add(BuildGcdTab());
        _tabs.TabPages.Add(BuildExtendedGcdTab());
        _tabs.TabPages.Add(BuildPhiTab());
        _tabs.TabPages.Add(BuildModPowTab());

        var host = new Panel { Dock = DockStyle.Fill, Height = 250 };
        host.Controls.Add(_tabs);
        AddInput(host, 250);

        var clear = Theme.SecondaryButton("清空输出");
        clear.Click += (_, _) => Output.Clear();
        AddToolbar(clear);
    }

    protected override int InitialInputHeight => 320;

    private TabPage BuildPrimeTab()
    {
        var number = Theme.InputBox(160);
        number.PlaceholderText = "例如 97";
        var button = Theme.PrimaryButton("判断");
        button.Click += (_, _) => RunSafely(() =>
        {
            long n = ReadLong(number, "待判断的数");
            Theme.AppendLine(Output, $"{n} {(NumberTheory.IsPrime(n) ? "是素数" : "不是素数")}。");
        });

        return CreateTab("素数判断",
            FieldRow("待判断的数", number),
            button,
            Hint("从 2 试除到 √n；n < 2 时不是素数。"));
    }

    private TabPage BuildGcdTab()
    {
        var a = Theme.InputBox(140);
        var b = Theme.InputBox(140);
        var button = Theme.PrimaryButton("计算");
        button.Click += (_, _) => RunSafely(() =>
        {
            long x = ReadLong(a, "第一个数");
            long y = ReadLong(b, "第二个数");
            long gcd = NumberTheory.Gcd(x, y);
            Theme.AppendLine(Output, $"gcd({x}, {y}) = {gcd}");
        });

        return CreateTab("最大公约数",
            FieldRow("第一个数", a),
            FieldRow("第二个数", b),
            button,
            Hint("使用欧几里得算法（辗转相除）。"));
    }

    private TabPage BuildExtendedGcdTab()
    {
        var a = Theme.InputBox(140);
        var b = Theme.InputBox(140);
        var button = Theme.PrimaryButton("求解");
        button.Click += (_, _) => RunSafely(() =>
        {
            long x = ReadLong(a, "第一个数");
            long y = ReadLong(b, "第二个数");
            long gcd = NumberTheory.ExtendedGcd(x, y, out long coefficientX, out long coefficientY);
            Theme.AppendLine(Output,
                $"gcd({x}, {y}) = {gcd}，且 {x} × {coefficientX} + {y} × {coefficientY} = {gcd}");
        });

        return CreateTab("扩展欧几里得",
            FieldRow("第一个数", a),
            FieldRow("第二个数", b),
            button,
            Hint("求 x、y，使 a·x + b·y = gcd(a, b)。"));
    }

    private TabPage BuildPhiTab()
    {
        var n = Theme.InputBox(160);
        n.PlaceholderText = "例如 36";
        var button = Theme.PrimaryButton("计算");
        button.Click += (_, _) => RunSafely(() =>
        {
            long value = ReadLong(n, "n");
            long phi = NumberTheory.EulerPhi(value);
            Theme.AppendLine(Output, $"φ({value}) = {phi}");
        });

        return CreateTab("欧拉函数",
            FieldRow("n", n),
            button,
            Hint("统计 1–n 中与 n 互质的正整数个数。"));
    }

    private TabPage BuildModPowTab()
    {
        var baseBox = Theme.InputBox(120);
        var exponentBox = Theme.InputBox(120);
        var modulusBox = Theme.InputBox(120);
        var button = Theme.PrimaryButton("计算");
        button.Click += (_, _) => RunSafely(() =>
        {
            long baseValue = ReadLong(baseBox, "底数");
            long exponent = ReadLong(exponentBox, "指数");
            long modulus = ReadLong(modulusBox, "模数");
            if (exponent < 0)
                throw new ArgumentException("指数不能为负数。");
            long result = NumberTheory.ModPow(baseValue, exponent, modulus);
            Theme.AppendLine(Output, $"{baseValue}^{exponent} mod {modulus} = {result}");
        });

        return CreateTab("模幂运算",
            FieldRow("底数", baseBox),
            FieldRow("指数", exponentBox),
            FieldRow("模数", modulusBox),
            button,
            Hint("使用快速幂（平方-乘）算法，避免 pow 溢出。"));
    }

    private static TabPage CreateTab(string title, params Control[] controls)
    {
        var page = new TabPage(title)
        {
            BackColor = Theme.Card,
            Padding = new Padding(8)
        };
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12),
            BackColor = Theme.Card
        };
        panel.Controls.AddRange(controls);
        page.Controls.Add(panel);
        return page;
    }

    private static FlowLayoutPanel FieldRow(string label, Control control)
    {
        var row = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 2, 0, 2),
            BackColor = Theme.Card
        };
        row.Controls.Add(new Label
        {
            Text = label,
            Width = 110,
            AutoSize = false,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 8, 8, 0)
        });
        control.Margin = new Padding(0, 6, 8, 6);
        row.Controls.Add(control);
        return row;
    }

    private static Label Hint(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Theme.Muted,
        Font = Theme.SmallFont,
        Margin = new Padding(0, 12, 0, 0)
    };

    private static long ReadLong(TextBox box, string fieldName)
    {
        if (!long.TryParse(box.Text.Trim(), out long value))
            throw new ArgumentException($"{fieldName} 必须是整数。");
        return value;
    }

    private void RunSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "计算失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

internal static class NumberTheory
{
    public static bool IsPrime(long n)
    {
        if (n < 2)
            return false;
        for (long divisor = 2; divisor <= n / divisor; divisor++)
        {
            if (n % divisor == 0)
                return false;
        }
        return true;
    }

    public static long Gcd(long a, long b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            long remainder = a % b;
            a = b;
            b = remainder;
        }
        return a;
    }

    public static long ExtendedGcd(long a, long b, out long x, out long y)
    {
        if (b == 0)
        {
            x = a >= 0 ? 1 : -1;
            y = 0;
            return Math.Abs(a);
        }

        long gcd = ExtendedGcd(b, a % b, out long x1, out long y1);
        x = y1;
        y = x1 - (a / b) * y1;
        return gcd;
    }

    public static long EulerPhi(long n)
    {
        if (n <= 0)
            throw new ArgumentException("n 必须是正整数。");

        long result = n;
        long remaining = n;
        for (long prime = 2; prime <= remaining / prime; prime++)
        {
            if (remaining % prime != 0)
                continue;

            while (remaining % prime == 0)
                remaining /= prime;
            result -= result / prime;
        }

        if (remaining > 1)
            result -= result / remaining;
        return result;
    }

    public static long ModPow(long baseValue, long exponent, long modulus)
    {
        if (modulus <= 0)
            throw new ArgumentException("模数必须是正整数。");
        if (modulus == 1)
            return 0;

        long result = 1;
        long current = ((baseValue % modulus) + modulus) % modulus;
        long power = exponent;

        while (power > 0)
        {
            if ((power & 1) == 1)
                result = (long)((Int128)result * current % modulus);
            current = (long)((Int128)current * current % modulus);
            power >>= 1;
        }

        return result;
    }
}