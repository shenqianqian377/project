using ISO11820.Global;

namespace ISO11820.Forms;

public partial class NewTestForm : Form
{
    private TextBox _txtProductId = null!, _txtTestId = null!, _txtProductName = null!,
                     _txtSpec = null!, _txtHeight = null!, _txtDiameter = null!,
                     _txtPreWeight = null!, _txtAmbTemp = null!, _txtAmbHumi = null!,
                     _txtTargetDuration = null!;
    private RadioButton _rbStandard = null!, _rbCustom = null!;
    private Label _lblDeviceNo = null!, _lblDeviceName = null!, _lblChkDate = null!, _lblConstPower = null!;

    public string ProductId => _txtProductId.Text.Trim();
    public string TestId => _txtTestId.Text.Trim();
    public string SampleName => _txtProductName.Text.Trim();
    public string Spec => _txtSpec.Text.Trim();
    public double Diameter => double.TryParse(_txtDiameter.Text, out var v) ? v : 0;
    public double SampleHeight => double.TryParse(_txtHeight.Text, out var v) ? v : 0;
    public double PreWeight => double.TryParse(_txtPreWeight.Text, out var v) ? v : 0;
    public double AmbTemp => double.TryParse(_txtAmbTemp.Text, out var v) ? v : 25;
    public double AmbHumi => double.TryParse(_txtAmbHumi.Text, out var v) ? v : 50;
    public int DurationMode => _rbStandard.Checked ? 0 : 1;
    public int TargetDuration => int.TryParse(_txtTargetDuration.Text, out var v) ? v : 3600;

    readonly Color BgDark = Color.FromArgb(15, 23, 42);
    readonly Color BgCard = Color.FromArgb(30, 41, 59);
    readonly Color BgInput = Color.FromArgb(51, 65, 85);
    readonly Color TextPri = Color.FromArgb(248, 250, 252);
    readonly Color TextSec = Color.FromArgb(148, 163, 184);
    readonly Color Accent = Color.FromArgb(59, 130, 246);

    public NewTestForm()
    {
        InitializeComponent();
        LoadDeviceInfo();
    }

    private void LoadDeviceInfo()
    {
        var dev = AppCtx.Instance.Db.GetApparatus(0);
        if (dev.TryGetValue("innernumber", out var i)) _lblDeviceNo.Text = i.ToString();
        if (dev.TryGetValue("apparatusname", out var n)) _lblDeviceName.Text = n.ToString();
        if (dev.TryGetValue("checkdatef", out var c)) _lblChkDate.Text = c.ToString();
        if (dev.TryGetValue("constpower", out var p)) _lblConstPower.Text = p.ToString();
    }

