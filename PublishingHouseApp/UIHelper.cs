using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    // Фабрика стандартных UI-элементов в едином стиле приложения.
    // Все кнопки, таблицы и поля ввода создаются через этот класс —
    // так оформление остаётся одинаковым во всём приложении.
    public static class UIHelper
    {
        // ── Кнопки ───────────────────────────────────────────────────────────

        // Синяя кнопка для основных действий (Добавить, Сохранить, Войти)
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
            // Эффект подсветки при наведении мыши
            btn.MouseEnter += (s, e) => btn.BackColor = AppColors.ButtonPrimaryHover;
            btn.MouseLeave += (s, e) => btn.BackColor = AppColors.ButtonPrimary;
            return btn;
        }

        // Красная кнопка для деструктивных действий (Удалить)
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

        // Таблица в едином стиле: тёмный заголовок, чередование строк, без лишних границ
        public static DataGridView MakeGrid()
        {
            var grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,         // редактирование через панель справа
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,        // одна строка за раз
                BorderStyle           = BorderStyle.None,
                BackgroundColor       = AppColors.PanelBackground,
                GridColor             = AppColors.PanelBorder,
                Font                  = new Font("Segoe UI", 9f),
                RowHeadersVisible     = false,        // убираем нумерацию строк
                EnableHeadersVisualStyles = false     // применяем свои стили заголовка
            };

            // Оформление заголовка таблицы
            grid.ColumnHeadersDefaultCellStyle.BackColor  = AppColors.GridHeader;
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = AppColors.GridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersHeight = 36;

            // Чередование цвета строк для удобства чтения
            grid.AlternatingRowsDefaultCellStyle.BackColor = AppColors.GridRowAlt;

            // Цвет выделенной строки — голубой вместо системного синего
            grid.DefaultCellStyle.SelectionBackColor = AppColors.GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            return grid;
        }

        // ── Labels ────────────────────────────────────────────────────────────

        // Крупный заголовок раздела (например «Управление авторами»)
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

        // Обычная подпись для полей ввода
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

        // Поле ввода; параметр password=true делает маску для пароля
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

        // ── Диалоговые окна ───────────────────────────────────────────────────

        // Диалог подтверждения действия — возвращает true если пользователь нажал «Да»
        public static bool Confirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // Окно с сообщением об ошибке
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Информационное окно
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
