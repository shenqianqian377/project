using ISO11820.Services;

namespace ISO11820.Forms;

/// <summary>
/// 主窗体 — 试验控制面板
/// UI 控件布局和事件绑定在此实现。
/// </summary>
public class MainForm : Form
{
    private readonly Label _lblStatus = new();
    private readonly Label _lblTimer = new();
    private readonly Label _lblDrift = new();
    private readonly Label _lblProductId = new();
    private readonly Button _btnNewTest = new();
    private readonly Button _btnStartHeat = new();
    private readonly Button _btnStopHeat = new();
    private readonly Button _btnStartRec = new();
    private readonly Button _btnStopRec = new();
    private readonly Button _btnTestRecord = new();
    private readonly Button _btnParams = new();
    private readonly RichTextBox _rtbLog = new();
    private TestController Controller => global::ISO11820.Global.AppCtx.Controller;

    public MainForm()
    {
        InitializeComponent();
        WireEvents();
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 建筑材料不燃性试验系统";
        this.Size = new Size(1280, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(0x0F, 0x17, 0x2A);
        this.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        this.IsMdiContainer = false;

        // 顶部信息栏
        _lblProductId.Text = "样品编号：—";
        _lblProductId.Font = new Font("微软雅黑", 12, FontStyle.Bold);
        _lblProductId.Location = new Point(20, 20);
        _lblProductId.Size = new Size(200, 30);
        _lblProductId.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);

        _lblStatus.Text = "状态：空闲";
        _lblStatus.Font = new Font("微软雅黑", 14, FontStyle.Bold);
        _lblStatus.Location = new Point(260, 18);
        _lblStatus.Size = new Size(200, 35);
        _lblStatus.ForeColor = Color.FromArgb(0x10, 0xB9, 0x81);

        _lblTimer.Text = "00:00";
        _lblTimer.Font = new Font("Consolas", 28, FontStyle.Bold);
        _lblTimer.Location = new Point(520, 12);
        _lblTimer.Size = new Size(140, 50);
        _lblTimer.ForeColor = Color.FromArgb(0x3B, 0x82, 0xF6);

        _lblDrift.Text = "温漂：-- °C/10min";
        _lblDrift.Font = new Font("微软雅黑", 12);
        _lblDrift.Location = new Point(700, 25);
        _lblDrift.Size = new Size(200, 30);
        _lblDrift.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);

        // 按钮列
        int btnY = 80;
        int btnH = 36;
        _btnNewTest.Text = "新建试验";
        _btnNewTest.Location = new Point(20, btnY); _btnNewTest.Size = new Size(120, btnH);
        _btnNewTest.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _btnNewTest.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _btnNewTest.FlatStyle = FlatStyle.Flat;
        _btnNewTest.Click += (s, e) => OpenNewTestForm();

        _btnStartHeat.Text = "开始升温";
        _btnStartHeat.Location = new Point(150, btnY); _btnStartHeat.Size = new Size(100, btnH);
        _btnStartHeat.BackColor = Color.FromArgb(0xFB, 0x92, 0x3C);
        _btnStartHeat.ForeColor = Color.White; _btnStartHeat.FlatStyle = FlatStyle.Flat;
        _btnStartHeat.Click += (s, e) => Controller.StartHeating();

        _btnStopHeat.Text = "停止升温";
        _btnStopHeat.Location = new Point(260, btnY); _btnStopHeat.Size = new Size(100, btnH);
        _btnStopHeat.BackColor = Color.FromArgb(0xEF, 0x44, 0x44);
        _btnStopHeat.ForeColor = Color.White; _btnStopHeat.FlatStyle = FlatStyle.Flat;
        _btnStopHeat.Click += (s, e) => Controller.StopHeating();

        _btnStartRec.Text = "开始记录";
        _btnStartRec.Location = new Point(380, btnY); _btnStartRec.Size = new Size(100, btnH);
        _btnStartRec.BackColor = Color.FromArgb(0x10, 0xB9, 0x81);
        _btnStartRec.ForeColor = Color.White; _btnStartRec.FlatStyle = FlatStyle.Flat;
        _btnStartRec.Click += (s, e) => Controller.StartRecording();

