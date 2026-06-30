using ISO11820.Data;
using ISO11820.Global;

namespace ISO11820.Forms;

public class LoginForm : Form
{
    private readonly TextBox _txtPwd = new();
    private readonly RadioButton _rbAdmin = new();
    private readonly RadioButton _rbExperimenter = new();
    private readonly Button _btnLogin = new();
    private readonly Label _lblTitle = new();
    private readonly Label _lblError = new();

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 建筑材料不燃性试验系统 - 登录";
        this.Size = new Size(420, 320);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(0x0F, 0x17, 0x2A);
        this.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);

        _lblTitle.Text = "ISO 11820 试验系统";
        _lblTitle.Font = new Font("微软雅黑", 18, FontStyle.Bold);
        _lblTitle.ForeColor = Color.FromArgb(0x3B, 0x82, 0xF6);
        _lblTitle.Size = new Size(350, 50);
        _lblTitle.Location = new Point(35, 30);
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        _rbAdmin.Text = "管理员 (admin)";
        _rbAdmin.Location = new Point(60, 100);
        _rbAdmin.Size = new Size(130, 30);
        _rbAdmin.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _rbAdmin.Checked = true;

        _rbExperimenter.Text = "试验员 (experimenter)";
        _rbExperimenter.Location = new Point(210, 100);
        _rbExperimenter.Size = new Size(150, 30);
        _rbExperimenter.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);

        var lblPwd = new Label();
        lblPwd.Text = "密码：";
        lblPwd.Location = new Point(60, 150);
        lblPwd.Size = new Size(50, 30);
        lblPwd.ForeColor = Color.FromArgb(0x94, 0xA3, 0xB8);

        _txtPwd.Location = new Point(120, 147);
        _txtPwd.Size = new Size(200, 30);
        _txtPwd.UseSystemPasswordChar = true;
        _txtPwd.BackColor = Color.FromArgb(0x33, 0x41, 0x55);
        _txtPwd.ForeColor = Color.FromArgb(0xF8, 0xFA, 0xFC);
        _txtPwd.BorderStyle = BorderStyle.FixedSingle;
        _txtPwd.Text = "123456";

        _btnLogin.Text = "登 录";
        _btnLogin.Location = new Point(140, 200);
        _btnLogin.Size = new Size(120, 40);
        _btnLogin.BackColor = Color.FromArgb(0x3B, 0x82, 0xF6);
        _btnLogin.ForeColor = Color.White;
        _btnLogin.FlatStyle = FlatStyle.Flat;
        _btnLogin.Font = new Font("微软雅黑", 12, FontStyle.Bold);
        _btnLogin.Click += BtnLogin_Click;

        _lblError.Text = "";
        _lblError.ForeColor = Color.FromArgb(0xEF, 0x44, 0x44);
        _lblError.Location = new Point(60, 250);
        _lblError.Size = new Size(300, 30);
        _lblError.TextAlign = ContentAlignment.MiddleCenter;

        this.Controls.AddRange(new Control[] { _lblTitle, _rbAdmin, _rbExperimenter,
            lblPwd, _txtPwd, _btnLogin, _lblError });
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        string username = _rbAdmin.Checked ? "admin" : "experimenter";
        string pwd = _txtPwd.Text;

        if (AppCtx.Db.Login(username, pwd, out string userId, out string userType))
        {
            AppCtx.CurrentUserId = userId;
            AppCtx.CurrentUserName = username;
            AppCtx.CurrentUserType = userType;

            var mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
        else
        {
            _lblError.Text = "密码错误，请重新输入";
            _txtPwd.Focus();
        }
    }
}