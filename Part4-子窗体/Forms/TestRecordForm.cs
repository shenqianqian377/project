using ISO11820.Global;
using ISO11820.Services;

namespace ISO11820.Forms;

public partial class TestRecordForm : Form
{
    private readonly TestController _controller;
    private CheckBox _chkFlame = null!;
    private NumericUpDown _numFlameTime = null!, _numFlameDuration = null!;
    private NumericUpDown _numPostWeight = null!;
    private Button _btnSave = null!;

    readonly Color BgDark = Color.FromArgb(15, 23, 42);
    readonly Color BgCard = Color.FromArgb(30, 41, 59);
    readonly Color BgInput = Color.FromArgb(51, 65, 85);
    readonly Color TextPri = Color.FromArgb(248, 250, 252);
    readonly Color TextSec = Color.FromArgb(148, 163, 184);
    readonly Color Success = Color.FromArgb(16, 185, 129);

    public TestRecordForm(TestController controller)
    {
        _controller = controller;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "试验记录 — 填写现象与结果";
        this.Size = new Size(520, 430);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = BgDark;

        var panel = new Panel { Location = new Point(12, 5), Size = new Size(480, 380), BackColor = BgCard };
        var title = new Label { Text = "填写试验结果", Font = new Font("Microsoft YaHei", 14, FontStyle.Bold), ForeColor = TextPri, Location = new Point(160, 15), AutoSize = true };
        panel.Controls.Add(title);

        int y = 55;

        var lblF = new Label { Text = "是否出现持续火焰:", Font = new Font("Microsoft YaHei", 10), ForeColor = TextPri, Location = new Point(30, y + 2), AutoSize = true, BackColor = BgCard };
        _chkFlame = new CheckBox { Location = new Point(190, y + 2), Size = new Size(20, 20), BackColor = BgCard };
        _chkFlame.CheckedChanged += (s, e) => { _numFlameTime.Enabled = _numFlameDuration.Enabled = _chkFlame.Checked; };
        panel.Controls.Add(lblF); panel.Controls.Add(_chkFlame);
        y += 38;

        var lblFt = new Label { Text = "火焰发生时刻 (秒):", Font = new Font("Microsoft YaHei", 10), ForeColor = TextPri, Location = new Point(30, y + 2), AutoSize = true, BackColor = BgCard };
        _numFlameTime = new NumericUpDown { Location = new Point(190, y), Width = 110, Maximum = 99999, Enabled = false, BackColor = BgInput, ForeColor = TextPri, Font = new Font("Microsoft YaHei", 10), BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(lblFt); panel.Controls.Add(_numFlameTime);
        y += 38;

        var lblFd = new Label { Text = "火焰持续时间 (秒):", Font = new Font("Microsoft YaHei", 10), ForeColor = TextPri, Location = new Point(30, y + 2), AutoSize = true, BackColor = BgCard };
        _numFlameDuration = new NumericUpDown { Location = new Point(190, y), Width = 110, Maximum = 99999, Enabled = false, BackColor = BgInput, ForeColor = TextPri, Font = new Font("Microsoft YaHei", 10), BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(lblFd); panel.Controls.Add(_numFlameDuration);
        y += 44;

        var sep = new Panel { Location = new Point(20, y), Size = new Size(440, 1), BackColor = Color.FromArgb(51, 65, 85) };
        panel.Controls.Add(sep); y += 16;

        var lblPw = new Label { Text = "试验后质量 (g) *必填", Font = new Font("Microsoft YaHei", 10), ForeColor = Color.FromArgb(252, 165, 165), Location = new Point(30, y + 2), AutoSize = true, BackColor = BgCard };
        _numPostWeight = new NumericUpDown { Location = new Point(220, y), Width = 140, DecimalPlaces = 2, Maximum = 99999, BackColor = BgInput, ForeColor = TextPri, Font = new Font("Microsoft YaHei", 10), BorderStyle = BorderStyle.FixedSingle };
        panel.Controls.Add(lblPw); panel.Controls.Add(_numPostWeight);
        y += 42;

        var lblN = new Label { Text = "备注:", Font = new Font("Microsoft YaHei", 10), ForeColor = TextPri, Location = new Point(30, y + 2), AutoSize = true, BackColor = BgCard };
        var txtNotes = new TextBox { Location = new Point(110, y), Size = new Size(320, 45), Multiline = true, BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Microsoft YaHei", 9) };
        panel.Controls.Add(lblN); panel.Controls.Add(txtNotes);
        y += 58;

        _btnSave = new Button
        {
            Text = "保 存 试 验 记 录",
            Location = new Point(130, y),
            Size = new Size(220, 42),
            Font = new Font("Microsoft YaHei", 11, FontStyle.Bold),
            BackColor = Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(5, 150, 105);
        _btnSave.Click += (s, e) => SaveRecord(txtNotes.Text);
        panel.Controls.Add(_btnSave);

        this.Controls.Add(panel);
    }

    private void SaveRecord(string notes)
    {
        double postWeight = (double)_numPostWeight.Value;
        if (postWeight <= 0) { MessageBox.Show("请填写试验后质量", "提示"); return; }

        double preWeight = _controller.PreWeight;
        double lostWeight = preWeight - postWeight;
        double lostPer = preWeight > 0 ? Math.Round(lostWeight / preWeight * 100, 2) : 0;

        double deltaTf1 = _controller.MaxTf1 - _controller.AmbTemp;
        double deltaTf2 = _controller.MaxTf2 - _controller.AmbTemp;
        double deltaTs = _controller.MaxTs - _controller.AmbTemp;
        double deltaTc = _controller.MaxTc - _controller.AmbTemp;
        double deltaTf = deltaTs;

        int flameTime = _chkFlame.Checked ? (int)_numFlameTime.Value : 0;
        int flameDuration = _chkFlame.Checked ? (int)_numFlameDuration.Value : 0;
        string pheno = (_chkFlame.Checked && flameDuration >= 5) ? "FLAME_DETECTED" : "NO_FLAME";

        var data = _controller.GetCurrentData();
        double fTf1 = (double)data["Tf1"], fTf2 = (double)data["Tf2"];
        double fTs = (double)data["Ts"], fTc = (double)data["Tc"];

        AppCtx.Instance.Db.UpdateTestResult(
            _controller.CurrentProductId, _controller.CurrentTestId,
            preWeight, postWeight, lostWeight, lostPer,
            deltaTf1, deltaTf2, deltaTf, deltaTs, deltaTc,
            _controller.ElapsedSeconds, pheno,
            _controller.MaxTf1, _controller.MaxTf2, _controller.MaxTs, _controller.MaxTc,
            _controller.MaxTf1Time, _controller.MaxTf2Time, _controller.MaxTsTime, _controller.MaxTcTime,
            fTf1, fTf2, fTs, fTc, 0, 0, 0, 0,
            _controller.ConstPowerValue, flameTime, flameDuration);

        var sensorData = _controller.GetSensorDataBuffer();
        try
        {
            var csvPath = AppCtx.Instance.ExportService.ExportCsv(_controller.CurrentProductId, _controller.CurrentTestId, sensorData);
            var xlsxPath = AppCtx.Instance.ExportService.ExportExcel(_controller.CurrentProductId, _controller.CurrentTestId, sensorData);
            var pdfPath = AppCtx.Instance.ExportService.ExportPdf(_controller.CurrentProductId, _controller.CurrentTestId);
            _controller.HasUnsavedResult = false;
            MessageBox.Show($"保存成功！\nCSV: {csvPath}\nExcel: {xlsxPath}\nPDF: {pdfPath}", "保存成功");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex) { MessageBox.Show($"导出错误: {ex.Message}", "错误"); }
    }
}
