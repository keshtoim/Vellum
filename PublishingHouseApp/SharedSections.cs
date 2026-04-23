using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public abstract class RoleFormBase : BaseMainForm
    {
        protected RoleFormBase(string roleName, string windowTitle, UserRole role)
            : base(roleName, windowTitle, role) { }

        // ══════════════════════════════════════════════════════════════════════
        // LAYOUT
        // ══════════════════════════════════════════════════════════════════════
        protected (DataGridView grid, Panel editPanel, Panel toolbar, SectionSearchBar searchBar)
            BuildSplitLayout(Panel host, string title, int editWidth = 420)
        {
            host.Controls.Clear();
            host.Padding = new Padding(0);

            var titleBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            var lbl = UIHelper.MakeSectionTitle(title);
            lbl.Location = new Point(16, 12);
            titleBar.Controls.Add(lbl);

            var searchBar = new SectionSearchBar();

            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = AppColors.FormBackground,
                Padding   = new Padding(8, 6, 8, 6)
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

            var grid     = UIHelper.MakeGrid();
            var gridWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            gridWrap.Controls.Add(grid);

            host.Controls.Add(gridWrap);
            host.Controls.Add(editPanel);
            host.Controls.Add(searchBar);
            host.Controls.Add(toolbar);
            host.Controls.Add(titleBar);

            return (grid, editPanel, toolbar, searchBar);
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDIT PANEL HELPERS
        // ══════════════════════════════════════════════════════════════════════
        protected (Label, TextBox) AddRow(Panel p, string ltext, ref int y, bool ro = false)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var tb  = new TextBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = ro ? AppColors.FormBackground : Color.White, ReadOnly = ro };
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
            y += 62;
            return (lbl, tb);
        }

        protected (Label, DateTimePicker) AddDate(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var dtp = new DateTimePicker { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), Format = DateTimePickerFormat.Short };
            p.Controls.Add(lbl);
            p.Controls.Add(dtp);
            y += 62;
            return (lbl, dtp);
        }

        /// <summary>
        /// ComboBox с кнопкой «+» для добавления новой записи прямо из поля.
        /// onAddClick — действие при нажатии «+», должно добавить запись и вернуть новый ID (-1 если отмена).
        /// После добавления список перезагружается через reloadItems.
        /// </summary>
        protected (Label, ComboBox) AddComboWithAdd(Panel p, string ltext, ref int y,
            Action<Action> onAddClick)
        {
            var lbl = new Label
            {
                Text      = ltext,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                Location  = new Point(0, y),
                AutoSize  = true
            };

            // Кнопка «+» фиксированной ширины 28px, ComboBox занимает всё остальное
            const int btnW   = 28;
            const int gap    = 4;
            int availableW   = p.Width - 36; // отступы панели
            int cbWidth      = availableW - btnW - gap;

            var cb = new ComboBox
            {
                Location      = new Point(0, y + 18),
                Width         = cbWidth,
                Font          = new Font("Segoe UI", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };

            // DropDownWidth = полная ширина панели, чтобы текст не обрезался
            cb.DropDown += (s, e) =>
                cb.DropDownWidth = Math.Max(cb.Width, availableW);

            var btnAdd = new Button
            {
                Text      = "+",
                Location  = new Point(cbWidth + gap, y + 18),
                Width     = btnW,
                Height    = cb.PreferredHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.ButtonPrimary,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.MouseEnter += (s, e) => btnAdd.BackColor = AppColors.ButtonPrimaryHover;
            btnAdd.MouseLeave += (s, e) => btnAdd.BackColor = AppColors.ButtonPrimary;
            btnAdd.Click += (s, e) => onAddClick(() =>
            {
                if (cb.Items.Count > 0)
                    cb.SelectedIndex = cb.Items.Count - 1;
            });

            // Пересчёт позиций при ресайзе панели
            p.SizeChanged += (s, e) =>
            {
                int av = p.Width - 36;
                int cw = av - btnW - gap;
                cb.Width          = cw;
                btnAdd.Left       = cw + gap;
            };

            p.Controls.Add(lbl);
            p.Controls.Add(cb);
            p.Controls.Add(btnAdd);
            y += 62;
            return (lbl, cb);
        }

        /// <summary>Обычный ComboBox без кнопки «+»</summary>
        protected (Label, ComboBox) AddCombo(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            var cb  = new ComboBox { Location = new Point(0, y + 18), Width = p.Width - 36, Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            p.Controls.Add(lbl);
            p.Controls.Add(cb);
            y += 62;
            return (lbl, cb);
        }

        protected void AddSaveCancel(Panel p, ref int y, Action onSave, Action onCancel)
        {
            var bs = UIHelper.MakePrimaryButton("Сохранить", 130, 34);
            bs.Location = new Point(0, y);
            bs.Click   += (s, e) => { try { onSave(); } catch (Exception ex) { UIHelper.ShowError($"Ошибка сохранения:\n{ex.Message}"); } };

            var bc = new Button
            {
                Text      = "Отмена",
                Width     = 100,
                Height    = 34,
                Location  = new Point(138, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.FormBackground,
                ForeColor = AppColors.TextPrimary,
                Font      = new Font("Segoe UI", 9.5f),
                Cursor    = Cursors.Hand
            };
            bc.FlatAppearance.BorderColor = AppColors.PanelBorder;
            bc.Click += (s, e) => onCancel();

            p.Controls.Add(bs);
            p.Controls.Add(bc);
        }

        // ══════════════════════════════════════════════════════════════════════
        // INLINE MINI-FORMS  — всплывают прямо в editPanel
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Показывает мини-форму добавления автора поверх editPanel.
        /// После сохранения вызывает onSaved(newId).
        /// </summary>
        protected void ShowInlineAddAuthor(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новый автор");
            int y = 40;
            var (_, tbS)  = AddMiniRow(mini, "Фамилия *", ref y);
            var (_, tbN)  = AddMiniRow(mini, "Имя *",     ref y);
            var (_, tbP)  = AddMiniRow(mini, "Отчество",  ref y);
            var (_, tbE)  = AddMiniRow(mini, "Email",     ref y);
            var (_, tbPh) = AddMiniRow(mini, "Телефон",   ref y);

            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbS.Text) || string.IsNullOrWhiteSpace(tbN.Text))
                    { UIHelper.ShowError("Заполните Фамилию и Имя."); return; }
                    int res = DatabaseHelper.SmartInsert("Author",
                        "INSERT INTO Author (surname,name,patronymic,email,phone,tax_id) VALUES (@s,@n,@p,@e,@ph,@t)",
                        new[] {
                            new SqlParameter("@s",  tbS.Text.Trim()),
                            new SqlParameter("@n",  tbN.Text.Trim()),
                            new SqlParameter("@p",  NE(tbP.Text)),
                            new SqlParameter("@e",  NE(tbE.Text)),
                            new SqlParameter("@ph", NE(tbPh.Text)),
                            new SqlParameter("@t",  (object)DBNull.Value),
                        });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Author", "author_id");
                    CloseMini(editPanel, mini);
                    onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        /// <summary>Мини-форма добавления договора</summary>
        protected void ShowInlineAddContract(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новый договор");
            int y = 40;
            var (_, dtpSign)  = AddMiniDate(mini, "Дата подписания *", ref y);
            var (_, dtpValid) = AddMiniDate(mini, "Действует до *",    ref y);
            var (_, tbAmt)    = AddMiniRow(mini,  "Сумма *",           ref y);
            var (_, cbA)      = AddMiniCombo(mini, "Автор *",          ref y);

            // Загружаем авторов
            foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT author_id,surname+' '+name AS fio FROM Author ORDER BY surname").Rows)
                cbA.Items.Add(new ComboItem(Convert.ToInt32(r["author_id"]), r["fio"].ToString()));
            dtpSign.Value = DateTime.Today;
            dtpValid.Value = DateTime.Today.AddYears(1);

            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (cbA.SelectedItem == null || string.IsNullOrWhiteSpace(tbAmt.Text))
                    { UIHelper.ShowError("Заполните все поля."); return; }
                    if (dtpValid.Value.Date <= dtpSign.Value.Date)
                    { UIHelper.ShowError("Дата окончания должна быть позже даты подписания."); return; }
                    if (!decimal.TryParse(tbAmt.Text.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal amt))
                    { UIHelper.ShowError("Сумма должна быть числом."); return; }
                    int aid = ((ComboItem)cbA.SelectedItem).Id;
                    int res = DatabaseHelper.SmartInsert("Contract",
                        "INSERT INTO Contract (signing_date,valid_until,amount,author_id) VALUES (@sd,@vu,@am,@ai)",
                        new[] {
                            new SqlParameter("@sd", dtpSign.Value.Date),
                            new SqlParameter("@vu", dtpValid.Value.Date),
                            new SqlParameter("@am", amt),
                            new SqlParameter("@ai", aid),
                        });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Contract", "contract_id");
                    CloseMini(editPanel, mini);
                    onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        /// <summary>Мини-форма добавления типа</summary>
        protected void ShowInlineAddType(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новый тип");
            int y = 40;
            var (_, tbName) = AddMiniRow(mini, "Название типа *", ref y);
            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbName.Text)) { UIHelper.ShowError("Введите название."); return; }
                    int res = DatabaseHelper.SmartInsert("Type",
                        "INSERT INTO Type (type_name) VALUES (@n)",
                        new[] { new SqlParameter("@n", tbName.Text.Trim()) });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Type", "type_id");
                    CloseMini(editPanel, mini); onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        /// <summary>Мини-форма добавления тематики</summary>
        protected void ShowInlineAddSubject(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новая тематика");
            int y = 40;
            var (_, tbName) = AddMiniRow(mini, "Название тематики *", ref y);
            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbName.Text)) { UIHelper.ShowError("Введите название."); return; }
                    int res = DatabaseHelper.SmartInsert("Subject",
                        "INSERT INTO Subject (subject_name) VALUES (@n)",
                        new[] { new SqlParameter("@n", tbName.Text.Trim()) });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Subject", "subject_id");
                    CloseMini(editPanel, mini); onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        /// <summary>Мини-форма добавления класса</summary>
        protected void ShowInlineAddClass(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новый класс");
            int y = 40;
            var (_, tbLevel) = AddMiniRow(mini, "Уровень класса *", ref y);
            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbLevel.Text)) { UIHelper.ShowError("Введите уровень."); return; }
                    int res = DatabaseHelper.SmartInsert("Class",
                        "INSERT INTO Class (class_level) VALUES (@l)",
                        new[] { new SqlParameter("@l", tbLevel.Text.Trim()) });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Class", "class_id");
                    CloseMini(editPanel, mini); onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        /// <summary>Мини-форма добавления формата</summary>
        protected void ShowInlineAddFormat(Panel editPanel, Action<int> onSaved)
        {
            var mini = BuildMiniPanel(editPanel, "Новый формат");
            int y = 40;
            var (_, tbName) = AddMiniRow(mini, "Название формата *", ref y);
            AddMiniSaveCancel(mini, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbName.Text)) { UIHelper.ShowError("Введите название."); return; }
                    int res = DatabaseHelper.SmartInsert("Format",
                        "INSERT INTO Format (format_name) VALUES (@n)",
                        new[] { new SqlParameter("@n", tbName.Text.Trim()) });
                    if (res < 0) return;
                    int newId = GetLastInsertedId("Format", "format_id");
                    CloseMini(editPanel, mini); onSaved(newId);
                },
                onCancel: () => CloseMini(editPanel, mini));
        }

        // ══════════════════════════════════════════════════════════════════════
        // MINI-PANEL BUILDER
        // ══════════════════════════════════════════════════════════════════════
        private Panel BuildMiniPanel(Panel editPanel, string title)
        {
            var mini = new Panel
            {
                BackColor = Color.FromArgb(245, 248, 252),
                Size      = new Size(editPanel.Width - 32, 0), // высота растёт динамически
                Location  = new Point(0, 0),
                Padding   = new Padding(12),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppColors.NavSelected,
                Location  = new Point(12, 8),
                AutoSize  = true
            };

            var sep = new Panel
            {
                BackColor = AppColors.NavSelected,
                Size      = new Size(mini.Width - 24, 2),
                Location  = new Point(12, 28)
            };

            mini.Controls.Add(lblTitle);
            mini.Controls.Add(sep);

            // Перекрываем содержимое editPanel — добавляем поверх
            editPanel.Controls.Add(mini);
            mini.BringToFront();
            return mini;
        }

        private void CloseMini(Panel editPanel, Panel mini)
        {
            editPanel.Controls.Remove(mini);
            mini.Dispose();
        }

        // ── Mini row helpers ──────────────────────────────────────────────────
        private (Label, TextBox) AddMiniRow(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 8.5f), ForeColor = AppColors.TextSecondary, Location = new Point(12, y), AutoSize = true };
            var tb  = new TextBox { Location = new Point(12, y + 16), Width = p.Width - 36, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(lbl); p.Controls.Add(tb);
            y += 52; p.Height = y + 50; return (lbl, tb);
        }

        private (Label, DateTimePicker) AddMiniDate(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 8.5f), ForeColor = AppColors.TextSecondary, Location = new Point(12, y), AutoSize = true };
            var dtp = new DateTimePicker { Location = new Point(12, y + 16), Width = p.Width - 36, Font = new Font("Segoe UI", 9.5f), Format = DateTimePickerFormat.Short };
            p.Controls.Add(lbl); p.Controls.Add(dtp);
            y += 52; p.Height = y + 50; return (lbl, dtp);
        }

        private (Label, ComboBox) AddMiniCombo(Panel p, string ltext, ref int y)
        {
            var lbl = new Label { Text = ltext, Font = new Font("Segoe UI", 8.5f), ForeColor = AppColors.TextSecondary, Location = new Point(12, y), AutoSize = true };
            var cb  = new ComboBox { Location = new Point(12, y + 16), Width = p.Width - 36, Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            p.Controls.Add(lbl); p.Controls.Add(cb);
            y += 52; p.Height = y + 50; return (lbl, cb);
        }

        private void AddMiniSaveCancel(Panel p, ref int y, Action onSave, Action onCancel)
        {
            var bs = UIHelper.MakePrimaryButton("Добавить", 100, 28);
            bs.Location = new Point(12, y);
            bs.Click   += (s, e) => { try { onSave(); } catch (Exception ex) { UIHelper.ShowError($"Ошибка:\n{ex.Message}"); } };

            var bc = new Button { Text = "Отмена", Width = 80, Height = 28, Location = new Point(118, y), FlatStyle = FlatStyle.Flat, BackColor = AppColors.FormBackground, ForeColor = AppColors.TextPrimary, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            bc.FlatAppearance.BorderColor = AppColors.PanelBorder;
            bc.Click += (s, e) => onCancel();

            p.Controls.Add(bs); p.Controls.Add(bc);
            p.Height = y + 40;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GRID / COMBO HELPERS
        // ══════════════════════════════════════════════════════════════════════
        protected void SetHeaders(DataGridView g, (string, string)[] map)
        {
            foreach (var (c, h) in map)
                if (g.Columns.Contains(c)) g.Columns[c].HeaderText = h;
        }

        protected void HideCols(DataGridView g, params string[] cols)
        {
            foreach (var c in cols)
                if (g.Columns.Contains(c)) g.Columns[c].Visible = false;
        }

        protected void SelectById(ComboBox cb, object id)
        {
            if (id == null || id == DBNull.Value) return;
            int t = Convert.ToInt32(id);
            foreach (var item in cb.Items)
                if (item is ComboItem ci && ci.Id == t) { cb.SelectedItem = item; return; }
        }

        protected Button MakeToolBtn(string text, int x, int w = 130)
        {
            var b = UIHelper.MakePrimaryButton(text, w);
            b.Location = new Point(x, 5);
            return b;
        }

        protected Button MakeDangerToolBtn(string text, int x, int w = 100)
        {
            var b = UIHelper.MakeDangerButton(text, w);
            b.Location = new Point(x, 5);
            return b;
        }

        protected void WireDoubleClick(DataGridView grid, Button editBtn)
        {
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) editBtn.PerformClick(); };
        }

        protected static object NE(string s) =>
            string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        protected static SqlParameter[] Append(SqlParameter[] arr, SqlParameter extra)
        {
            var r = new SqlParameter[arr.Length + 1];
            arr.CopyTo(r, 0);
            r[arr.Length] = extra;
            return r;
        }

        private static int GetLastInsertedId(string table, string pkCol)
        {
            try
            {
                var res = DatabaseHelper.ExecuteScalar($"SELECT MAX({pkCol}) FROM {table}");
                return res != null && res != DBNull.Value ? Convert.ToInt32(res) : -1;
            }
            catch { return -1; }
        }
    }
}
