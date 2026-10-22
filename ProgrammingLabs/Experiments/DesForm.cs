using ProgrammingLabs.Ui;

namespace ProgrammingLabs.Experiments;

internal sealed class DesForm : ExperimentFormBase
{
    private readonly TextBox _keyBox;
    private readonly TextBox _plainBox;

    public DesForm()
        : base("DES 加密", "标准 DES：IP 置换 → 16 轮 Feistel 迭代 → 逆置换，输出 16 进制密文。")
    {
        _keyBox = Theme.InputBox(260);
        _keyBox.MaxLength = 8;
        _keyBox.Font = new Font("Consolas", 11F);
        _keyBox.PlaceholderText = "8 个 ASCII 字符";
        AddLabeledRow("密钥", _keyBox, 80);

        _plainBox = Theme.InputBox(260);
        _plainBox.MaxLength = 8;
        _plainBox.Font = new Font("Consolas", 11F);
        _plainBox.PlaceholderText = "8 个 ASCII 字符";
        AddLabeledRow("明文", _plainBox, 80);

        var hint = new Label
        {
            Text = "不足 8 字节时自动补 0x00，超过 8 字节时截断；本实验仅用于学习，请勿用于真实数据加密。",
            AutoSize = true,
            ForeColor = Theme.Muted,
            Font = Theme.SmallFont,
            Margin = new Padding(0, 4, 0, 0)
        };
        AddInput(hint);

        var encrypt = Theme.PrimaryButton("加密");
        var example = Theme.SecondaryButton("使用示例");
        var clear = Theme.SecondaryButton("清空输出");
        encrypt.Click += (_, _) => Encrypt();
        example.Click += (_, _) =>
        {
            _keyBox.Text = "12345678";
            _plainBox.Text = "abcdefgh";
        };
        clear.Click += (_, _) => Output.Clear();
        AddToolbar(encrypt, example, clear);
    }

    protected override int InitialInputHeight => 230;

    private void Encrypt()
    {
        try
        {
            string key = _keyBox.Text;
            string plain = _plainBox.Text;
            if (key.Length == 0)
                throw new ArgumentException("请输入密钥。");
            if (plain.Length == 0)
                throw new ArgumentException("请输入明文。");
            if (key.Any(c => c > 127) || plain.Any(c => c > 127))
                throw new ArgumentException("本演示按 ASCII 处理，请输入 ASCII 字符。");

            string cipher = DesCipher.Encrypt(key, plain);

            Theme.AppendLine(Output, "DES 加密结果");
            Theme.AppendHeader(Output, "项目", "内容");
            Theme.AppendLine(Output, $"{"密钥",-12}{key,-12}");
            Theme.AppendLine(Output, $"{"明文",-12}{plain,-12}");
            Theme.AppendLine(Output, $"{"密文(hex)",-12}{cipher}");
            Theme.AppendLine(Output);

            if (key.Length < 8 || plain.Length < 8)
                Theme.AppendLine(Output, "提示：输入长度不足 8 字节，已按 0x00 补齐。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "无法加密", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}