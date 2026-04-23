using System;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public class BaseMainForm : Form
    {
        protected Panel            pnlHeader;
        protected FlowLayoutPanel  pnlNav;      // FlowLayout — порядок кнопок = порядок добавления
        protected Panel            pnlContent;
        protected Label            lblRoleName;

        private Button    _activeNavBtn;
        private readonly UserRole _role;

        protected BaseMainForm(string roleName, string windowTitle, UserRole role)
        {
            _role = role;
            InitializeLayout(roleName, windowTitle);
        }

        private void InitializeLayout(string roleName, string windowTitle)
        {
            Text            = windowTitle + " — ИС «Просвещение»";
            Size            = new Size(1200, 700);
            MinimumSize     = new Size(1000, 600);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = AppColors.FormBackground;
            WindowState     = FormWindowState.Maximized;

            // ── Header ────────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = AppColors.NavBackground
            };

            var lblApp = new Label
            {
                Text      = "ИС «Просвещение»",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(20, 14)
            };

            lblRoleName = new Label
            {
                Text      = roleName,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(189, 215, 234),
                AutoSize  = true
            };
            pnlHeader.SizeChanged += (s, e) =>
                lblRoleName.Location = new Point(pnlHeader.Width - lblRoleName.Width - 90, 18);

            var btnLogout = new Button
            {
                Text      = "Выйти",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f),
                Size      = new Size(70, 30),
                Cursor    = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = Color.FromArgb(231, 76, 60);
            btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = Color.FromArgb(192, 57, 43);
            pnlHeader.SizeChanged += (s, e) =>
                btnLogout.Location = new Point(pnlHeader.Width - btnLogout.Width - 12, 13);
            btnLogout.Click += (s, e) => Close();

            pnlHeader.Controls.Add(lblApp);
            pnlHeader.Controls.Add(lblRoleName);
            pnlHeader.Controls.Add(btnLogout);

            // ── Nav — FlowLayoutPanel сверху вниз, порядок = порядок добавления ──
            pnlNav = new FlowLayoutPanel
            {
                Dock          = DockStyle.Left,
                Width         = 210,
                BackColor     = AppColors.NavBackground,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = false,
                Padding       = new Padding(0, 8, 0, 0)
            };

            // ── Content ───────────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = AppColors.FormBackground
            };

            Controls.Add(pnlContent);
            Controls.Add(pnlNav);
            Controls.Add(pnlHeader);
        }

        // ── Dashboard — добавляется первым, отображается первым ───────────────
        protected Button AddDashboardNav()
        {
            var dashPanel = AddContentPanel();
            dashPanel.Controls.Add(new DashboardPanel(_role) { Dock = DockStyle.Fill });

            var btn = MakeNavButton("🏠  Главная", dashPanel);
            pnlNav.Controls.Add(btn);
            ActivatePanel(btn, dashPanel);
            return btn;
        }

        // ── Nav button factory ────────────────────────────────────────────────
        protected Button MakeNavButton(string text, Panel targetPanel)
        {
            var btn = new Button
            {
                Text      = "  " + text,
                Width     = 210,        // фиксированная ширина вместо Dock
                Height    = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.NavBackground,
                ForeColor = AppColors.NavText,
                Font      = new Font("Segoe UI", 9.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0)
            };
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor = AppColors.NavHover;
            btn.MouseEnter += (s, e) => { if (btn != _activeNavBtn) btn.BackColor = AppColors.NavHover; };
            btn.MouseLeave += (s, e) => { if (btn != _activeNavBtn) btn.BackColor = AppColors.NavBackground; };
            btn.Click      += (s, e) => ActivatePanel(btn, targetPanel);
            return btn;
        }

        protected void ActivatePanel(Button navBtn, Panel targetPanel)
        {
            foreach (Control c in pnlContent.Controls)
                if (c is Panel p) p.Visible = false;

            targetPanel.Visible = true;
            targetPanel.BringToFront();

            if (_activeNavBtn != null)
            {
                _activeNavBtn.BackColor = AppColors.NavBackground;
                _activeNavBtn.Font      = new Font("Segoe UI", 9.5f);
            }
            navBtn.BackColor = AppColors.NavSelected;
            navBtn.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _activeNavBtn    = navBtn;
        }

        protected Panel AddContentPanel()
        {
            var panel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = AppColors.FormBackground,
                Visible   = false
            };
            pnlContent.Controls.Add(panel);
            return panel;
        }
    }
}
