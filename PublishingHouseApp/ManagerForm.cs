using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public class ManagerForm : BaseMainForm
    {
        private Panel pnlPublications, pnlPrintRuns, pnlReports, pnlSearch;

        public ManagerForm() : base("Менеджер", "Менеджер")
        {
            BuildNav();
            BuildAllPanels();
        }

        private void BuildNav()
        {
            pnlPublications = AddContentPanel();
            pnlPrintRuns    = AddContentPanel();
            pnlReports      = AddContentPanel();
            pnlSearch       = AddContentPanel();

            var items = new (string, Panel)[]
            {
                ("Поиск и сортировка", pnlSearch),
                ("Отчёты",             pnlReports),
                ("Тиражи",             pnlPrintRuns),
                ("Издания",            pnlPublications),
            };

            Button firstBtn = null;
            foreach (var (name, panel) in items)
            {
                var btn = MakeNavButton(name, panel);
                pnlNav.Controls.Add(btn);
                firstBtn = btn;
            }
            if (firstBtn != null) ActivatePanel(firstBtn, pnlPublications);
        }

        private void BuildAllPanels()
        {
            BuildPublicationsPanel();
            BuildPrintRunsPanel();
            BuildReportsPanel();
            BuildSearchPanel();
        }

        // ── Layout helpers ────────────────────────────────────────────────────
        private (DataGridView grid, Panel editPanel, Panel toolbar)
            BuildSplitLayout(Panel host, string title, int editWidth = 340)
        {
            host.Controls.Clear(); host.Padding = new Padding(0);
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            var lbl = UIHelper.MakeSectionTitle(title); lbl.Location = new Point(16, 12); titleBar.Controls.Add(lbl);
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = AppColors.FormBackground, Padding = new Padding(16, 8, 16, 8) };
            var editPanel = new Panel { Dock = DockStyle.Right, Width = editWidth, BackColor = Color.White, Padding = new Padding(16), Visible = false };
            editPanel.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppColors.PanelBorder, 1), 0, 0, 0, editPanel.Height);
            var grid = UIHelper.MakeGrid();
            var gridWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            gridWrap.Controls.Add(grid);
            host.Controls.Add(gridWrap); host.Controls.Add(editPanel); host.Controls.Add(toolbar); host.Controls.Add(titleBar);
            return (grid, editPanel, toolbar);
        }

        private (Label, TextBox) AddRow(Panel p, string ltext, ref int y, bool ro = false)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var tb  = new TextBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = ro ? AppColors.FormBackground : Color.White, ReadOnly = ro };
            p.Controls.Add(lbl); p.Controls.Add(tb); y += 62; return (lbl, tb);
        }
        private (Label, ComboBox) AddCombo(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var cb  = new ComboBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            p.Controls.Add(lbl); p.Controls.Add(cb); y += 62; return (lbl, cb);
        }
        private void AddSaveCancel(Panel p, ref int y, Action onSave, Action onCancel)
        {
            var bs = UIHelper.MakePrimaryButton("Сохранить", 130, 34); bs.Location = new Point(0, y); bs.Click += (s, e) => onSave();
            var bc = new Button { Text = "Отмена", Width = 100, Height = 34, Location = new Point(138, y), FlatStyle = FlatStyle.Flat, BackColor = AppColors.FormBackground, ForeColor = AppColors.TextPrimary, Font = new Font("Segoe UI", 9.5f), Cursor = Cursors.Hand };
            bc.FlatAppearance.BorderColor = AppColors.PanelBorder; bc.Click += (s, e) => onCancel();
            p.Controls.Add(bs); p.Controls.Add(bc);
        }
        private void SetHeaders(DataGridView g, (string, string)[] map) { foreach (var (c, h) in map) if (g.Columns.Contains(c)) g.Columns[c].HeaderText = h; }
        private void HideCols(DataGridView g, params string[] cols) { foreach (var c in cols) if (g.Columns.Contains(c)) g.Columns[c].Visible = false; }
        private void SelectById(ComboBox cb, object id) { if (id == null || id == DBNull.Value) return; int t = Convert.ToInt32(id); foreach (var item in cb.Items) if (item is ComboItem ci && ci.Id == t) { cb.SelectedItem = item; return; } }
        private Button MakeToolBtn(string text, int x, int w = 130) { var b = UIHelper.MakePrimaryButton(text, w); b.Location = new Point(x, 8); return b; }

        // ══════════════════════════════════════════════════════════════════════
        // PUBLICATIONS (read-only view for manager)
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _pubGrid;

        private void BuildPublicationsPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlPublications, "Издания");
            _pubGrid = grid;

            var note = UIHelper.MakeLabel("Просмотр изданий");
            note.ForeColor = AppColors.TextSecondary; note.Location = new Point(16, 14);
            toolbar.Controls.Add(note);

            LoadPublications();
        }

        private void LoadPublications()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT p.title AS Название, p.isbn AS ISBN, t.type_name AS Тип, s.subject_name AS Тематика, cl.class_level AS Класс, a.surname+' '+a.name AS Автор FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Subject s ON s.subject_id=p.subject_id LEFT JOIN Class cl ON cl.class_id=p.class_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id ORDER BY p.title");
            _pubGrid.DataSource = dt;
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRINT RUNS (add + edit only, no delete)
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _printGrid;

        private void BuildPrintRunsPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlPrintRuns, "Учёт тиражей");
            _printGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Тираж"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbYear)   = AddRow(scroll,  "Год *",        ref y);
            var (_, tbQty)    = AddRow(scroll,  "Количество *", ref y);
            var (_, cbFormat) = AddCombo(scroll, "Формат *",    ref y);
            var (_, cbPub)    = AddCombo(scroll, "Издание *",   ref y);
            int editingId = -1;

            Action loadC = () =>
            {
                cbFormat.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT format_id, format_name FROM Format ORDER BY format_name").Rows)
                    cbFormat.Items.Add(new ComboItem(Convert.ToInt32(r["format_id"]), r["format_name"].ToString()));
                cbPub.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id, title FROM Publication ORDER BY title").Rows)
                    cbPub.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]), r["title"].ToString()));
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (!int.TryParse(tbYear.Text, out int yr) || yr < 1900 || yr > 2100) { UIHelper.ShowError("Введите корректный год."); return; }
                    if (!int.TryParse(tbQty.Text, out int qty) || qty <= 0) { UIHelper.ShowError("Количество должно быть > 0."); return; }
                    if (cbFormat.SelectedItem == null || cbPub.SelectedItem == null) { UIHelper.ShowError("Выберите формат и издание."); return; }
                    int fid = ((ComboItem)cbFormat.SelectedItem).Id, pid = ((ComboItem)cbPub.SelectedItem).Id;
                    var prm = new[] { new SqlParameter("@y", yr), new SqlParameter("@q", qty), new SqlParameter("@f", fid), new SqlParameter("@p", pid) };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO PrintRun (year,quantity,format_id,publication_id) VALUES (@y,@q,@f,@p)", prm);
                    else
                    {
                        var p2 = new SqlParameter[5]; prm.CopyTo(p2, 0); p2[4] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE PrintRun SET year=@y,quantity=@q,format_id=@f,publication_id=@p WHERE print_run_id=@id", p2);
                    }
                    LoadPrintRuns(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadC(); tbYear.Text = DateTime.Now.Year.ToString(); tbQty.Text = ""; cbFormat.SelectedIndex = cbPub.SelectedIndex = -1; editingId = -1; lblH.Text = "Новый тираж"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите тираж."); return; }
                loadC(); var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["print_run_id"].Value); lblH.Text = "Редактировать тираж";
                tbYear.Text = row.Cells["year"].Value?.ToString(); tbQty.Text = row.Cells["quantity"].Value?.ToString();
                SelectById(cbFormat, row.Cells["format_id"].Value); SelectById(cbPub, row.Cells["publication_id"].Value);
                editPanel.Visible = true;
            };
            LoadPrintRuns();
        }

        private void LoadPrintRuns()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT pr.print_run_id, pr.year, pr.quantity, f.format_name, p.title AS publication, pr.format_id, pr.publication_id FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id ORDER BY pr.year DESC");
            _printGrid.DataSource = dt; HideCols(_printGrid, "print_run_id","format_id","publication_id");
            SetHeaders(_printGrid, new[] { ("year","Год"),("quantity","Количество"),("format_name","Формат"),("publication","Издание") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // REPORTS — stub for Step 3
        // ══════════════════════════════════════════════════════════════════════
        private void BuildReportsPanel()
        {
            var lbl = UIHelper.MakeSectionTitle("Формирование отчётов"); lbl.Location = new Point(16, 16);
            pnlReports.Controls.Add(lbl);
            var note = UIHelper.MakeLabel("Отчёты будут добавлены на следующем шаге."); note.ForeColor = AppColors.TextSecondary; note.Location = new Point(16, 60);
            pnlReports.Controls.Add(note);
        }

        // ══════════════════════════════════════════════════════════════════════
        // SEARCH
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSearchPanel()
        {
            pnlSearch.Controls.Clear(); pnlSearch.BackColor = AppColors.FormBackground; pnlSearch.Padding = new Padding(20);
            var lblTitle = UIHelper.MakeSectionTitle("Поиск и сортировка"); lblTitle.Location = new Point(0, 0);
            var tbSearch = UIHelper.MakeTextBox(320); tbSearch.Location = new Point(0, 50); tbSearch.Height = 32;
            var cbTable = new ComboBox { Location = new Point(330, 50), Width = 150, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTable.Items.AddRange(new[] { "Издания","Тиражи" }); cbTable.SelectedIndex = 0;
            var cbSort = new ComboBox { Location = new Point(490, 50), Width = 150, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList };
            var btnGo = UIHelper.MakePrimaryButton("Найти", 90, 32); btnGo.Location = new Point(650, 50);
            var searchGrid = UIHelper.MakeGrid();
            var wrap = new Panel { Location = new Point(0, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            wrap.Controls.Add(searchGrid);
            pnlSearch.SizeChanged += (s, e) => wrap.Size = new Size(pnlSearch.Width - 40, pnlSearch.Height - 120);

            string[][] sortCols   = { new[] { "p.title","p.isbn","a.surname" }, new[] { "pr.year","pr.quantity","p.title" } };
            string[][] sortLabels = { new[] { "Название","ISBN","Автор" },      new[] { "Год","Количество","Издание" } };
            cbTable.SelectedIndexChanged += (s, e) => { cbSort.Items.Clear(); cbSort.Items.AddRange(sortLabels[cbTable.SelectedIndex]); cbSort.SelectedIndex = 0; };
            cbSort.Items.AddRange(sortLabels[0]); cbSort.SelectedIndex = 0;

            string[] queries = {
                "SELECT p.title AS Название, p.isbn AS ISBN, t.type_name AS Тип, a.surname+' '+a.name AS Автор FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id WHERE p.title LIKE @k OR p.isbn LIKE @k ORDER BY {0}",
                "SELECT pr.year AS Год, pr.quantity AS Количество, f.format_name AS Формат, p.title AS Издание FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id WHERE p.title LIKE @k OR f.format_name LIKE @k ORDER BY {0}"
            };

            btnGo.Click += (s, e) => { int ti = cbTable.SelectedIndex, si = cbSort.SelectedIndex < 0 ? 0 : cbSort.SelectedIndex; searchGrid.DataSource = DatabaseHelper.ExecuteQuery(string.Format(queries[ti], sortCols[ti][si]), new[] { new SqlParameter("@k", "%" + tbSearch.Text.Trim() + "%") }); };
            tbSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnGo.PerformClick(); };
            btnGo.PerformClick();

            pnlSearch.Controls.Add(lblTitle); pnlSearch.Controls.Add(tbSearch); pnlSearch.Controls.Add(cbTable); pnlSearch.Controls.Add(cbSort); pnlSearch.Controls.Add(btnGo); pnlSearch.Controls.Add(wrap);
        }
    }
}
