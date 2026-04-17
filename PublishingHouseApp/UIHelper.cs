using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    /// <summary>
    /// Фабрика стандартных UI-элементов в едином стиле приложения
    /// </summary>
    public static class UIHelper
    {
        // ── Кнопки ───────────────────────────────────────────────────────────

        public static Button MakePrimaryButton(string text, int width = 130, int height = 34)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.ButtonPrimary,
                ForeColor = AppColors.ButtonText,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = AppColors.ButtonPrimaryHover;
            btn.MouseLeave += (s, e) => btn.BackColor = AppColors.ButtonPrimary;
            return btn;
        }

        public static Button MakeDangerButton(string text, int width = 130, int height = 34)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.ButtonDanger,
                ForeColor = AppColors.ButtonText,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = AppColors.ButtonDangerHover;
            btn.MouseLeave += (s, e) => btn.BackColor = AppColors.ButtonDanger;
            return btn;
        }

        // ── DataGridView ──────────────────────────────────────────────────────

        public static DataGridView MakeGrid()
        {
            var grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BorderStyle           = BorderStyle.None,
                BackgroundColor       = AppColors.PanelBackground,
                GridColor             = AppColors.PanelBorder,
                Font                  = new Font("Segoe UI", 9f),
                RowHeadersVisible     = false,
                EnableHeadersVisualStyles = false
            };

            // Заголовок
            grid.ColumnHeadersDefaultCellStyle.BackColor  = AppColors.GridHeader;
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = AppColors.GridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersHeight = 36;

            // Чередование строк
            grid.AlternatingRowsDefaultCellStyle.BackColor = AppColors.GridRowAlt;

            // Выделение
            grid.DefaultCellStyle.SelectionBackColor = AppColors.GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            return grid;
        }

        // ── Labels ────────────────────────────────────────────────────────────

        public static Label MakeSectionTitle(string text)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.SectionTitle,
                AutoSize  = true
            };
        }

        public static Label MakeLabel(string text)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextPrimary,
                AutoSize  = true
            };
        }

        // ── TextBox ───────────────────────────────────────────────────────────

        public static TextBox MakeTextBox(int width = 220, bool password = false)
        {
            var tb = new TextBox
            {
                Width       = width,
                Height      = 28,
                Font        = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White,
                ForeColor   = AppColors.TextPrimary
            };
            if (password) tb.PasswordChar = '●';
            return tb;
        }

        // ── Panel helpers ─────────────────────────────────────────────────────

        public static Panel MakeContentPanel()
        {
            return new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = AppColors.PanelBackground,
                Padding   = new Padding(16)
            };
        }

        // ── Confirm dialog ────────────────────────────────────────────────────

        public static bool Confirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
