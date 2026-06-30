using ISO11820.Global;

namespace ISO11820.Forms;

public partial class LoginForm : Form
{
    private TextBox _txtPwd = null!;
    private RadioButton _rbAdmin = null!, _rbExperimenter = null!;
    private Button _btnLogin = null!;
    private Label _lblError = null!;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 建筑材料不燃性试验仿真系统";
        this.Size = new Size(520, 460);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(15, 23, 42);

        var card = new Panel
        {
            Size = new Size(400, 340),
            Location = new Point(60, 50),
            BackColor = Color.FromArgb(30, 41, 59),
        };

        var icon = new Label
        {
            Text = "🔥",
            Font = new Font("Segoe UI", 36),
            ForeColor = Color.FromArgb(251, 146, 60),
            Size = new Size(60, 60),
            Location = new Point(170, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var title = new Label
        {
            Text = "不燃性试验系统",
            Font = new Font("Microsoft YaHei", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(248, 250, 252),
            AutoSize = true,
            Location = new Point(110, 85)
        };

        var subtitle = new Label
        {
            Text = "ISO 11820 建筑材料不燃性试验仿真平台",
            Font = new Font("Microsoft YaHei", 9),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(75, 120)
        };

        var sep = new Panel
        {
            Size = new Size(320, 1),
            Location = new Point(40, 150),
            BackColor = Color.FromArgb(51, 65, 85)
        };

        _rbAdmin = new RadioButton
        {
            Text = "管理员",
            Font = new Font("Microsoft YaHei", 11),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(60, 170),
            Size = new Size(110, 30),
            Checked = true,
            FlatStyle = FlatStyle.Flat
        };

        _rbExperimenter = new RadioButton
        {
            Text = "试验员",
            Font = new Font("Microsoft YaHei", 11),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(200, 170),
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat
        };

        _txtPwd = new TextBox
        {
            Location = new Point(60, 220),
            Size = new Size(280, 36),
            Font = new Font("Microsoft YaHei", 13),
            PasswordChar = '●',
            BackColor = Color.FromArgb(51, 65, 85),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = HorizontalAlignment.Center
        };
        _txtPwd.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

        _btnLogin = new Button
        {
            Text = "登 录 系 统",
            Location = new Point(60, 275),
            Size = new Size(280, 42),
            Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
        _btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
        _btnLogin.Click += (s, e) => DoLogin();

        _lblError = new Label
        {
            Text = "",
            Font = new Font("Microsoft YaHei", 9),
            ForeColor = Color.FromArgb(252, 165, 165),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(280, 20),
            Location = new Point(60, 325)
        };

        var hint = new Label
        {
            Text = "默认密码: 123456",
            Font = new Font("Microsoft YaHei", 8),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(280, 20),
            Location = new Point(80, 322)
        };

        card.Controls.Add(icon);
        card.Controls.Add(title);
        card.Controls.Add(subtitle);
        card.Controls.Add(sep);
        card.Controls.Add(_rbAdmin);
        card.Controls.Add(_rbExperimenter);
        card.Controls.Add(_txtPwd);
        card.Controls.Add(_btnLogin);
        card.Controls.Add(_lblError);

        var footer = new Label
        {
            Text = "Build on .NET 8 | SQLite | Simulation Mode",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true,
            Location = new Point(160, 400)
        };

        this.Controls.Add(card);
        this.Controls.Add(footer);
    }

    private void DoLogin()
    {
        _lblError.Text = "";
        string pwd = _txtPwd.Text.Trim();
        if (string.IsNullOrEmpty(pwd))
        {
            _lblError.Text = "请输入访问口令";
            return;
        }

        string role = _rbAdmin.Checked ? "管理员" : "试验员";
        string username = _rbAdmin.Checked ? "admin" : "experimenter";

        if (AppCtx.Instance.Db.Login(username, pwd, out string userid, out string usertype))
        {
            AppCtx.Instance.CurrentUserId = userid;
            AppCtx.Instance.CurrentUserName = username;
            AppCtx.Instance.CurrentUserType = usertype;
            AppCtx.Instance.OperatorRole = role;

            var mainForm = new MainForm();
            mainForm.FormClosed += (s, e) => this.Close();
            mainForm.Show();
            this.Hide();
        }
        else
        {
            _lblError.Text = "密码错误，请重新输入";
            _txtPwd.SelectAll();
            _txtPwd.Focus();
        }
    }
}