        _btnStopRec.Text = "停止记录";
        _btnStopRec.Location = new Point(490, btnY); _btnStopRec.Size = new Size(100, btnH);
        _btnStopRec.BackColor = Color.FromArgb(0xEF, 0x44, 0x44);
        _btnStopRec.ForeColor = Color.White; _btnStopRec.FlatStyle = FlatStyle.Flat;
        _btnStopRec.Click += (s, e) => Controller.StopRecording();

        _btnTestRecord.Text = "试验记录";
        _btnTestRecord.Location = new Point(610, btnY); _btnTestRecord.Size = new Size(100, btnH);
        _btnTestRecord.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _btnTestRecord.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _btnTestRecord.FlatStyle = FlatStyle.Flat;
        _btnTestRecord.Click += (s, e) => OpenTestRecordForm();

        _btnParams.Text = "参数设置";
        _btnParams.Location = new Point(720, btnY); _btnParams.Size = new Size(100, btnH);
        _btnParams.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _btnParams.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _btnParams.FlatStyle = FlatStyle.Flat;

        // 日志框
        _rtbLog.Location = new Point(20, 140);
        _rtbLog.Size = new Size(820, 200);
        _rtbLog.BackColor = Color.FromArgb(0x1E, 0x29, 0x3B);
        _rtbLog.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _rtbLog.ReadOnly = true;
        _rtbLog.Font = new Font("Consolas", 10);

        this.Controls.AddRange(new Control[] {
            _lblProductId, _lblStatus, _lblTimer, _lblDrift,
            _btnNewTest, _btnStartHeat, _btnStopHeat,
            _btnStartRec, _btnStopRec, _btnTestRecord, _btnParams,
            _rtbLog
        });
    }

    private void WireEvents()
    {
        Controller.DataBroadcast += OnDataBroadcast;
    }

    private void OnDataBroadcast(object? sender, DataBroadcastEventArgs e)
    {
        this.Invoke(() =>
        {
            // 更新状态
            _lblStatus.Text = "状态：" + TestController.GetStateText(e.State);
            _lblStatus.ForeColor = e.State switch
            {
                TestState.Preparing => Color.FromArgb(0xFB, 0x92, 0x3C),
                TestState.Ready => Color.FromArgb(0x10, 0xB9, 0x81),
                TestState.Recording => Color.FromArgb(0x3B, 0x82, 0xF6),
                TestState.Complete => Color.FromArgb(0x94, 0xA3, 0xB8),
                _ => Color.FromArgb(0x94, 0xA3, 0xB8)
            };

            // 计时器
            int min = e.ElapsedSeconds / 60;
            int sec = e.ElapsedSeconds % 60;
            _lblTimer.Text = $"{min:D2}:{sec:D2}";
            _lblProductId.Text = string.IsNullOrEmpty(e.ProductId) ? "样品编号：—" : $"样品编号：{e.ProductId}";

            // 温漂
            if (e.State == TestState.Recording || e.State == TestState.Ready)
                _lblDrift.Text = $"温漂：{e.TemperatureDrift:F2} °C/10min";
            else
                _lblDrift.Text = "温漂：-- °C/10min";

            // 按钮状态
            UpdateButtonStates(e.State);

            // 消息日志
            foreach (var msg in e.Messages)
            {
                _rtbLog.SelectionColor = msg.IsWarning ? Color.Yellow : Color.FromArgb(0xF8, 0xFA, 0xFC);
                _rtbLog.AppendText($"{msg.Time}  {msg.Message}\n");
                _rtbLog.ScrollToCaret();
            }
        });
    }

    private void UpdateButtonStates(TestState state)
    {
        _btnNewTest.Enabled = state == TestState.Idle;
        _btnStartHeat.Enabled = state == TestState.Idle;
        _btnStopHeat.Enabled = state is TestState.Preparing or TestState.Ready or TestState.Complete;
        _btnStartRec.Enabled = state == TestState.Ready;
        _btnStopRec.Enabled = state == TestState.Recording;
        _btnTestRecord.Enabled = state == TestState.Complete;
    }

    private void OpenNewTestForm()
    {
        var form = new NewTestForm();
        form.ShowDialog(this);
    }

    private void OpenTestRecordForm()
    {
        if (Controller.HasUnsavedResult)
        {
            var form = new TestRecordForm();
            form.ShowDialog(this);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        Application.Exit();
    }
}