    private void InitializeComponent()
    {
        this.Text = "新建试验";
        this.Size = new Size(600, 680);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = BgDark;

        int y = 15;
        var panel = new Panel { Location = new Point(12, 5), Size = new Size(560, 630), BackColor = BgCard };

        var title = new Label { Text = "创建新试验", Font = new Font("Microsoft YaHei", 14, FontStyle.Bold), ForeColor = TextPri, Location = new Point(200, y), AutoSize = true };
        panel.Controls.Add(title);
        y += 42;

        AddRow(panel, "样品编号", ref _txtProductId, ref y, "如 SAMPLE-001");
        AddRow(panel, "试验标识", ref _txtTestId, ref y, "如 20240614-001");
        _txtTestId.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        AddRow(panel, "样品名称", ref _txtProductName, ref y, "如 岩棉隔热板");
        AddRow(panel, "规格型号", ref _txtSpec, ref y, "如 100x50x25mm");
        AddRow(panel, "高度 (mm)", ref _txtHeight, ref y, "50");
        AddRow(panel, "直径 (mm)", ref _txtDiameter, ref y, "45");
        y += 4;

        var gbEnv = new GroupBox { Text = "环境信息", ForeColor = TextSec, Font = new Font("Microsoft YaHei", 9), Location = new Point(22, y), Size = new Size(510, 52) };
        AddLabeledInput(gbEnv, "环境温度(°C):", 20, 20, ref _txtAmbTemp, 140, 17, 80); _txtAmbTemp.Text = "25";
        AddLabeledInput(gbEnv, "环境湿度(%):", 290, 20, ref _txtAmbHumi, 400, 17, 80); _txtAmbHumi.Text = "50";
        panel.Controls.Add(gbEnv); y += 60;

        var gbDur = new GroupBox { Text = "试验时长模式", ForeColor = TextSec, Font = new Font("Microsoft YaHei", 9), Location = new Point(22, y), Size = new Size(510, 52) };
        _rbStandard = new RadioButton { Text = "标准 60 分钟", Location = new Point(20, 20), Font = new Font("Microsoft YaHei", 9), ForeColor = TextPri, Checked = true, BackColor = BgCard };
        _rbCustom = new RadioButton { Text = "自定义(秒):", Location = new Point(170, 20), Font = new Font("Microsoft YaHei", 9), ForeColor = TextPri, BackColor = BgCard };
        _txtTargetDuration = new TextBox { Location = new Point(270, 17), Size = new Size(80, 24), Text = "3600", Enabled = false, BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft YaHei", 9) };
        _rbStandard.CheckedChanged += (s, e) => _txtTargetDuration.Enabled = _rbCustom.Checked;
        _rbCustom.CheckedChanged += (s, e) => _txtTargetDuration.Enabled = _rbCustom.Checked;
        gbDur.Controls.Add(_rbStandard); gbDur.Controls.Add(_rbCustom); gbDur.Controls.Add(_txtTargetDuration);
        panel.Controls.Add(gbDur); y += 60;

        AddRow(panel, "试验前质量 (g)", ref _txtPreWeight, ref y, "如 125.5");
        y += 2;

        var opLabel = new Label { Text = $"操作员: {AppCtx.Instance.CurrentUserName}", Font = new Font("Microsoft YaHei", 9), ForeColor = TextSec, Location = new Point(36, y), AutoSize = true };
        panel.Controls.Add(opLabel); y += 30;

        var gbDev = new GroupBox { Text = "设备信息（自动填入）", ForeColor = TextSec, Font = new Font("Microsoft YaHei", 9), Location = new Point(22, y), Size = new Size(510, 78) };
        AddDevRow(gbDev, "设备编号:", 18, 22, ref _lblDeviceNo, 100, 22);
        AddDevRow(gbDev, "设备名称:", 240, 22, ref _lblDeviceName, 320, 22);
        AddDevRow(gbDev, "检定日期:", 18, 48, ref _lblChkDate, 100, 48);
        AddDevRow(gbDev, "恒功率值:", 240, 48, ref _lblConstPower, 320, 48);
        panel.Controls.Add(gbDev); y += 90;

        var btn = new Button
        {
            Text = "创 建 试 验",
            Location = new Point(170, y),
            Size = new Size(200, 42),
            Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
            BackColor = Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
        btn.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(TestId) || PreWeight <= 0)
            { MessageBox.Show("请填写样品编号、试验标识和试验前质量", "提示"); return; }
            this.DialogResult = DialogResult.OK; this.Close();
        };
        panel.Controls.Add(btn);
        this.Controls.Add(panel);
    }

    void AddRow(Panel p, string label, ref TextBox tb, ref int y, string hint)
    {
        var lbl = new Label { Text = label + ":", Font = new Font("Microsoft YaHei", 9), ForeColor = TextSec, Location = new Point(36, y + 3), AutoSize = true };
        tb = new TextBox { Location = new Point(170, y), Size = new Size(230, 24), BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft YaHei", 9) };
        var h = new Label { Text = hint, Font = new Font("Microsoft YaHei", 8), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(410, y + 3), AutoSize = true };
        p.Controls.Add(lbl); p.Controls.Add(tb); p.Controls.Add(h);
        y += 32;
    }

    void AddLabeledInput(GroupBox gb, string label, int lx, int ly, ref TextBox tb, int tx, int ty, int tw)
    {
        var lbl = new Label { Text = label, Font = new Font("Microsoft YaHei", 9), ForeColor = TextSec, Location = new Point(lx, ly), AutoSize = true, BackColor = BgCard };
        tb = new TextBox { Location = new Point(tx, ty), Size = new Size(tw, 22), BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft YaHei", 9) };
        gb.Controls.Add(lbl); gb.Controls.Add(tb);
    }

    void AddDevRow(GroupBox gb, string label, int lx, int ly, ref Label lb, int bx, int by)
    {
        var lbl = new Label { Text = label, Font = new Font("Microsoft YaHei", 9), ForeColor = TextSec, Location = new Point(lx, ly), AutoSize = true, BackColor = BgCard };
        lb = new Label { Text = "-", Font = new Font("Microsoft YaHei", 9), ForeColor = TextPri, Location = new Point(bx, by), AutoSize = true, BackColor = BgCard, MaximumSize = new Size(180, 20) };
        gb.Controls.Add(lbl); gb.Controls.Add(lb);
    }
}
