using ISO11820.Global;

namespace ISO11820.Forms;

/// <summary>
/// 新建试验窗体
/// </summary>
public class NewTestForm : Form
{
    private readonly TextBox _txtProductId = new();
    private readonly TextBox _txtProductName = new();
    private readonly TextBox _txtSpec = new();
    private readonly TextBox _txtDiameter = new();
    private readonly TextBox _txtHeight = new();
    private readonly TextBox _txtPreWeight = new();
    private readonly TextBox _txtAmbTemp = new();
    private readonly TextBox _txtAmbHumi = new();
    private readonly ComboBox _cmbDurationMode = new();
    private readonly Button _btnCreate = new();
    private readonly Button _btnCancel = new();

    public NewTestForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "新建试验";
        this.Size = new Size(480, 520);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(0x0F, 0x17, 0x2A);
        this.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);

        int y = 20, gap = 35;

        AddField("样品编号：", y, ref _txtProductId, "如 20240613-001"); y += gap;
        AddField("样品名称：", y, ref _txtProductName, "如 岩棉隔热板"); y += gap;
        AddField("规格型号：", y, ref _txtSpec, "如 100×50×25mm"); y += gap;
        AddField("直径 (mm)：", y, ref _txtDiameter, ""); y += gap;
        AddField("高度 (mm)：", y, ref _txtHeight, ""); y += gap;
        AddField("试验前质量 (g)：", y, ref _txtPreWeight, ""); y += gap;
        AddField("环境温度 (°C)：", y, ref _txtAmbTemp, "25"); y += gap;
        AddField("环境湿度 (%)：", y, ref _txtAmbHumi, "50"); y += gap;

        // 时长模式
        var lblMode = new Label();
        lblMode.Text = "时长模式：";
        lblMode.Location = new Point(30, y);
        lblMode.Size = new Size(100, 30);
        lblMode.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);
        _cmbDurationMode.Location = new Point(140, y);
        _cmbDurationMode.Size = new Size(290, 30);
        _cmbDurationMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbDurationMode.Items.AddRange(new[] { "标准 60 分钟", "自定义" });
        _cmbDurationMode.SelectedIndex = 0;
        _cmbDurationMode.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _cmbDurationMode.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        this.Controls.Add(lblMode);
        this.Controls.Add(_cmbDurationMode);
        y += gap + 10;

        _btnCreate.Text = "创建试验";
        _btnCreate.Location = new Point(100, y);
        _btnCreate.Size = new Size(120, 40);
        _btnCreate.BackColor = Color.FromArgb(0x3B, 0x82, 0xF6);
        _btnCreate.ForeColor = Color.White;
        _btnCreate.FlatStyle = FlatStyle.Flat;
        _btnCreate.Click += BtnCreate_Click;

        _btnCancel.Text = "取消";
        _btnCancel.Location = new Point(260, y);
        _btnCancel.Size = new Size(100, 40);
        _btnCancel.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _btnCancel.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.DialogResult = DialogResult.Cancel;

        this.Controls.Add(_btnCreate);
        this.Controls.Add(_btnCancel);
    }

    private void AddField(string label, int y, ref TextBox tb, string placeholder)
    {
        var lbl = new Label();
        lbl.Text = label;
        lbl.Location = new Point(30, y);
        lbl.Size = new Size(100, 30);
        lbl.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);

        tb.Location = new Point(140, y);
        tb.Size = new Size(290, 30);
        tb.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        tb.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        tb.BorderStyle = BorderStyle.FixedSingle;

        if (!string.IsNullOrEmpty(placeholder))
            tb.Text = placeholder;

        this.Controls.Add(lbl);
        this.Controls.Add(tb);
    }

    private void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (!double.TryParse(_txtPreWeight.Text, out double preWeight) || preWeight <= 0)
        {
            MessageBox.Show("请输入有效的试验前质量", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string testId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string productId = string.IsNullOrEmpty(_txtProductId.Text) ? testId : _txtProductId.Text;
        double ambtemp = double.TryParse(_txtAmbTemp.Text, out var at) ? at : 25;
        double ambhumi = double.TryParse(_txtAmbHumi.Text, out var ah) ? ah : 50;
        double diameter = double.TryParse(_txtDiameter.Text, out var d) ? d : 0;
        double height = double.TryParse(_txtHeight.Text, out var h) ? h : 0;
        int durationMode = _cmbDurationMode.SelectedIndex;
        int targetDuration = durationMode == 0 ? 3600 : 3600;

        AppCtx.Controller.CreateTest(
            productId, testId, AppCtx.CurrentUserName,
            preWeight, ambtemp, ambhumi,
            durationMode, targetDuration,
            _txtProductName.Text, _txtSpec.Text, diameter, height);

        MessageBox.Show($"试验已创建\n样品编号：{productId}\n试验编号：{testId}",
            "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}