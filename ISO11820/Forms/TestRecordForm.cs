using ISO11820.Global;

namespace ISO11820.Forms;

/// <summary>
/// 试验记录窗体 — 填写试验后质量、火焰现象、备注
/// </summary>
public class TestRecordForm : Form
{
    private readonly NumericUpDown _numPostWeight = new();
    private readonly CheckBox _chkFlame = new();
    private readonly NumericUpDown _numFlameTime = new();
    private readonly NumericUpDown _numFlameDuration = new();
    private readonly TextBox _txtMemo = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnCancel = new();
    private readonly Label _lblInfo = new();

    public TestRecordForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "试验记录";
        this.Size = new Size(440, 380);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(0x0F, 0x17, 0x2A);
        this.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);

        var ctrl = AppCtx.Controller;
        _lblInfo.Text = $"样品：{ctrl.CurrentProductId}  试验：{ctrl.CurrentTestId}\n" +
                        $"试验前质量：{ctrl.PreWeight}g  已记录：{ctrl.ElapsedSeconds}s";
        _lblInfo.Location = new Point(20, 20);
        _lblInfo.Size = new Size(390, 40);
        _lblInfo.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);

        int y = 80;
        var lblWeight = new Label();
        lblWeight.Text = "试验后质量 (g)：";
        lblWeight.Location = new Point(20, y);
        lblWeight.Size = new Size(130, 30);
        lblWeight.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);
        _numPostWeight.Location = new Point(160, y);
        _numPostWeight.Size = new Size(100, 30);
        _numPostWeight.DecimalPlaces = 2;
        _numPostWeight.Maximum = 50000;
        _numPostWeight.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _numPostWeight.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        this.Controls.Add(lblWeight);
        this.Controls.Add(_numPostWeight);
        y += 40;

        _chkFlame.Text = "出现持续火焰";
        _chkFlame.Location = new Point(20, y);
        _chkFlame.Size = new Size(130, 30);
        _chkFlame.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _chkFlame.CheckedChanged += (s, e) =>
        {
            _numFlameTime.Enabled = _chkFlame.Checked;
            _numFlameDuration.Enabled = _chkFlame.Checked;
        };
        this.Controls.Add(_chkFlame);
        y += 35;

        var lblFt = new Label();
        lblFt.Text = "火焰开始 (秒)：";
        lblFt.Location = new Point(40, y);
        lblFt.Size = new Size(110, 30);
        lblFt.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);
        _numFlameTime.Location = new Point(160, y);
        _numFlameTime.Size = new Size(100, 30);
        _numFlameTime.Maximum = 10000;
        _numFlameTime.Enabled = false;
        _numFlameTime.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _numFlameTime.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        this.Controls.Add(lblFt);
        this.Controls.Add(_numFlameTime);
        y += 35;

        var lblFd = new Label();
        lblFd.Text = "火焰持续 (秒)：";
        lblFd.Location = new Point(40, y);
        lblFd.Size = new Size(110, 30);
        lblFd.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);
        _numFlameDuration.Location = new Point(160, y);
        _numFlameDuration.Size = new Size(100, 30);
        _numFlameDuration.Maximum = 10000;
        _numFlameDuration.Enabled = false;
        _numFlameDuration.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _numFlameDuration.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        this.Controls.Add(lblFd);
        this.Controls.Add(_numFlameDuration);
        y += 40;

        var lblMemo = new Label();
        lblMemo.Text = "备注：";
        lblMemo.Location = new Point(20, y);
        lblMemo.Size = new Size(60, 30);
        lblMemo.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);
        _txtMemo.Location = new Point(80, y);
        _txtMemo.Size = new Size(310, 30);
        _txtMemo.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _txtMemo.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _txtMemo.BorderStyle = BorderStyle.FixedSingle;
        this.Controls.Add(lblMemo);
        this.Controls.Add(_txtMemo);
        y += 45;

        _btnSave.Text = "保存";
        _btnSave.Location = new Point(100, y);
        _btnSave.Size = new Size(100, 40);
        _btnSave.BackColor = Color.FromArgb(0x10, 0xB9, 0x81);
        _btnSave.ForeColor = Color.White;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.Click += BtnSave_Click;

        _btnCancel.Text = "取消";
        _btnCancel.Location = new Point(230, y);
        _btnCancel.Size = new Size(100, 40);
        _btnCancel.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _btnCancel.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { _lblInfo, _btnSave, _btnCancel });
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_numPostWeight.Value <= 0)
        {
            MessageBox.Show("请输入试验后质量", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var ctrl = AppCtx.Controller;
        string phenoCode = _chkFlame.Checked ? "FLAME" : "";
        int flameTime = (int)_numFlameTime.Value;
        int flameDuration = (int)_numFlameDuration.Value;

        ctrl.SaveTestResult((double)_numPostWeight.Value, phenoCode,
                            flameTime, flameDuration, _txtMemo.Text);

        ctrl.MarkSaved();
        MessageBox.Show("试验记录已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}