using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace PublishingHouseApp
{
    /// <summary>
    /// Генерация Excel-отчётов через EPPlus 4.5.3
    /// </summary>
    public static class ReportHelper
    {
        private const string OrgName = "Издательство «Просвещение»";

        // ══════════════════════════════════════════════════════════════════════
        // PUBLIC ENTRY POINTS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Общий сводный отчёт по всем изданиям</summary>
        public static void GenerateFullReport()
        {
            string path = ChooseSavePath("Отчёт_Общий");
            if (path == null) return;

            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Общий отчёт");
                int row = WriteHeader(ws, "Сводный отчёт по изданиям");

                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        p.publication_id,
                        p.title                          AS [Название издания],
                        p.isbn                           AS [ISBN],
                        t.type_name                      AS [Тип],
                        s.subject_name                   AS [Тематика],
                        cl.class_level                   AS [Класс],
                        a.surname+' '+a.name             AS [Автор],
                        c.signing_date                   AS [Дата договора],
                        c.valid_until                    AS [Договор действует до],
                        c.amount                         AS [Сумма договора],
                        COUNT(DISTINCT ps.stage_id)      AS [Кол-во этапов],
                        COUNT(DISTINCT e.expertise_id)   AS [Кол-во экспертиз],
                        ISNULL(SUM(pr.quantity),0)        AS [Суммарный тираж]
                    FROM Publication p
                    LEFT JOIN Type     t  ON t.type_id      = p.type_id
                    LEFT JOIN Subject  s  ON s.subject_id   = p.subject_id
                    LEFT JOIN Class    cl ON cl.class_id    = p.class_id
                    LEFT JOIN Contract c  ON c.contract_id  = p.contract_id
                    LEFT JOIN Author   a  ON a.author_id    = c.author_id
                    LEFT JOIN PreparationStage ps ON ps.publication_id = p.publication_id
                    LEFT JOIN Expertise e  ON e.stage_id    = ps.stage_id
                    LEFT JOIN PrintRun  pr ON pr.publication_id = p.publication_id
                    GROUP BY p.publication_id, p.title, p.isbn,
                             t.type_name, s.subject_name, cl.class_level,
                             a.surname, a.name,
                             c.signing_date, c.valid_until, c.amount
                    ORDER BY p.title");

                row = WriteTable(ws, dt, row,
                    hide: new[] { "publication_id" });

                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт за период — по дате подписания договора ИЛИ году тиража</summary>
        public static void GeneratePeriodReport(DateTime dateFrom, DateTime dateTo,
                                                 int? yearFrom, int? yearTo)
        {
            string path = ChooseSavePath("Отчёт_За_Период");
            if (path == null) return;

            using (var pkg = CreatePackage())
            {
                string period = $"с {dateFrom:dd.MM.yyyy} по {dateTo:dd.MM.yyyy}";
                if (yearFrom.HasValue && yearTo.HasValue)
                    period += $"  /  тиражи {yearFrom}–{yearTo}";

                // ── Лист 1: Договоры за период ────────────────────────────────
                var ws1 = AddSheet(pkg, "Договоры");
                int r1 = WriteHeader(ws1, $"Договоры за период {period}");

                var dtContracts = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        c.contract_id,
                        a.surname+' '+a.name  AS [Автор],
                        c.signing_date        AS [Дата подписания],
                        c.valid_until         AS [Действует до],
                        c.amount              AS [Сумма],
                        p.title               AS [Издание]
                    FROM Contract c
                    JOIN Author      a ON a.author_id    = c.author_id
                    LEFT JOIN Publication p ON p.contract_id = c.contract_id
                    WHERE c.signing_date BETWEEN @df AND @dt
                    ORDER BY c.signing_date",
                    new[] {
                        new System.Data.SqlClient.SqlParameter("@df", dateFrom.Date),
                        new System.Data.SqlClient.SqlParameter("@dt", dateTo.Date),
                    });

                WriteTable(ws1, dtContracts, r1, hide: new[] { "contract_id" });
                AutoFit(ws1);

                // ── Лист 2: Тиражи за период ──────────────────────────────────
                if (yearFrom.HasValue && yearTo.HasValue)
                {
                    var ws2 = AddSheet(pkg, "Тиражи");
                    int r2 = WriteHeader(ws2, $"Тиражи за период {yearFrom}–{yearTo}");

                    var dtPrint = DatabaseHelper.ExecuteQuery(@"
                        SELECT
                            pr.year           AS [Год],
                            p.title           AS [Издание],
                            f.format_name     AS [Формат],
                            pr.quantity       AS [Количество]
                        FROM PrintRun pr
                        JOIN Publication p ON p.publication_id = pr.publication_id
                        JOIN Format      f ON f.format_id      = pr.format_id
                        WHERE pr.year BETWEEN @yf AND @yt
                        ORDER BY pr.year, p.title",
                        new[] {
                            new System.Data.SqlClient.SqlParameter("@yf", yearFrom.Value),
                            new System.Data.SqlClient.SqlParameter("@yt", yearTo.Value),
                        });

                    WriteTable(ws2, dtPrint, r2);
                    AutoFit(ws2);
                }

                // ── Лист 3: Этапы за период ───────────────────────────────────
                var ws3 = AddSheet(pkg, "Этапы подготовки");
                int r3 = WriteHeader(ws3, $"Этапы подготовки за период {period}");

                var dtStages = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        ps.stage_name   AS [Этап],
                        ps.start_date   AS [Дата начала],
                        ps.status       AS [Статус],
                        p.title         AS [Издание]
                    FROM PreparationStage ps
                    JOIN Publication p ON p.publication_id = ps.publication_id
                    WHERE ps.start_date BETWEEN @df AND @dt
                    ORDER BY ps.start_date",
                    new[] {
                        new System.Data.SqlClient.SqlParameter("@df", dateFrom.Date),
                        new System.Data.SqlClient.SqlParameter("@dt", dateTo.Date),
                    });

                WriteTable(ws3, dtStages, r3);
                AutoFit(ws3);

                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по авторам</summary>
        public static void GenerateAuthorsReport()
        {
            string path = ChooseSavePath("Отчёт_Авторы");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Авторы");
                int row = WriteHeader(ws, "Список авторов");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        a.surname       AS [Фамилия],
                        a.name          AS [Имя],
                        a.patronymic    AS [Отчество],
                        a.email         AS [Email],
                        a.phone         AS [Телефон],
                        a.tax_id        AS [ИНН],
                        COUNT(c.contract_id) AS [Кол-во договоров]
                    FROM Author a
                    LEFT JOIN Contract c ON c.author_id = a.author_id
                    GROUP BY a.author_id, a.surname, a.name, a.patronymic,
                             a.email, a.phone, a.tax_id
                    ORDER BY a.surname, a.name");
                WriteTable(ws, dt, row);
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по договорам</summary>
        public static void GenerateContractsReport()
        {
            string path = ChooseSavePath("Отчёт_Договоры");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Договоры");
                int row = WriteHeader(ws, "Список договоров");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        a.surname+' '+a.name AS [Автор],
                        c.signing_date       AS [Дата подписания],
                        c.valid_until        AS [Действует до],
                        c.amount             AS [Сумма],
                        ISNULL(p.title, '—') AS [Издание]
                    FROM Contract c
                    JOIN Author      a ON a.author_id    = c.author_id
                    LEFT JOIN Publication p ON p.contract_id = c.contract_id
                    ORDER BY c.signing_date DESC");
                WriteTable(ws, dt, row);
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по изданиям</summary>
        public static void GeneratePublicationsReport()
        {
            string path = ChooseSavePath("Отчёт_Издания");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Издания");
                int row = WriteHeader(ws, "Список изданий");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        p.title           AS [Название],
                        p.isbn            AS [ISBN],
                        t.type_name       AS [Тип],
                        s.subject_name    AS [Тематика],
                        cl.class_level    AS [Класс],
                        a.surname+' '+a.name AS [Автор]
                    FROM Publication p
                    LEFT JOIN Type     t  ON t.type_id    = p.type_id
                    LEFT JOIN Subject  s  ON s.subject_id = p.subject_id
                    LEFT JOIN Class    cl ON cl.class_id  = p.class_id
                    LEFT JOIN Contract c  ON c.contract_id= p.contract_id
                    LEFT JOIN Author   a  ON a.author_id  = c.author_id
                    ORDER BY p.title");
                WriteTable(ws, dt, row);
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по этапам подготовки</summary>
        public static void GenerateStagesReport()
        {
            string path = ChooseSavePath("Отчёт_Этапы");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Этапы");
                int row = WriteHeader(ws, "Этапы подготовки изданий");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        p.title         AS [Издание],
                        ps.stage_name   AS [Этап],
                        ps.start_date   AS [Дата начала],
                        ps.status       AS [Статус]
                    FROM PreparationStage ps
                    JOIN Publication p ON p.publication_id = ps.publication_id
                    ORDER BY p.title, ps.start_date");
                WriteTable(ws, dt, row);

                // Цветовая индикация статусов
                ColorizeStatusColumn(ws, dt, row, "Статус");
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по экспертизам</summary>
        public static void GenerateExpertiseReport()
        {
            string path = ChooseSavePath("Отчёт_Экспертизы");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Экспертизы");
                int row = WriteHeader(ws, "Экспертизы по изданиям");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        p.title         AS [Издание],
                        ps.stage_name   AS [Этап],
                        e.date          AS [Дата экспертизы],
                        e.result        AS [Результат],
                        e.valid_until   AS [Действует до]
                    FROM Expertise e
                    JOIN PreparationStage ps ON ps.stage_id    = e.stage_id
                    JOIN Publication      p  ON p.publication_id = ps.publication_id
                    ORDER BY p.title, e.date");
                WriteTable(ws, dt, row);
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        /// <summary>Отчёт по тиражам</summary>
        public static void GeneratePrintRunsReport()
        {
            string path = ChooseSavePath("Отчёт_Тиражи");
            if (path == null) return;
            using (var pkg = CreatePackage())
            {
                var ws = AddSheet(pkg, "Тиражи");
                int row = WriteHeader(ws, "Учёт тиражей");
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT
                        p.title         AS [Издание],
                        pr.year         AS [Год],
                        f.format_name   AS [Формат],
                        pr.quantity     AS [Количество]
                    FROM PrintRun pr
                    JOIN Publication p ON p.publication_id = pr.publication_id
                    JOIN Format      f ON f.format_id      = pr.format_id
                    ORDER BY p.title, pr.year");

                row = WriteTable(ws, dt, row);

                // Итоговая строка
                WriteTotalRow(ws, dt, row, "Количество", "ИТОГО:");
                AutoFit(ws);
                pkg.SaveAs(new FileInfo(path));
            }
            OpenFile(path);
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private static ExcelPackage CreatePackage()
        {
            return new ExcelPackage();
        }

        private static ExcelWorksheet AddSheet(ExcelPackage pkg, string name)
        {
            return pkg.Workbook.Worksheets.Add(name);
        }

        /// <summary>
        /// Рисует шапку листа: название организации, название отчёта, дата формирования.
        /// Возвращает номер строки, с которой начинать таблицу.
        /// </summary>
        private static int WriteHeader(ExcelWorksheet ws, string reportTitle)
        {
            // Строка 1 — название организации
            ws.Cells[1, 1].Value = OrgName;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[1, 1].Style.Font.Color.SetColor(Color.FromArgb(44, 62, 80));

            // Строка 2 — название отчёта
            ws.Cells[2, 1].Value = reportTitle;
            ws.Cells[2, 1].Style.Font.Bold = true;
            ws.Cells[2, 1].Style.Font.Size = 12;

            // Строка 3 — дата формирования
            ws.Cells[3, 1].Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cells[3, 1].Style.Font.Italic = true;
            ws.Cells[3, 1].Style.Font.Color.SetColor(Color.FromArgb(127, 140, 141));

            // Строка 4 — пустая разделитель
            return 5; // таблица начинается с 5-й строки
        }

        /// <summary>
        /// Записывает DataTable в лист начиная с указанной строки.
        /// Возвращает номер строки ПОСЛЕ последней записи.
        /// </summary>
        private static int WriteTable(ExcelWorksheet ws, DataTable dt, int startRow,
                                       string[] hide = null)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                ws.Cells[startRow, 1].Value = "Нет данных для отображения.";
                ws.Cells[startRow, 1].Style.Font.Italic = true;
                return startRow + 1;
            }

            // Определяем видимые колонки
            var visibleCols = new System.Collections.Generic.List<int>();
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                bool hidden = false;
                if (hide != null)
                    foreach (var h in hide)
                        if (dt.Columns[c].ColumnName.Equals(h, StringComparison.OrdinalIgnoreCase))
                        { hidden = true; break; }
                if (!hidden) visibleCols.Add(c);
            }

            // Заголовок таблицы
            int col = 1;
            foreach (int ci in visibleCols)
            {
                var cell = ws.Cells[startRow, col];
                cell.Value = dt.Columns[ci].ColumnName;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(44, 62, 80));
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(189, 195, 199));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col++;
            }

            // Данные
            int dataRow = startRow + 1;
            bool alt = false;
            foreach (DataRow dr in dt.Rows)
            {
                col = 1;
                foreach (int ci in visibleCols)
                {
                    var cell = ws.Cells[dataRow, col];
                    var val  = dr[ci];

                    // Форматирование дат
                    if (val is DateTime dt2)
                    {
                        cell.Value = dt2;
                        cell.Style.Numberformat.Format = "dd.mm.yyyy";
                    }
                    else if (val == DBNull.Value)
                        cell.Value = "—";
                    else
                        cell.Value = val;

                    // Чередование строк
                    if (alt)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(245, 248, 250));
                    }

                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(189, 195, 199));
                    col++;
                }
                alt = !alt;
                dataRow++;
            }

            return dataRow;
        }

        /// <summary>
        /// Добавляет строку ИТОГО с суммой указанной колонки
        /// </summary>
        private static void WriteTotalRow(ExcelWorksheet ws, DataTable dt,
                                           int nextRow, string sumColName, string label)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            int sumColIdx = -1;
            int visibleIdx = 1;
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                if (dt.Columns[c].ColumnName.Equals(sumColName, StringComparison.OrdinalIgnoreCase))
                { sumColIdx = visibleIdx; break; }
                visibleIdx++;
            }
            if (sumColIdx < 0) return;

            // Label в первой ячейке
            var lblCell = ws.Cells[nextRow, 1];
            lblCell.Value = label;
            lblCell.Style.Font.Bold = true;
            lblCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            lblCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(174, 214, 241));

            // Сумма
            decimal total = 0;
            foreach (DataRow dr in dt.Rows)
                if (dr[sumColName] != DBNull.Value)
                    total += Convert.ToDecimal(dr[sumColName]);

            var sumCell = ws.Cells[nextRow, sumColIdx];
            sumCell.Value = total;
            sumCell.Style.Font.Bold = true;
            sumCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            sumCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(174, 214, 241));
        }

        /// <summary>
        /// Окрашивает строки по значению статуса
        /// </summary>
        private static void ColorizeStatusColumn(ExcelWorksheet ws, DataTable dt,
                                                   int dataStartRow, string statusColName)
        {
            int statusExcelCol = -1;
            int vi = 1;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName == statusColName) { statusExcelCol = vi; break; }
                vi++;
            }
            if (statusExcelCol < 0) return;

            int row = dataStartRow + 1;
            foreach (DataRow dr in dt.Rows)
            {
                string status = dr[statusColName]?.ToString();
                Color bg = Color.White;
                if (status == "Завершён" || status == "Завершено")
                    bg = Color.FromArgb(213, 245, 227); // зелёный
                else if (status == "В работе")
                    bg = Color.FromArgb(254, 249, 219); // жёлтый
                else if (status == "Запланирован")
                    bg = Color.FromArgb(214, 234, 248); // голубой

                if (bg != Color.White)
                {
                    int totalCols = dt.Columns.Count;
                    for (int c = 1; c <= totalCols; c++)
                    {
                        ws.Cells[row, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, c].Style.Fill.BackgroundColor.SetColor(bg);
                    }
                }
                row++;
            }
        }

        private static void AutoFit(ExcelWorksheet ws)
        {
            ws.Cells[ws.Dimension?.Address ?? "A1"].AutoFitColumns(10, 60);
        }

        private static string ChooseSavePath(string defaultName)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = "Сохранить отчёт";
                dlg.Filter           = "Excel файл (*.xlsx)|*.xlsx";
                dlg.FileName         = $"{defaultName}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
            }
        }

        private static void OpenFile(string path)
        {
            try { System.Diagnostics.Process.Start(path); }
            catch { UIHelper.ShowInfo($"Отчёт сохранён:\n{path}"); }
        }
    }
}
