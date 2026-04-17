using System;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public class AuthForm : Form
    {
        // ── UI controls ───────────────────────────────────────────────────────
        private Panel     pnlLeft;
        private Panel     pnlRight;
        private Label     lblTitle;
        private Label     lblSubtitle;
        private Label     lblLogin;
        private Label     lblPassword;
        private TextBox   txtLogin;
        private TextBox   txtPassword;
        private Button    btnLogin;
        private Label     lblError;
        private CheckBox  chkShowPassword;

        public AuthForm()
        {
            InitializeComponent();
            CheckDatabaseConnection();
        }

        private void InitializeComponent()
        {
            // ── Form ─────────────────────────────────────────────────────────
            Text            = "Авторизация — ИС «Просвещение»";
            Size            = new Size(780, 480);
            MinimumSize     = new Size(780, 480);
            MaximumSize     = new Size(780, 480);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = AppColors.FormBackground;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;

            // ── Left decorative panel ────────────────────────────────────────
            pnlLeft = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 340,
                BackColor = AppColors.NavBackground
            };

            lblTitle = new Label
            {
                Text      = "ИС «Просвещение»",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                Width     = 300,
                Height    = 60,
                Location  = new Point(20, 140),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitle = new Label
            {
                Text      = "Учёт работы издательства",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(189, 215, 234),
                AutoSize  = false,
                Width     = 300,
                Height    = 36,
                Location  = new Point(20, 205),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Decorative accent line
            var accent = new Panel
            {
                BackColor = AppColors.NavSelected,
                Size      = new Size(60, 4),
                Location  = new Point(20, 130)
            };

            pnlLeft.Controls.Add(lblTitle);
            pnlLeft.Controls.Add(lblSubtitle);
            pnlLeft.Controls.Add(accent);

            // ── Right login panel ────────────────────────────────────────────
            pnlRight = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Padding   = new Padding(40, 0, 40, 0)
            };

            var lblSignIn = new Label
            {
                Text      = "Вход в систему",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize  = true,
                Location  = new Point(40, 90)
            };

            lblLogin = new Label
            {
                Text      = "Логин",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true,
                Location  = new Point(40, 150)
            };

            txtLogin = new TextBox
            {
                Location    = new Point(40, 170),
                Width       = 300,
                Height      = 32,
                Font        = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = AppColors.FormBackground,
                ForeColor   = AppColors.TextPrimary
            };

            lblPassword = new Label
            {
                Text      = "Пароль",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true,
                Location  = new Point(40, 215)
            };

            txtPassword = new TextBox
            {
                Location     = new Point(40, 235),
                Width        = 300,
                Height       = 32,
                Font         = new Font("Segoe UI", 11f),
                BorderStyle  = BorderStyle.FixedSingle,
                BackColor    = AppColors.FormBackground,
                ForeColor    = AppColors.TextPrimary,
                PasswordChar = '●'
            };

            chkShowPassword = new CheckBox
            {
                Text      = "Показать пароль",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true,
                Location  = new Point(40, 272),
                Cursor    = Cursors.Hand
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';

            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppColors.ButtonDanger,
                AutoSize  = true,
                Location  = new Point(40, 300)
            };

            btnLogin = new Button
            {
                Text      = "Войти",
                Location  = new Point(40, 330),
                Width     = 300,
                Height    = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.ButtonPrimary,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = AppColors.ButtonPrimaryHover;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = AppColors.ButtonPrimary;
            btnLogin.Click += BtnLogin_Click;

            // Enter key submits form
            txtLogin.KeyDown    += TxtKeyDown;
            txtPassword.KeyDown += TxtKeyDown;

            pnlRight.Controls.Add(lblSignIn);
            pnlRight.Controls.Add(lblLogin);
            pnlRight.Controls.Add(txtLogin);
            pnlRight.Controls.Add(lblPassword);
            pnlRight.Controls.Add(txtPassword);
            pnlRight.Controls.Add(chkShowPassword);
            pnlRight.Controls.Add(lblError);
            pnlRight.Controls.Add(btnLogin);

            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);

            AcceptButton = btnLogin;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void TxtKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) BtnLogin_Click(sender, e);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Введите логин и пароль.";
                return;
            }

            UserRole role = AppUsers.Authenticate(login, password);

            if (role == UserRole.None)
            {
                lblError.Text = "Неверный логин или пароль.";
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            OpenRoleForm(role);
        }

        private void OpenRoleForm(UserRole role)
        {
            Form nextForm = null;

            switch (role)
            {
                case UserRole.Admin:
                    nextForm = new AdminForm();
                    break;
                case UserRole.Editor:
                    nextForm = new EditorForm();
                    break;
                case UserRole.Manager:
                    nextForm = new ManagerForm();
                    break;
            }

            if (nextForm == null) return;

            Hide();
            nextForm.FormClosed += (s, e) =>
            {
                txtLogin.Clear();
                txtPassword.Clear();
                lblError.Text = "";
                Show();
            };
            nextForm.Show();
        }

        // ── Database connection check ─────────────────────────────────────────

        private void CheckDatabaseConnection()
        {
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных.\n\n" +
                    "Убедитесь, что SQL Server LocalDB запущен и база данных PublishingHouse01 существует.",
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
