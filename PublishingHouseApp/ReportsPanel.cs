using System;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    /// <summary>
    /// Переиспользуемая панель раздела «Отчёты».
    /// Встраивается в AdminForm и ManagerForm.
    /// </summary>
    public class ReportsPanel : Panel
    {
        public ReportsPanel()
        {
            BackColor = AppColors.FormBackground;
            Dock      = DockStyle.Fill;
            Padding   = new Padding(24);
            Build();
        }

        private void Build()
        {
            // ── Заголовок ─────────────────────────────────────────────────────
            var lblTitle = UIHelper.MakeSectionTitle("Формирование отчётов");
            lblTitle.Location = new Point(0, 0);
            Controls.Add(lblTitle);

            var sep = new Panel
            {
                BackColor = AppColors.PanelBorder,
                Size      = new Size(700, 1),
                Location  = new Point(0, 38)
            };
            Controls.Add(sep);

            int y = 52;

            // ── Блок 1: Общий отчёт ───────────────────────────────────────────
            y = AddSection("Сводный отчёт", y);
            y = AddReportCard(
                title:       "Общий отчёт по изданиям",
                description: "Все издания с авторами, договорами, этапами, экспертизами и тиражами.",
                buttonText:  "Сформировать",
                y:           y,
                onClick:     () => SafeRun(ReportHelper.GenerateFullReport));

            // ── Блок 2: Отчёт за период ───────────────────────────────────────
            y += 8;
            y = AddSection("Отчёт за период", y);
            y = BuildPeriodBlock(y);

            // ── Блок 3: Отчёты по таблицам ───────────────────────────────────
            y += 8;
            y = AddSection("Отчёты по разделам", y);

            var reports = new[]
            {
                ("Авторы",              "Полный список авторов с количеством договоров.",
                 (Action)(() => SafeRun(ReportHelper.GenerateAuthorsReport))),
                ("Договоры",            "Все договоры с авторами и связанными изданиями.",
                 (Action)(() => SafeRun(ReportHelper.GenerateContractsReport))),
                ("Издания",             "Список всех изданий с типом, тематикой и автором.",
                 (Action)(() => SafeRun(ReportHelper.GeneratePublicationsReport))),
                ("Этапы подготовки",    "Этапы по всем изданиям с цветовой индикацией статуса.",
                 (Action)(() => SafeRun(ReportHelper.GenerateStagesReport))),
                ("Экспертизы",          "Экспертизы по изданиям с датами и результатами.",
                 (Action)(() => SafeRun(ReportHelper.GenerateExpertiseReport))),
                ("Тиражи",              "Тиражи по изданиям с итоговой суммой.",
                 (Action)(() => SafeRun(ReportHelper.GeneratePrintRunsReport))),
            };

            // Карточки в две колонки
            int col = 0;
            int rowX = y;
            foreach (var (title, desc, action) in reports)
            {
                int cardX = col == 0 ? 0 : 370;
                AddReportCardAt(title, desc, "Сформировать", rowX, cardX, action);
                col++;
                if (col == 2) { col = 0; rowX += 90; }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int AddSection(string title, int y)
        {
            var lbl = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true,
                Location  = new Point(0, y)
            };
            Controls.Add(lbl);
            return y + 28;
        }

        private int AddReportCard(string title, string description,
                                   string buttonText, int y, Action onClick)
        {
            var card = MakeCard(title, description, buttonText, y, 0, onClick);
            Controls.Add(card);
            return y + card.Height + 10;
        }

        private void AddReportCardAt(string title, string description,
                                      string buttonText, int y, int x, Action onClick)
        {
            var card = MakeCard(title, description, buttonText, y, x, onClick);
            Controls.Add(card);
        }

        private Panel MakeCard(string title, string description,
                                string buttonText, int y, int x, Action onClick)
        {
            var card = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(355, 76),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(AppColors.PanelBorder), 0, 0, card.Width - 1, card.Height - 1);

            // Accent bar
            var accent = new Panel
            {
                BackColor = AppColors.ButtonPrimary,
                Size      = new Size(4, 76),
                Location  = new Point(0, 0)
            };

            var lblTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location  = new Point(14, 10),
                AutoSize  = true
            };

            var lblDesc = new Label
            {
                Text      = description,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                Location  = new Point(14, 32),
                Size      = new Size(220, 32),
                AutoSize  = false
            };

            var btn = UIHelper.MakePrimaryButton(buttonText, 100, 28);
            btn.Location = new Point(card.Width - 116, 24);
            btn.Click   += (s, e) => onClick();

            card.Controls.Add(accent);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(btn);

            return card;
        }

        // ── Period block ──────────────────────────────────────────────────────

        private int BuildPeriodBlock(int y)
        {
            var card = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(730, 110),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(AppColors.PanelBorder), 0, 0, card.Width - 1, card.Height - 1);

            var accent = new Panel { BackColor = AppColors.NavSelected, Size = new Size(4, 110), Location = new Point(0, 0) };

            // Row 1 — dates
            var lblFrom = new Label { Text = "Дата с:", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(14, 12), AutoSize = true };
            var dtpFrom = new DateTimePicker { Location = new Point(60, 8), Width = 130, Font = new Font("Segoe UI", 9f), Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Now.Year, 1, 1) };

            var lblTo = new Label { Text = "по:", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(200, 12), AutoSize = true };
            var dtpTo = new DateTimePicker { Location = new Point(220, 8), Width = 130, Font = new Font("Segoe UI", 9f), Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            // Row 2 — years
            var lblYearFrom = new Label { Text = "Год тиража с:", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(14, 50), AutoSize = true };
            var tbYearFrom  = new TextBox { Location = new Point(105, 46), Width = 60, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, Text = DateTime.Now.Year.ToString() };

            var lblYearTo = new Label { Text = "по:", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(175, 50), AutoSize = true };
            var tbYearTo  = new TextBox { Location = new Point(196, 46), Width = 60, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, Text = DateTime.Now.Year.ToString() };

            var chkYears = new CheckBox
            {
                Text      = "Включить фильтр по тиражам",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextPrimary,
                Location  = new Point(270, 50),
                AutoSize  = true,
                Checked   = true,
                Cursor    = Cursors.Hand
            };
            tbYearFrom.Enabled = tbYearTo.Enabled = chkYears.Checked;
            chkYears.CheckedChanged += (s, e) => tbYearFrom.Enabled = tbYearTo.Enabled = chkYears.Checked;

            var btn = UIHelper.MakePrimaryButton("Сформировать", 130, 30);
            btn.Location = new Point(card.Width - 148, 38);
            btn.Click += (s, e) =>
            {
                int? yf = null, yt = null;
                if (chkYears.Checked)
                {
                    if (!int.TryParse(tbYearFrom.Text, out int yfi) || !int.TryParse(tbYearTo.Text, out int yti))
                    { UIHelper.ShowError("Введите корректные годы."); return; }
                    if (yfi > yti) { UIHelper.ShowError("Год «с» не может быть больше года «по»."); return; }
                    yf = yfi; yt = yti;
                }
                if (dtpFrom.Value > dtpTo.Value)
                { UIHelper.ShowError("Дата «с» не может быть позже даты «по»."); return; }
                SafeRun(() => ReportHelper.GeneratePeriodReport(dtpFrom.Value, dtpTo.Value, yf, yt));
            };

            card.Controls.AddRange(new Control[] { accent, lblFrom, dtpFrom, lblTo, dtpTo, lblYearFrom, tbYearFrom, lblYearTo, tbYearTo, chkYears, btn });
            Controls.Add(card);
            return y + card.Height + 10;
        }

        private static void SafeRun(Action action)
        {
            try { action(); }
            catch (Exception ex) { UIHelper.ShowError($"Ошибка при формировании отчёта:\n{ex.Message}"); }
        }
    }
}
