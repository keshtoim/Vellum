using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public class AdminForm : BaseMainForm
    {
        private Panel pnlUsers, pnlAuthors, pnlContracts, pnlPublications;
        private Panel pnlStages, pnlExpertise, pnlPrintRuns;
        private Panel pnlReports, pnlSearch, pnlSettings;

        public AdminForm() : base("Администратор", "Администратор")
        {
            BuildNav();
            BuildAllPanels();
        }

        private void BuildNav()
        {
            pnlUsers        = AddContentPanel();
            pnlAuthors      = AddContentPanel();
            pnlContracts    = AddContentPanel();
            pnlPublications = AddContentPanel();
            pnlStages       = AddContentPanel();
            pnlExpertise    = AddContentPanel();
            pnlPrintRuns    = AddContentPanel();
            pnlReports      = AddContentPanel();
            pnlSearch       = AddContentPanel();
            pnlSettings     = AddContentPanel();

            var items = new (string, Panel)[]
            {
                ("Настройки",          pnlSettings),
                ("Поиск и сортировка", pnlSearch),
                ("Отчёты",             pnlReports),
                ("Тиражи",             pnlPrintRuns),
                ("Экспертиза",         pnlExpertise),
                ("Этапы подготовки",   pnlStages),
                ("Издания",            pnlPublications),
                ("Договоры",           pnlContracts),
                ("Авторы",             pnlAuthors),
                ("Пользователи",       pnlUsers),
            };

            Button firstBtn = null;
            foreach (var (name, panel) in items)
            {
                var btn = MakeNavButton(name, panel);
                pnlNav.Controls.Add(btn);
                firstBtn = btn;
            }
            if (firstBtn != null) ActivatePanel(firstBtn, pnlUsers);
        }

        private void BuildAllPanels()
        {
            BuildUsersPanel();
            BuildAuthorsPanel();
            BuildContractsPanel();
            BuildPublicationsPanel();
            BuildStagesPanel();
            BuildExpertisePanel();
            BuildPrintRunsPanel();
            BuildReportsPanel();
            BuildSearchPanel();
            BuildSettingsPanel();
        }

        // ══════════════════════════════════════════════════════════════════════
        // LAYOUT HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private (DataGridView grid, Panel editPanel, Panel toolbar)
            BuildSplitLayout(Panel host, string title, int editWidth = 340)
        {
            host.Controls.Clear();
            host.Padding = new Padding(0);

            var titleBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            var lbl = UIHelper.MakeSectionTitle(title);
            lbl.Location = new Point(16, 12);
            titleBar.Controls.Add(lbl);

            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = AppColors.FormBackground,
                Padding   = new Padding(16, 8, 16, 8)
            };

            var editPanel = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = editWidth,
                BackColor = Color.White,
                Padding   = new Padding(16),
                Visible   = false
            };
            editPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppColors.PanelBorder, 1), 0, 0, 0, editPanel.Height);

            var grid    = UIHelper.MakeGrid();
            var gridWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            gridWrap.Controls.Add(grid);

            host.Controls.Add(gridWrap);
            host.Controls.Add(editPanel);
            host.Controls.Add(toolbar);
            host.Controls.Add(titleBar);

            return (grid, editPanel, toolbar);
        }

        private (Label, TextBox) AddRow(Panel p, string labelText, ref int y, bool readOnly = false)
        {
            var lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var tb  = new TextBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = readOnly ? AppColors.FormBackground : Color.White, ReadOnly = readOnly };
            p.Controls.Add(lbl); p.Controls.Add(tb); y += 62;
            return (lbl, tb);
        }

        private (Label, ComboBox) AddCombo(Panel p, string labelText, ref int y)
        {
            var lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var cb  = new ComboBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            p.Controls.Add(lbl); p.Controls.Add(cb); y += 62;
            return (lbl, cb);
        }

        private (Label, DateTimePicker) AddDate(Panel p, string labelText, ref int y)
        {
            var lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var dtp = new DateTimePicker { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), Format = DateTimePickerFormat.Short };
            p.Controls.Add(lbl); p.Controls.Add(dtp); y += 62;
            return (lbl, dtp);
        }

        private void AddSaveCancel(Panel p, ref int y, Action onSave, Action onCancel)
        {
            var btnSave = UIHelper.MakePrimaryButton("Сохранить", 130, 34);
            btnSave.Location = new Point(0, y);
            btnSave.Click   += (s, e) => onSave();

            var btnCancel = new Button { Text = "Отмена", Width = 100, Height = 34, Location = new Point(138, y), FlatStyle = FlatStyle.Flat, BackColor = AppColors.FormBackground, ForeColor = AppColors.TextPrimary, Font = new Font("Segoe UI", 9.5f), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderColor = AppColors.PanelBorder;
            btnCancel.Click += (s, e) => onCancel();

            p.Controls.Add(btnSave);
            p.Controls.Add(btnCancel);
        }

        private void SetHeaders(DataGridView g, (string, string)[] map)
        {
            foreach (var (col, hdr) in map)
                if (g.Columns.Contains(col)) g.Columns[col].HeaderText = hdr;
        }

        private void Hide(DataGridView g, params string[] cols)
        {
            foreach (var c in cols)
                if (g.Columns.Contains(c)) g.Columns[c].Visible = false;
        }

        private void SelectById(ComboBox cb, object id)
        {
            if (id == null || id == DBNull.Value) return;
            int targetId = Convert.ToInt32(id);
            foreach (var item in cb.Items)
                if (item is ComboItem ci && ci.Id == targetId) { cb.SelectedItem = item; return; }
        }

        private Button MakeToolBtn(string text, int x, int w = 130)
        {
            var b = UIHelper.MakePrimaryButton(text, w);
            b.Location = new Point(x, 8);
            return b;
        }

        private Button MakeDangerToolBtn(string text, int x, int w = 100)
        {
            var b = UIHelper.MakeDangerButton(text, w);
            b.Location = new Point(x, 8);
            return b;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 1. USERS
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUsersPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlUsers, "Управление пользователями");

            var dt = new DataTable();
            dt.Columns.Add("Роль"); dt.Columns.Add("Логин"); dt.Columns.Add("Пароль");
            dt.Rows.Add("Администратор", "admin",   "admin123");
            dt.Rows.Add("Редактор",      "editor",  "editor123");
            dt.Rows.Add("Менеджер",      "manager", "manager123");
            grid.DataSource = dt;

            var note = UIHelper.MakeLabel("Учётные данные хранятся в AppUsers.cs");
            note.ForeColor = AppColors.TextSecondary; note.Location = new Point(16, 14);
            toolbar.Controls.Add(note);

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            int y = 40;
            var lblH = UIHelper.MakeSectionTitle("Смена пароля"); lblH.Location = new Point(0, 0);
            var (_, tbRole)  = AddRow(scroll, "Роль",         ref y, readOnly: true);
            var (_, tbLogin) = AddRow(scroll, "Логин",        ref y, readOnly: true);
            var (_, tbPass)  = AddRow(scroll, "Новый пароль", ref y);
            AddSaveCancel(scroll, ref y,
                onSave: () => { if (string.IsNullOrWhiteSpace(tbPass.Text)) { UIHelper.ShowError("Введите пароль."); return; } UIHelper.ShowInfo($"Пароль для '{tbLogin.Text}' изменён.\nОбновите AppUsers.cs."); editPanel.Visible = false; },
                onCancel: () => editPanel.Visible = false);
            scroll.Controls.Add(lblH);
            editPanel.Controls.Add(scroll);

            grid.SelectionChanged += (s, e) => { if (grid.SelectedRows.Count == 0) return; var r = grid.SelectedRows[0]; tbRole.Text = r.Cells["Роль"].Value?.ToString(); tbLogin.Text = r.Cells["Логин"].Value?.ToString(); tbPass.Text = ""; };

            var btnEdit = MakeToolBtn("Сменить пароль", 16, 150);
            btnEdit.Click += (s, e) => { if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите пользователя."); return; } editPanel.Visible = true; };
            toolbar.Controls.Add(btnEdit);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 2. AUTHORS
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _authorsGrid;

        private void BuildAuthorsPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlAuthors, "Управление авторами");
            _authorsGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 292, 100);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Автор"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbSurname)    = AddRow(scroll, "Фамилия *",  ref y);
            var (_, tbName)       = AddRow(scroll, "Имя *",      ref y);
            var (_, tbPatronymic) = AddRow(scroll, "Отчество",   ref y);
            var (_, tbEmail)      = AddRow(scroll, "Email",      ref y);
            var (_, tbPhone)      = AddRow(scroll, "Телефон",    ref y);
            var (_, tbTaxId)      = AddRow(scroll, "ИНН",        ref y);
            int editingId = -1;

            Action clearEdit = () => { tbSurname.Text = tbName.Text = tbPatronymic.Text = tbEmail.Text = tbPhone.Text = tbTaxId.Text = ""; editingId = -1; lblH.Text = "Новый автор"; };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbSurname.Text) || string.IsNullOrWhiteSpace(tbName.Text))
                    { UIHelper.ShowError("Заполните обязательные поля (*)"); return; }
                    var prm = new[] {
                        new SqlParameter("@s",  tbSurname.Text.Trim()),
                        new SqlParameter("@n",  tbName.Text.Trim()),
                        new SqlParameter("@p",  string.IsNullOrWhiteSpace(tbPatronymic.Text) ? (object)DBNull.Value : tbPatronymic.Text.Trim()),
                        new SqlParameter("@e",  string.IsNullOrWhiteSpace(tbEmail.Text)      ? (object)DBNull.Value : tbEmail.Text.Trim()),
                        new SqlParameter("@ph", string.IsNullOrWhiteSpace(tbPhone.Text)      ? (object)DBNull.Value : tbPhone.Text.Trim()),
                        new SqlParameter("@t",  string.IsNullOrWhiteSpace(tbTaxId.Text)      ? (object)DBNull.Value : tbTaxId.Text.Trim()),
                    };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO Author (surname,name,patronymic,email,phone,tax_id) VALUES (@s,@n,@p,@e,@ph,@t)", prm);
                    else
                    {
                        var prm2 = new SqlParameter[prm.Length + 1];
                        prm.CopyTo(prm2, 0); prm2[prm.Length] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE Author SET surname=@s,name=@n,patronymic=@p,email=@e,phone=@ph,tax_id=@t WHERE author_id=@id", prm2);
                    }
                    LoadAuthors(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH);
            editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { clearEdit(); editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите автора."); return; }
                var row = grid.SelectedRows[0];
                editingId = Convert.ToInt32(row.Cells["author_id"].Value);
                lblH.Text = "Редактировать автора";
                tbSurname.Text = row.Cells["surname"].Value?.ToString();
                tbName.Text    = row.Cells["name"].Value?.ToString();
                tbPatronymic.Text = row.Cells["patronymic"].Value?.ToString();
                tbEmail.Text   = row.Cells["email"].Value?.ToString();
                tbPhone.Text   = row.Cells["phone"].Value?.ToString();
                tbTaxId.Text   = row.Cells["tax_id"].Value?.ToString();
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите автора."); return; }
                var row = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить автора «{row.Cells["surname"].Value} {row.Cells["name"].Value}»?")) return;
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Author WHERE author_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(row.Cells["author_id"].Value)) });
                LoadAuthors(); editPanel.Visible = false;
            };
            LoadAuthors();
        }

        public void LoadAuthors()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT author_id,surname,name,patronymic,email,phone,tax_id FROM Author ORDER BY surname,name");
            _authorsGrid.DataSource = dt;
            Hide(_authorsGrid, "author_id");
            SetHeaders(_authorsGrid, new[] { ("surname","Фамилия"),("name","Имя"),("patronymic","Отчество"),("email","Email"),("phone","Телефон"),("tax_id","ИНН") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 3. CONTRACTS
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _contractsGrid;

        private void BuildContractsPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlContracts, "Управление договорами");
            _contractsGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 292, 100);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Договор"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, dtpSign)  = AddDate(scroll, "Дата подписания *", ref y);
            var (_, dtpValid) = AddDate(scroll, "Действует до *",    ref y);
            var (_, tbAmount) = AddRow(scroll,  "Сумма *",           ref y);
            var (_, cbAuthor) = AddCombo(scroll, "Автор *",          ref y);
            int editingId = -1;

            Action loadAuthors = () =>
            {
                cbAuthor.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT author_id, surname+' '+name AS fio FROM Author ORDER BY surname").Rows)
                    cbAuthor.Items.Add(new ComboItem(Convert.ToInt32(r["author_id"]), r["fio"].ToString()));
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (cbAuthor.SelectedItem == null || string.IsNullOrWhiteSpace(tbAmount.Text))
                    { UIHelper.ShowError("Заполните все обязательные поля."); return; }
                    if (!decimal.TryParse(tbAmount.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount))
                    { UIHelper.ShowError("Сумма должна быть числом."); return; }
                    int aid = ((ComboItem)cbAuthor.SelectedItem).Id;
                    var prm = new[] { new SqlParameter("@sd", dtpSign.Value.Date), new SqlParameter("@vu", dtpValid.Value.Date), new SqlParameter("@am", amount), new SqlParameter("@ai", aid) };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO Contract (signing_date,valid_until,amount,author_id) VALUES (@sd,@vu,@am,@ai)", prm);
                    else
                    {
                        var prm2 = new SqlParameter[5]; prm.CopyTo(prm2, 0); prm2[4] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE Contract SET signing_date=@sd,valid_until=@vu,amount=@am,author_id=@ai WHERE contract_id=@id", prm2);
                    }
                    LoadContracts(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadAuthors(); dtpSign.Value = DateTime.Today; dtpValid.Value = DateTime.Today.AddYears(1); tbAmount.Text = ""; cbAuthor.SelectedIndex = -1; editingId = -1; lblH.Text = "Новый договор"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите договор."); return; }
                loadAuthors();
                var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["contract_id"].Value); lblH.Text = "Редактировать договор";
                if (row.Cells["signing_date"].Value != DBNull.Value) dtpSign.Value = Convert.ToDateTime(row.Cells["signing_date"].Value);
                if (row.Cells["valid_until"].Value  != DBNull.Value) dtpValid.Value = Convert.ToDateTime(row.Cells["valid_until"].Value);
                tbAmount.Text = row.Cells["amount"].Value?.ToString();
                SelectById(cbAuthor, row.Cells["author_id"].Value);
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите договор."); return; }
                var row = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить договор №{row.Cells["contract_id"].Value}?")) return;
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Contract WHERE contract_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(row.Cells["contract_id"].Value)) });
                LoadContracts(); editPanel.Visible = false;
            };
            LoadContracts();
        }

        public void LoadContracts()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT c.contract_id, c.signing_date, c.valid_until, c.amount, a.surname+' '+a.name AS author, c.author_id FROM Contract c JOIN Author a ON a.author_id=c.author_id ORDER BY c.signing_date DESC");
            _contractsGrid.DataSource = dt;
            Hide(_contractsGrid, "contract_id", "author_id");
            SetHeaders(_contractsGrid, new[] { ("signing_date","Дата подписания"),("valid_until","Действует до"),("amount","Сумма"),("author","Автор") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 4. PUBLICATIONS
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _pubGrid;

        private void BuildPublicationsPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlPublications, "Управление изданиями");
            _pubGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 292, 100);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Издание"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbTitle)   = AddRow(scroll, "Название *",  ref y);
            var (_, tbIsbn)    = AddRow(scroll, "ISBN",        ref y);
            var (_, cbContract)= AddCombo(scroll, "Договор *", ref y);
            var (_, cbType)    = AddCombo(scroll, "Тип",       ref y);
            var (_, cbSubject) = AddCombo(scroll, "Тематика",  ref y);
            var (_, cbClass)   = AddCombo(scroll, "Класс",     ref y);
            int editingId = -1;

            Action loadCombos = () =>
            {
                cbContract.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT c.contract_id, a.surname+' '+a.name+' ('+CONVERT(varchar,c.signing_date,104)+')' AS lbl FROM Contract c JOIN Author a ON a.author_id=c.author_id ORDER BY c.signing_date DESC").Rows)
                    cbContract.Items.Add(new ComboItem(Convert.ToInt32(r["contract_id"]), r["lbl"].ToString()));
                cbType.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT type_id, type_name FROM Type ORDER BY type_name").Rows)
                    cbType.Items.Add(new ComboItem(Convert.ToInt32(r["type_id"]), r["type_name"].ToString()));
                cbSubject.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT subject_id, subject_name FROM Subject ORDER BY subject_name").Rows)
                    cbSubject.Items.Add(new ComboItem(Convert.ToInt32(r["subject_id"]), r["subject_name"].ToString()));
                cbClass.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT class_id, class_level FROM Class ORDER BY class_level").Rows)
                    cbClass.Items.Add(new ComboItem(Convert.ToInt32(r["class_id"]), r["class_level"].ToString()));
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbTitle.Text) || cbContract.SelectedItem == null)
                    { UIHelper.ShowError("Заполните обязательные поля."); return; }
                    int cid = ((ComboItem)cbContract.SelectedItem).Id;
                    object tid = cbType.SelectedItem    != null ? (object)((ComboItem)cbType.SelectedItem).Id    : DBNull.Value;
                    object sid = cbSubject.SelectedItem != null ? (object)((ComboItem)cbSubject.SelectedItem).Id : DBNull.Value;
                    object clid= cbClass.SelectedItem   != null ? (object)((ComboItem)cbClass.SelectedItem).Id   : DBNull.Value;
                    var prm = new[] { new SqlParameter("@ti", tbTitle.Text.Trim()), new SqlParameter("@is", string.IsNullOrWhiteSpace(tbIsbn.Text) ? (object)DBNull.Value : tbIsbn.Text.Trim()), new SqlParameter("@co", cid), new SqlParameter("@ty", tid), new SqlParameter("@su", sid), new SqlParameter("@cl", clid) };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO Publication (title,isbn,contract_id,type_id,subject_id,class_id) VALUES (@ti,@is,@co,@ty,@su,@cl)", prm);
                    else
                    {
                        var prm2 = new SqlParameter[7]; prm.CopyTo(prm2, 0); prm2[6] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE Publication SET title=@ti,isbn=@is,contract_id=@co,type_id=@ty,subject_id=@su,class_id=@cl WHERE publication_id=@id", prm2);
                    }
                    LoadPublications(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadCombos(); tbTitle.Text = tbIsbn.Text = ""; cbContract.SelectedIndex = cbType.SelectedIndex = cbSubject.SelectedIndex = cbClass.SelectedIndex = -1; editingId = -1; lblH.Text = "Новое издание"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите издание."); return; }
                loadCombos();
                var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["publication_id"].Value); lblH.Text = "Редактировать издание";
                tbTitle.Text = row.Cells["title"].Value?.ToString(); tbIsbn.Text = row.Cells["isbn"].Value?.ToString();
                SelectById(cbContract, row.Cells["contract_id"].Value); SelectById(cbType, row.Cells["type_id"].Value);
                SelectById(cbSubject, row.Cells["subject_id"].Value);   SelectById(cbClass, row.Cells["class_id"].Value);
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите издание."); return; }
                var row = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить издание «{row.Cells["title"].Value}»?")) return;
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Publication WHERE publication_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(row.Cells["publication_id"].Value)) });
                LoadPublications(); editPanel.Visible = false;
            };
            LoadPublications();
        }

        public void LoadPublications()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT p.publication_id, p.title, p.isbn,
                       p.contract_id, p.type_id, p.subject_id, p.class_id,
                       t.type_name, s.subject_name, cl.class_level, a.surname+' '+a.name AS author
                FROM Publication p
                LEFT JOIN Type     t  ON t.type_id      = p.type_id
                LEFT JOIN Subject  s  ON s.subject_id   = p.subject_id
                LEFT JOIN Class    cl ON cl.class_id    = p.class_id
                LEFT JOIN Contract c  ON c.contract_id  = p.contract_id
                LEFT JOIN Author   a  ON a.author_id    = c.author_id
                ORDER BY p.title");
            _pubGrid.DataSource = dt;
            Hide(_pubGrid, "publication_id", "contract_id", "type_id", "subject_id", "class_id");
            SetHeaders(_pubGrid, new[] { ("title","Название"),("isbn","ISBN"),("type_name","Тип"),("subject_name","Тематика"),("class_level","Класс"),("author","Автор") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 5. STAGES
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _stagesGrid;
        private static readonly string[] Statuses = { "Запланирован", "В работе", "Завершён" };

        private void BuildStagesPanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlStages, "Этапы подготовки");
            _stagesGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 292, 100);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Этап подготовки"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbName)   = AddRow(scroll,  "Название *",    ref y);
            var (_, dtpStart) = AddDate(scroll, "Дата начала *", ref y);
            var (_, cbStatus) = AddCombo(scroll, "Статус *",     ref y);
            var (_, cbPub)    = AddCombo(scroll, "Издание *",    ref y);
            cbStatus.Items.AddRange(Statuses);
            int editingId = -1, prevStatusIdx = -1;

            Action loadPubs = () =>
            {
                cbPub.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id, title FROM Publication ORDER BY title").Rows)
                    cbPub.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]), r["title"].ToString()));
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbName.Text) || cbStatus.SelectedIndex < 0 || cbPub.SelectedItem == null)
                    { UIHelper.ShowError("Заполните все обязательные поля."); return; }
                    int newIdx = cbStatus.SelectedIndex;
                    if (editingId != -1)
                    {
                        if (newIdx < prevStatusIdx)  { UIHelper.ShowError("Нельзя вернуть статус назад."); return; }
                        if (newIdx > prevStatusIdx + 1) { UIHelper.ShowError($"Следующий допустимый статус: «{Statuses[prevStatusIdx + 1]}»"); return; }
                    }
                    int pid = ((ComboItem)cbPub.SelectedItem).Id;
                    var prm = new[] { new SqlParameter("@n", tbName.Text.Trim()), new SqlParameter("@d", dtpStart.Value.Date), new SqlParameter("@s", Statuses[newIdx]), new SqlParameter("@p", pid) };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO PreparationStage (stage_name,start_date,status,publication_id) VALUES (@n,@d,@s,@p)", prm);
                    else
                    {
                        var prm2 = new SqlParameter[5]; prm.CopyTo(prm2, 0); prm2[4] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE PreparationStage SET stage_name=@n,start_date=@d,status=@s,publication_id=@p WHERE stage_id=@id", prm2);
                    }
                    LoadStages(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadPubs(); tbName.Text = ""; dtpStart.Value = DateTime.Today; cbStatus.SelectedIndex = 0; cbPub.SelectedIndex = -1; editingId = -1; prevStatusIdx = -1; lblH.Text = "Новый этап"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите этап."); return; }
                loadPubs();
                var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["stage_id"].Value); lblH.Text = "Редактировать этап";
                tbName.Text = row.Cells["stage_name"].Value?.ToString();
                if (row.Cells["start_date"].Value != DBNull.Value) dtpStart.Value = Convert.ToDateTime(row.Cells["start_date"].Value);
                prevStatusIdx = Array.IndexOf(Statuses, row.Cells["status"].Value?.ToString());
                cbStatus.SelectedIndex = prevStatusIdx;
                SelectById(cbPub, row.Cells["publication_id"].Value);
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите этап."); return; }
                var row = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить этап «{row.Cells["stage_name"].Value}»?")) return;
                DatabaseHelper.ExecuteNonQuery("DELETE FROM PreparationStage WHERE stage_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(row.Cells["stage_id"].Value)) });
                LoadStages(); editPanel.Visible = false;
            };
            LoadStages();
        }

        public void LoadStages()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT ps.stage_id, ps.stage_name, ps.start_date, ps.status, p.title AS publication, ps.publication_id FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id ORDER BY ps.start_date DESC");
            _stagesGrid.DataSource = dt;
            Hide(_stagesGrid, "stage_id", "publication_id");
            SetHeaders(_stagesGrid, new[] { ("stage_name","Название"),("start_date","Дата начала"),("status","Статус"),("publication","Издание") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 6. EXPERTISE
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _expGrid;

        private void BuildExpertisePanel()
        {
            var (grid, editPanel, toolbar) = BuildSplitLayout(pnlExpertise, "Экспертиза");
            _expGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 16, 120);
            var btnEdit = MakeToolBtn("Редактировать", 144, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 292, 100);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH = UIHelper.MakeSectionTitle("Экспертиза"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, dtpDate)  = AddDate(scroll, "Дата *",         ref y);
            var (_, tbResult) = AddRow(scroll,  "Результат *",    ref y);
            var (_, dtpValid) = AddDate(scroll, "Действует до *", ref y);
            var (_, cbStage)  = AddCombo(scroll, "Этап *",        ref y);
            int editingId = -1;

            Action loadStages = () =>
            {
                cbStage.Items.Clear();
                foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT ps.stage_id, ps.stage_name+' ('+p.title+')' AS lbl FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id ORDER BY ps.stage_name").Rows)
                    cbStage.Items.Add(new ComboItem(Convert.ToInt32(r["stage_id"]), r["lbl"].ToString()));
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbResult.Text) || cbStage.SelectedItem == null)
                    { UIHelper.ShowError("Заполните все обязательные поля."); return; }
                    int sid = ((ComboItem)cbStage.SelectedItem).Id;
                    var prm = new[] { new SqlParameter("@d", dtpDate.Value.Date), new SqlParameter("@r", tbResult.Text.Trim()), new SqlParameter("@v", dtpValid.Value.Date), new SqlParameter("@s", sid) };
                    if (editingId == -1)
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO Expertise (date,result,valid_until,stage_id) VALUES (@d,@r,@v,@s)", prm);
                    else
                    {
                        var prm2 = new SqlParameter[5]; prm.CopyTo(prm2, 0); prm2[4] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE Expertise SET date=@d,result=@r,valid_until=@v,stage_id=@s WHERE expertise_id=@id", prm2);
                    }
                    LoadExpertise(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadStages(); dtpDate.Value = DateTime.Today; tbResult.Text = ""; dtpValid.Value = DateTime.Today.AddYears(1); cbStage.SelectedIndex = -1; editingId = -1; lblH.Text = "Новая экспертиза"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите экспертизу."); return; }
                loadStages();
                var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["expertise_id"].Value); lblH.Text = "Редактировать экспертизу";
                if (row.Cells["date"].Value        != DBNull.Value) dtpDate.Value  = Convert.ToDateTime(row.Cells["date"].Value);
                tbResult.Text = row.Cells["result"].Value?.ToString();
                if (row.Cells["valid_until"].Value != DBNull.Value) dtpValid.Value = Convert.ToDateTime(row.Cells["valid_until"].Value);
                SelectById(cbStage, row.Cells["stage_id"].Value);
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите экспертизу."); return; }
                var row = grid.SelectedRows[0];
                if (!UIHelper.Confirm("Удалить экспертизу?")) return;
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Expertise WHERE expertise_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(row.Cells["expertise_id"].Value)) });
                LoadExpertise(); editPanel.Visible = false;
            };
            LoadExpertise();
        }

        public void LoadExpertise()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT e.expertise_id, e.date, e.result, e.valid_until, ps.stage_name AS stage, e.stage_id FROM Expertise e JOIN PreparationStage ps ON ps.stage_id=e.stage_id ORDER BY e.date DESC");
            _expGrid.DataSource = dt;
            Hide(_expGrid, "expertise_id", "stage_id");
            SetHeaders(_expGrid, new[] { ("date","Дата"),("result","Результат"),("valid_until","Действует до"),("stage","Этап") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 7. PRINT RUNS
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

            Action loadCombos = () =>
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
                        var prm2 = new SqlParameter[5]; prm.CopyTo(prm2, 0); prm2[4] = new SqlParameter("@id", editingId);
                        DatabaseHelper.ExecuteNonQuery("UPDATE PrintRun SET year=@y,quantity=@q,format_id=@f,publication_id=@p WHERE print_run_id=@id", prm2);
                    }
                    LoadPrintRuns(); editPanel.Visible = false;
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { loadCombos(); tbYear.Text = DateTime.Now.Year.ToString(); tbQty.Text = ""; cbFormat.SelectedIndex = cbPub.SelectedIndex = -1; editingId = -1; lblH.Text = "Новый тираж"; editPanel.Visible = true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите тираж."); return; }
                loadCombos();
                var row = grid.SelectedRows[0]; editingId = Convert.ToInt32(row.Cells["print_run_id"].Value); lblH.Text = "Редактировать тираж";
                tbYear.Text = row.Cells["year"].Value?.ToString(); tbQty.Text = row.Cells["quantity"].Value?.ToString();
                SelectById(cbFormat, row.Cells["format_id"].Value); SelectById(cbPub, row.Cells["publication_id"].Value);
                editPanel.Visible = true;
            };
            LoadPrintRuns();
        }

        public void LoadPrintRuns()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT pr.print_run_id, pr.year, pr.quantity, f.format_name, p.title AS publication, pr.format_id, pr.publication_id FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id ORDER BY pr.year DESC");
            _printGrid.DataSource = dt;
            Hide(_printGrid, "print_run_id", "format_id", "publication_id");
            SetHeaders(_printGrid, new[] { ("year","Год"),("quantity","Количество"),("format_name","Формат"),("publication","Издание") });
        }

        // ══════════════════════════════════════════════════════════════════════
        // 8. REPORTS — stub for Step 3
        // ══════════════════════════════════════════════════════════════════════
        private void BuildReportsPanel()
        {
            var lbl = UIHelper.MakeSectionTitle("Формирование отчётов");
            lbl.Location = new Point(16, 16);
            pnlReports.Controls.Add(lbl);
            var note = UIHelper.MakeLabel("Отчёты будут добавлены на следующем шаге.");
            note.ForeColor = AppColors.TextSecondary; note.Location = new Point(16, 60);
            pnlReports.Controls.Add(note);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 9. SEARCH
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSearchPanel()
        {
            pnlSearch.Controls.Clear();
            pnlSearch.BackColor = AppColors.FormBackground;
            pnlSearch.Padding   = new Padding(20);

            var lblTitle = UIHelper.MakeSectionTitle("Поиск и сортировка");
            lblTitle.Location = new Point(0, 0);

            var tbSearch = UIHelper.MakeTextBox(320); tbSearch.Location = new Point(0, 50); tbSearch.Height = 32;
            var placeholder = new Label { Text = "Поиск...", ForeColor = AppColors.TextSecondary, Location = new Point(4, 54), AutoSize = true, Font = new Font("Segoe UI", 10f) };
            tbSearch.Enter += (s, e) => placeholder.Visible = false;
            tbSearch.Leave += (s, e) => placeholder.Visible = string.IsNullOrEmpty(tbSearch.Text);

            var cbTable = new ComboBox { Location = new Point(330, 50), Width = 150, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTable.Items.AddRange(new[] { "Авторы","Договоры","Издания","Этапы","Тиражи" });
            cbTable.SelectedIndex = 0;

            var cbSort = new ComboBox { Location = new Point(490, 50), Width = 150, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList };

            var btnGo = UIHelper.MakePrimaryButton("Найти", 90, 32); btnGo.Location = new Point(650, 50);

            var searchGrid = UIHelper.MakeGrid();
            var wrap = new Panel { Location = new Point(0, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            wrap.Controls.Add(searchGrid);
            pnlSearch.SizeChanged += (s, e) => wrap.Size = new Size(pnlSearch.Width - 40, pnlSearch.Height - 120);

            string[][] sortCols =
            {
                new[] { "surname","name","email" },
                new[] { "c.signing_date","c.amount","a.surname" },
                new[] { "p.title","p.isbn","a.surname" },
                new[] { "ps.stage_name","ps.status","ps.start_date" },
                new[] { "pr.year","pr.quantity","p.title" }
            };
            string[][] sortLabels =
            {
                new[] { "Фамилия","Имя","Email" },
                new[] { "Дата","Сумма","Автор" },
                new[] { "Название","ISBN","Автор" },
                new[] { "Название","Статус","Дата" },
                new[] { "Год","Количество","Издание" }
            };

            cbTable.SelectedIndexChanged += (s, e) =>
            {
                cbSort.Items.Clear(); cbSort.Items.AddRange(sortLabels[cbTable.SelectedIndex]); cbSort.SelectedIndex = 0;
            };
            cbSort.Items.AddRange(sortLabels[0]); cbSort.SelectedIndex = 0;

            string[] queries =
            {
                "SELECT surname AS Фамилия, name AS Имя, patronymic AS Отчество, email AS Email, phone AS Телефон FROM Author WHERE surname LIKE @k OR name LIKE @k OR email LIKE @k ORDER BY {0}",
                "SELECT c.signing_date AS [Дата подписания], c.valid_until AS [Действует до], c.amount AS Сумма, a.surname+' '+a.name AS Автор FROM Contract c JOIN Author a ON a.author_id=c.author_id WHERE a.surname LIKE @k OR a.name LIKE @k ORDER BY {0}",
                "SELECT p.title AS Название, p.isbn AS ISBN, t.type_name AS Тип, a.surname+' '+a.name AS Автор FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id WHERE p.title LIKE @k OR p.isbn LIKE @k ORDER BY {0}",
                "SELECT ps.stage_name AS Название, ps.status AS Статус, ps.start_date AS [Дата начала], p.title AS Издание FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id WHERE ps.stage_name LIKE @k OR ps.status LIKE @k ORDER BY {0}",
                "SELECT pr.year AS Год, pr.quantity AS Количество, f.format_name AS Формат, p.title AS Издание FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id WHERE p.title LIKE @k OR f.format_name LIKE @k ORDER BY {0}"
            };

            btnGo.Click += (s, e) =>
            {
                int ti = cbTable.SelectedIndex, si = cbSort.SelectedIndex < 0 ? 0 : cbSort.SelectedIndex;
                string sql = string.Format(queries[ti], sortCols[ti][si]);
                searchGrid.DataSource = DatabaseHelper.ExecuteQuery(sql, new[] { new SqlParameter("@k", "%" + tbSearch.Text.Trim() + "%") });
            };

            tbSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnGo.PerformClick(); };
            btnGo.PerformClick();

            pnlSearch.Controls.Add(lblTitle);
            pnlSearch.Controls.Add(placeholder);
            pnlSearch.Controls.Add(tbSearch);
            pnlSearch.Controls.Add(cbTable);
            pnlSearch.Controls.Add(cbSort);
            pnlSearch.Controls.Add(btnGo);
            pnlSearch.Controls.Add(wrap);
        }

        // ══════════════════════════════════════════════════════════════════════
        // 10. SETTINGS
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSettingsPanel()
        {
            pnlSettings.Controls.Clear();
            pnlSettings.Padding = new Padding(20);

            var lblTitle = UIHelper.MakeSectionTitle("Настройки системы");
            lblTitle.Location = new Point(0, 0);
            var sep = new Panel { BackColor = AppColors.PanelBorder, Size = new Size(500, 1), Location = new Point(0, 46) };

            var lblSub = new Label { Text = "Учётные данные пользователей", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Location = new Point(0, 58), AutoSize = true };

            int y = 94;
            var users = new[] { ("Администратор","admin","admin123"), ("Редактор","editor","editor123"), ("Менеджер","manager","manager123") };
            foreach (var (role, login, pass) in users)
            {
                var card = new Panel { Size = new Size(480, 58), Location = new Point(0, y), BackColor = Color.White };
                card.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(AppColors.PanelBorder), 0, 0, card.Width - 1, card.Height - 1);
                card.Controls.Add(new Label { Text = role, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Location = new Point(12, 8), AutoSize = true });
                card.Controls.Add(new Label { Text = $"Логин: {login}    Пароль: {pass}", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(12, 30), AutoSize = true });
                pnlSettings.Controls.Add(card);
                y += 66;
            }

            var note = UIHelper.MakeLabel("Для смены паролей перейдите в раздел «Пользователи».");
            note.ForeColor = AppColors.TextSecondary; note.Location = new Point(0, y + 8);

            pnlSettings.Controls.Add(lblTitle);
            pnlSettings.Controls.Add(sep);
            pnlSettings.Controls.Add(lblSub);
            pnlSettings.Controls.Add(note);
        }
    }

    // ── ComboItem helper ──────────────────────────────────────────────────────
    public class ComboItem
    {
        public int    Id   { get; }
        public string Text { get; }
        public ComboItem(int id, string text) { Id = id; Text = text; }
        public override string ToString() => Text;
    }
}
