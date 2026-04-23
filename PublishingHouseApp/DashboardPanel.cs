using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    /// <summary>
    /// Панель «Главная» — общая статистика для всех ролей.
    /// </summary>
    public class DashboardPanel : Panel
    {
        private readonly UserRole _role;
        private FlowLayoutPanel _cardsFlow;
        private Panel           _contractsPanel;
        private DataGridView    _expiringGrid;

        public DashboardPanel(UserRole role)
        {
            _role     = role;
            BackColor = AppColors.FormBackground;
            Dock      = DockStyle.Fill;
            Padding   = new Padding(24, 20, 24, 20);
            Build();
        }

        // ══════════════════════════════════════════════════════════════════════
        // BUILD
        // ══════════════════════════════════════════════════════════════════════
        private void Build()
        {
            // ── Заголовок ─────────────────────────────────────────────────────
            var lblTitle = UIHelper.MakeSectionTitle("Главная");
            lblTitle.Location = new Point(24, 20);

            var lblSub = new Label
            {
                Text      = $"Добро пожаловать, {AppUsers.GetRoleDisplayName(_role)}  •  {DateTime.Now:dd MMMM yyyy}",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true,
                Location  = new Point(24, 52)
            };

            var sep = new Panel { BackColor = AppColors.PanelBorder, Size = new Size(900, 1), Location = new Point(24, 76) };

            // ── Карточки статистики ───────────────────────────────────────────
            _cardsFlow = new FlowLayoutPanel
            {
                Location      = new Point(24, 88),
                Size          = new Size(900, 110),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = AppColors.FormBackground
            };

            // ── Таблица истекающих договоров ──────────────────────────────────
            var lblContracts = new Label
            {
                Text      = "Договоры, истекающие в ближайшие 30 дней",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize  = true,
                Location  = new Point(24, 216)
            };

            _expiringGrid = UIHelper.MakeGrid();
            _contractsPanel = new Panel
            {
                Location  = new Point(24, 244),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.White
            };
            _contractsPanel.Controls.Add(_expiringGrid);

            // Адаптируем размеры при ресайзе
            SizeChanged += (s, e) =>
            {
                _cardsFlow.Width      = Width - 48;
                lblContracts.Width    = Width - 48;
                _contractsPanel.Size  = new Size(Width - 48, Height - 270);
            };

            Controls.Add(lblTitle);
            Controls.Add(lblSub);
            Controls.Add(sep);
            Controls.Add(_cardsFlow);
            Controls.Add(lblContracts);
            Controls.Add(_contractsPanel);

            LoadData();
        }

        // ══════════════════════════════════════════════════════════════════════
        // LOAD
        // ══════════════════════════════════════════════════════════════════════
        public void LoadData()
        {
            try
            {
                _cardsFlow.Controls.Clear();
                LoadStatCards();
                LoadExpiringContracts();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Ошибка загрузки главной страницы:\n{ex.Message}");
            }
        }

        private void LoadStatCards()
        {
            // Пользователи (роли)
            AddStatCard("Пользователи", "3", "роли в системе",
                AppColors.NavSelected, "👤");

            // Авторы
            int authorsCount = GetScalarInt("SELECT COUNT(*) FROM Author");
            AddStatCard("Авторы", authorsCount.ToString(), "зарегистрировано",
                Color.FromArgb(39, 174, 96), "✍");

            // Договоры
            int contractsCount = GetScalarInt("SELECT COUNT(*) FROM Contract");
            AddStatCard("Договоры", contractsCount.ToString(), "всего договоров",
                Color.FromArgb(142, 68, 173), "📄");

            // Ближайший истекающий договор
            string nearestContract = GetNearestExpiringContract();
            AddStatCard("Ближайший договор", nearestContract, "истекает скорее всего",
                Color.FromArgb(230, 126, 34), "⚠");

            // Издания
            int pubCount = GetScalarInt("SELECT COUNT(*) FROM Publication");
            AddStatCard("Издания", pubCount.ToString(), "в базе",
                Color.FromArgb(41, 128, 185), "📚");

            // Тиражи — суммарный
            int totalPrint = GetScalarInt("SELECT ISNULL(SUM(quantity),0) FROM PrintRun");
            AddStatCard("Суммарный тираж", totalPrint.ToString("N0"), "экз. всего",
                Color.FromArgb(22, 160, 133), "🖨");
        }

        private void LoadExpiringContracts()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT
                    a.surname+' '+a.name    AS [Автор],
                    c.signing_date          AS [Дата подписания],
                    c.valid_until           AS [Действует до],
                    c.amount                AS [Сумма],
                    ISNULL(p.title,'—')     AS [Издание],
                    DATEDIFF(day, GETDATE(), c.valid_until) AS [Дней осталось]
                FROM Contract c
                JOIN Author a ON a.author_id = c.author_id
                LEFT JOIN Publication p ON p.contract_id = c.contract_id
                WHERE c.valid_until BETWEEN CAST(GETDATE() AS date)
                                        AND DATEADD(day, 30, CAST(GETDATE() AS date))
                ORDER BY c.valid_until ASC");

            _expiringGrid.DataSource = dt;

            // Цветовая индикация по количеству дней
            _expiringGrid.RowPrePaint -= OnRowPrePaint;
            _expiringGrid.RowPrePaint += OnRowPrePaint;

            if (dt.Rows.Count == 0)
            {
                // Показываем сообщение что нет истекающих договоров
                _expiringGrid.DataSource = null;
                var emptyLabel = new Label
                {
                    Text      = "✓  Нет договоров, истекающих в ближайшие 30 дней.",
                    Font      = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(39, 174, 96),
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                _contractsPanel.Controls.Clear();
                _contractsPanel.Controls.Add(emptyLabel);
            }
            else
            {
                _contractsPanel.Controls.Clear();
                _contractsPanel.Controls.Add(_expiringGrid);
            }
        }

        private void OnRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                var grid = (DataGridView)sender;
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
                var row = grid.Rows[e.RowIndex];
                if (!grid.Columns.Contains("Дней осталось")) return;
                var val = row.Cells["Дней осталось"].Value;
                if (val == null || val == DBNull.Value) return;
                int days = Convert.ToInt32(val);
                if (days <= 7)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);  // красноватый
                else if (days <= 14)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 219);  // жёлтый
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(232, 246, 243);  // зелёный
            }
            catch { /* не критично */ }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private void AddStatCard(string title, string value, string subtitle,
                                  Color accentColor, string icon)
        {
            var card = new Panel
            {
                Size      = new Size(172, 90),
                BackColor = Color.White,
                Margin    = new Padding(0, 0, 12, 0),
                Cursor    = Cursors.Default
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(AppColors.PanelBorder), 0, 0, card.Width - 1, card.Height - 1);

            // Accent top bar
            var topBar = new Panel
            {
                BackColor = accentColor,
                Size      = new Size(card.Width, 4),
                Location  = new Point(0, 0)
            };

            var lblIcon = new Label
            {
                Text      = icon,
                Font      = new Font("Segoe UI", 18f),
                Location  = new Point(card.Width - 44, 12),
                Size      = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Адаптивный размер шрифта — чем длиннее текст, тем меньше шрифт
            float valueFontSize = value.Length <= 6  ? 18f :
                                  value.Length <= 12 ? 13f :
                                  value.Length <= 20 ? 10f : 8.5f;

            var lblValue = new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", valueFontSize, FontStyle.Bold),
                ForeColor = accentColor,
                Location  = new Point(10, 14),
                Size      = new Size(card.Width - 50, 34),
                AutoSize  = false,
                AutoEllipsis = true
            };

            var lblTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location  = new Point(10, 52),
                Size      = new Size(card.Width - 16, 16),
                AutoSize  = false,
                AutoEllipsis = true
            };

            var lblSub = new Label
            {
                Text      = subtitle,
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = AppColors.TextSecondary,
                Location  = new Point(10, 70),
                Size      = new Size(card.Width - 16, 14),
                AutoSize  = false,
                AutoEllipsis = true
            };

            card.Controls.Add(topBar);
            card.Controls.Add(lblIcon);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSub);

            _cardsFlow.Controls.Add(card);
        }

        private static int GetScalarInt(string sql)
        {
            try
            {
                var result = DatabaseHelper.ExecuteScalar(sql);
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch { return 0; }
        }

        private static string GetNearestExpiringContract()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT TOP 1
                        a.surname+' '+a.name AS fio,
                        c.valid_until
                    FROM Contract c
                    JOIN Author a ON a.author_id = c.author_id
                    WHERE c.valid_until >= CAST(GETDATE() AS date)
                    ORDER BY c.valid_until ASC");

                if (dt.Rows.Count == 0) return "нет данных";

                var row  = dt.Rows[0];
                string fio  = row["fio"].ToString();
                var date = Convert.ToDateTime(row["valid_until"]);
                int days = (date - DateTime.Today).Days;
                // Укорачиваем ФИО до фамилии + инициалов если длинное
                var parts = fio.Split(' ');
                string shortFio = parts.Length >= 2
                    ? $"{parts[0]} {parts[1][0]}."
                    : fio;
                return $"{shortFio} ({days} дн.)";
            }
            catch { return "ошибка"; }
        }
    }
}
