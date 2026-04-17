using System;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    /// <summary>
    /// Базовая форма для Admin / Editor / Manager.
    /// Содержит: шапку, боковую навигацию, контентную область.
    /// Дочерние формы добавляют кнопки навигации и панели контента.
    /// </summary>
    public class BaseMainForm : Form
    {
        // ── Layout panels ─────────────────────────────────────────────────────
        protected Panel pnlHeader;
        protected Panel pnlNav;
        protected Panel pnlContent;

        // ── Header controls ───────────────────────────────────────────────────
        private Label lblAppTitle;
        protected Label lblRoleName;

        // ── Nav ───────────────────────────────────────────────────────────────
        private Button _activeNavBtn;

        protected BaseMainForm(string roleName, string windowTitle)
        {
            InitializeLayout(roleName, windowTitle);
        }

        private void InitializeLayout(string roleName, string windowTitle)
        {
            // ── Form ─────────────────────────────────────────────────────────
            Text            = windowTitle + " — ИС «Просвещение»";
            Size            = new Size(1200, 700);
            MinimumSize     = new Size(1000, 600);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = AppColors.FormBackground;
            WindowState     = FormWindowState.Maximized;

            // ── Header (top bar) ─────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = AppColors.NavBackground
            };

            lblAppTitle = new Label
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
            // Will be right-aligned after load
            pnlHeader.SizeChanged += (s, e) =>
                lblRoleName.Location = new Point(pnlHeader.Width - lblRoleName.Width - 70, 18);

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
            pnlHeader.SizeChanged += (s, e) =>
                btnLogout.Location = new Point(pnlHeader.Width - btnLogout.Width - 10, 13);
            btnLogout.Click += (s, e) => Close();

            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblRoleName);
            pnlHeader.Controls.Add(btnLogout);

            // ── Side navigation ──────────────────────────────────────────────
            pnlNav = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 210,
                BackColor = AppColors.NavBackground,
                Padding   = new Padding(0, 10, 0, 0)
            };

            // ── Content area ─────────────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = AppColors.FormBackground,
                Padding   = new Padding(20)
            };

            Controls.Add(pnlContent);
            Controls.Add(pnlNav);
            Controls.Add(pnlHeader);
        }

        // ── Navigation button factory ─────────────────────────────────────────

        /// <summary>
        /// Создаёт кнопку бокового меню навигации.
        /// При клике переключает активную панель контента.
        /// </summary>
        protected Button MakeNavButton(string text, Panel targetPanel)
        {
            var btn = new Button
            {
                Text      = "  " + text,
                Dock      = DockStyle.Top,
                Height    = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.NavBackground,
                ForeColor = AppColors.NavText,
                Font      = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize      = 0;
            btn.FlatAppearance.MouseOverBackColor = AppColors.NavHover;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != _activeNavBtn) btn.BackColor = AppColors.NavHover;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != _activeNavBtn) btn.BackColor = AppColors.NavBackground;
            };

            btn.Click += (s, e) => ActivatePanel(btn, targetPanel);

            return btn;
        }

        /// <summary>
        /// Скрывает все дочерние панели pnlContent и показывает нужную,
        /// подсвечивает активную кнопку навигации.
        /// </summary>
        protected void ActivatePanel(Button navBtn, Panel targetPanel)
        {
            // Hide all content panels
            foreach (Control c in pnlContent.Controls)
                if (c is Panel p) p.Visible = false;

            targetPanel.Visible = true;
            targetPanel.BringToFront();

            // Reset previous active button
            if (_activeNavBtn != null)
            {
                _activeNavBtn.BackColor = AppColors.NavBackground;
                _activeNavBtn.Font      = new Font("Segoe UI", 10f);
            }

            // Highlight new active button
            navBtn.BackColor = AppColors.NavSelected;
            navBtn.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
            _activeNavBtn    = navBtn;
        }

        /// <summary>
        /// Добавляет панель в контентную область (скрытой по умолчанию)
        /// </summary>
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
