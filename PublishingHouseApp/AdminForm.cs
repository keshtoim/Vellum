using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    public class AdminForm : RoleFormBase
    {
        private Panel pnlAuthors, pnlContracts, pnlPublications;
        private Panel pnlStages, pnlExpertise, pnlPrintRuns;
        private Panel pnlReports, pnlSettings;

        public AdminForm() : base("Администратор", "Администратор", UserRole.Admin)
        {
            BuildNav();
            BuildAllPanels();
        }

        // ══════════════════════════════════════════════════════════════════════
        // NAV — порядок добавления = порядок отображения (FlowLayout)
        // ══════════════════════════════════════════════════════════════════════
        private void BuildNav()
        {
            AddDashboardNav();   // 1. Главная

            pnlAuthors      = AddContentPanel();
            pnlContracts    = AddContentPanel();
            pnlPublications = AddContentPanel();
            pnlStages       = AddContentPanel();
            pnlExpertise    = AddContentPanel();
            pnlPrintRuns    = AddContentPanel();
            pnlReports      = AddContentPanel();
            pnlSettings     = AddContentPanel();

            // 2–9 — в нужном порядке сверху вниз
            pnlNav.Controls.Add(MakeNavButton("Авторы",           pnlAuthors));
            pnlNav.Controls.Add(MakeNavButton("Договоры",         pnlContracts));
            pnlNav.Controls.Add(MakeNavButton("Издания",          pnlPublications));
            pnlNav.Controls.Add(MakeNavButton("Этапы подготовки", pnlStages));
            pnlNav.Controls.Add(MakeNavButton("Экспертиза",       pnlExpertise));
            pnlNav.Controls.Add(MakeNavButton("Тиражи",           pnlPrintRuns));
            pnlNav.Controls.Add(MakeNavButton("Отчёты",           pnlReports));
            pnlNav.Controls.Add(MakeNavButton("Настройки",        pnlSettings));
        }

        private void BuildAllPanels()
        {
            try { BuildAuthorsPanel(); }     catch (Exception ex) { ShowPanelError(pnlAuthors,      "Авторы", ex); }
            try { BuildContractsPanel(); }   catch (Exception ex) { ShowPanelError(pnlContracts,    "Договоры", ex); }
            try { BuildPublicationsPanel(); }catch (Exception ex) { ShowPanelError(pnlPublications, "Издания", ex); }
            try { BuildStagesPanel(); }      catch (Exception ex) { ShowPanelError(pnlStages,       "Этапы", ex); }
            try { BuildExpertisePanel(); }   catch (Exception ex) { ShowPanelError(pnlExpertise,    "Экспертиза", ex); }
            try { BuildPrintRunsPanel(); }   catch (Exception ex) { ShowPanelError(pnlPrintRuns,    "Тиражи", ex); }
            BuildReportsPanel();
            BuildSettingsPanel();
        }

        private void ShowPanelError(Panel panel, string name, Exception ex)
        {
            var lbl = UIHelper.MakeLabel($"Ошибка загрузки раздела «{name}»:\n{ex.Message}");
            lbl.ForeColor = AppColors.ButtonDanger;
            lbl.Location  = new Point(16, 16);
            panel.Controls.Add(lbl);
        }

        // ══════════════════════════════════════════════════════════════════════
        // AUTHORS
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView _authorsGrid;

        private void BuildAuthorsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlAuthors, "Управление авторами");
            _authorsGrid = grid;

            var btnAdd  = MakeToolBtn("+ Добавить", 8, 115);
            var btnEdit = MakeToolBtn("Редактировать", 131, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 279);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH   = UIHelper.MakeSectionTitle("Автор"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbS)  = AddRow(scroll, "Фамилия *",  ref y);
            var (_, tbN)  = AddRow(scroll, "Имя *",      ref y);
            var (_, tbP)  = AddRow(scroll, "Отчество",   ref y);
            var (_, tbE)  = AddRow(scroll, "Email",      ref y);
            var (_, tbPh) = AddRow(scroll, "Телефон",    ref y);
            var (_, tbT)  = AddRow(scroll, "ИНН",        ref y);
            int eid = -1;

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbS.Text) || string.IsNullOrWhiteSpace(tbN.Text))
                    { UIHelper.ShowError("Заполните обязательные поля (*)"); return; }
                    var prm = new[] {
                        new SqlParameter("@s",  tbS.Text.Trim()),
                        new SqlParameter("@n",  tbN.Text.Trim()),
                        new SqlParameter("@p",  NE(tbP.Text)),
                        new SqlParameter("@e",  NE(tbE.Text)),
                        new SqlParameter("@ph", NE(tbPh.Text)),
                        new SqlParameter("@t",  NE(tbT.Text)),
                    };
                    int res = eid == -1
                        ? DatabaseHelper.SmartInsert("Author", "INSERT INTO Author (surname,name,patronymic,email,phone,tax_id) VALUES (@s,@n,@p,@e,@ph,@t)", prm)
                        : DatabaseHelper.ExecuteNonQuery("UPDATE Author SET surname=@s,name=@n,patronymic=@p,email=@e,phone=@ph,tax_id=@t WHERE author_id=@id", Append(prm, new SqlParameter("@id", eid)));
                    if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { tbS.Text=tbN.Text=tbP.Text=tbE.Text=tbPh.Text=tbT.Text=""; eid=-1; lblH.Text="Новый автор"; editPanel.Visible=true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите автора."); return; }
                var r = grid.SelectedRows[0]; eid = Convert.ToInt32(r.Cells["author_id"].Value); lblH.Text = "Редактировать автора";
                tbS.Text=r.Cells["surname"].Value?.ToString(); tbN.Text=r.Cells["name"].Value?.ToString();
                tbP.Text=r.Cells["patronymic"].Value?.ToString(); tbE.Text=r.Cells["email"].Value?.ToString();
                tbPh.Text=r.Cells["phone"].Value?.ToString(); tbT.Text=r.Cells["tax_id"].Value?.ToString();
                editPanel.Visible = true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите автора."); return; }
                var r = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить автора «{r.Cells["surname"].Value} {r.Cells["name"].Value}»?")) return;
                int res = DatabaseHelper.ExecuteNonQuery("DELETE FROM Author WHERE author_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(r.Cells["author_id"].Value)) });
                if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
            };
            WireDoubleClick(grid, btnEdit);

            sb.Init(grid, (kw, sc) =>
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    $"SELECT author_id,surname,name,patronymic,email,phone,tax_id FROM Author WHERE surname LIKE @k OR name LIKE @k OR email LIKE @k ORDER BY {sc}",
                    new[] { new SqlParameter("@k", $"%{kw}%") });
                HideCols(grid, "author_id");
                SetHeaders(grid, new[] { ("surname","Фамилия"),("name","Имя"),("patronymic","Отчество"),("email","Email"),("phone","Телефон"),("tax_id","ИНН") });
                return dt;
            }, new[] { "Фамилия","Имя","Email" }, new[] { "surname","name","email" });
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONTRACTS
        // ══════════════════════════════════════════════════════════════════════
        private void BuildContractsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlContracts, "Управление договорами");

            var btnAdd  = MakeToolBtn("+ Добавить", 8, 115);
            var btnEdit = MakeToolBtn("Редактировать", 131, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 279);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH   = UIHelper.MakeSectionTitle("Договор"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, dtpSign)  = AddDate(scroll, "Дата подписания *", ref y);
            var (_, dtpValid) = AddDate(scroll, "Действует до *",    ref y);
            var (_, tbAmt)    = AddRow(scroll,  "Сумма *",           ref y);

            // Автор — с кнопкой «+»
            ComboBox cbAuthor = null;
            var (_, _cbAuthor) = AddComboWithAdd(scroll, "Автор *", ref y,
                onAddClick: afterSave => ShowInlineAddAuthor(editPanel, newId =>
                {
                    LoadAuthorsToCombo(cbAuthor);
                    SelectById(cbAuthor, newId);
                    afterSave();
                }));
            cbAuthor = _cbAuthor;

            int eid = -1;

            Action reloadA = () => LoadAuthorsToCombo(cbAuthor);

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (cbAuthor.SelectedItem == null || string.IsNullOrWhiteSpace(tbAmt.Text)) { UIHelper.ShowError("Заполните все обязательные поля."); return; }
                    if (dtpValid.Value.Date <= dtpSign.Value.Date) { UIHelper.ShowError("Дата окончания должна быть позже даты подписания."); return; }
                    if (!decimal.TryParse(tbAmt.Text.Replace(',','.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amt)) { UIHelper.ShowError("Сумма должна быть числом."); return; }
                    int aid = ((ComboItem)cbAuthor.SelectedItem).Id;
                    var prm = new[] { new SqlParameter("@sd", dtpSign.Value.Date), new SqlParameter("@vu", dtpValid.Value.Date), new SqlParameter("@am", amt), new SqlParameter("@ai", aid) };
                    int res = eid == -1
                        ? DatabaseHelper.SmartInsert("Contract", "INSERT INTO Contract (signing_date,valid_until,amount,author_id) VALUES (@sd,@vu,@am,@ai)", prm)
                        : DatabaseHelper.ExecuteNonQuery("UPDATE Contract SET signing_date=@sd,valid_until=@vu,amount=@am,author_id=@ai WHERE contract_id=@id", Append(prm, new SqlParameter("@id", eid)));
                    if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { reloadA(); dtpSign.Value=DateTime.Today; dtpValid.Value=DateTime.Today.AddYears(1); tbAmt.Text=""; cbAuthor.SelectedIndex=-1; eid=-1; lblH.Text="Новый договор"; editPanel.Visible=true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите договор."); return; }
                reloadA(); var r=grid.SelectedRows[0]; eid=Convert.ToInt32(r.Cells["contract_id"].Value); lblH.Text="Редактировать договор";
                if (r.Cells["signing_date"].Value != DBNull.Value) dtpSign.Value  = Convert.ToDateTime(r.Cells["signing_date"].Value);
                if (r.Cells["valid_until"].Value  != DBNull.Value) dtpValid.Value = Convert.ToDateTime(r.Cells["valid_until"].Value);
                tbAmt.Text = r.Cells["amount"].Value?.ToString(); SelectById(cbAuthor, r.Cells["author_id"].Value); editPanel.Visible=true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите договор."); return; }
                var r = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить договор №{r.Cells["contract_id"].Value}?")) return;
                int res = DatabaseHelper.ExecuteNonQuery("DELETE FROM Contract WHERE contract_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(r.Cells["contract_id"].Value)) });
                if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
            };
            WireDoubleClick(grid, btnEdit);

            sb.Init(grid, (kw, sc) =>
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    $"SELECT c.contract_id,c.signing_date,c.valid_until,c.amount,a.surname+' '+a.name AS author,c.author_id FROM Contract c JOIN Author a ON a.author_id=c.author_id WHERE a.surname LIKE @k OR a.name LIKE @k ORDER BY {sc}",
                    new[] { new SqlParameter("@k", $"%{kw}%") });
                HideCols(grid, "contract_id","author_id");
                SetHeaders(grid, new[] { ("signing_date","Дата подписания"),("valid_until","Действует до"),("amount","Сумма"),("author","Автор") });
                return dt;
            }, new[] { "Дата","Сумма","Автор" }, new[] { "c.signing_date","c.amount","a.surname" });
        }

        // ══════════════════════════════════════════════════════════════════════
        // PUBLICATIONS — все ComboBox с кнопками «+»
        // ══════════════════════════════════════════════════════════════════════
        private void BuildPublicationsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlPublications, "Управление изданиями");

            var btnAdd  = MakeToolBtn("+ Добавить", 8, 115);
            var btnEdit = MakeToolBtn("Редактировать", 131, 140);
            var btnDel  = MakeDangerToolBtn("Удалить", 279);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

            var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var lblH   = UIHelper.MakeSectionTitle("Издание"); lblH.Location = new Point(0, 0);
            int y = 40;
            var (_, tbTitle) = AddRow(scroll, "Название *", ref y);
            var (_, tbIsbn)  = AddRow(scroll, "ISBN",       ref y);

            // Договор — с кнопкой «+»
            ComboBox cbContract = null;
            var (_, _cbContract) = AddComboWithAdd(scroll, "Договор *", ref y,
                onAddClick: afterSave => ShowInlineAddContract(editPanel, newId =>
                {
                    LoadContractsToCombo(cbContract); SelectById(cbContract, newId); afterSave();
                }));
            cbContract = _cbContract;

            // Тип — с кнопкой «+»
            ComboBox cbType = null;
            var (_, _cbType) = AddComboWithAdd(scroll, "Тип", ref y,
                onAddClick: afterSave => ShowInlineAddType(editPanel, newId =>
                {
                    LoadTypesToCombo(cbType); SelectById(cbType, newId); afterSave();
                }));
            cbType = _cbType;

            // Тематика — с кнопкой «+»
            ComboBox cbSubject = null;
            var (_, _cbSubject) = AddComboWithAdd(scroll, "Тематика", ref y,
                onAddClick: afterSave => ShowInlineAddSubject(editPanel, newId =>
                {
                    LoadSubjectsToCombo(cbSubject); SelectById(cbSubject, newId); afterSave();
                }));
            cbSubject = _cbSubject;

            // Класс — с кнопкой «+»
            ComboBox cbClass = null;
            var (_, _cbClass) = AddComboWithAdd(scroll, "Класс", ref y,
                onAddClick: afterSave => ShowInlineAddClass(editPanel, newId =>
                {
                    LoadClassesToCombo(cbClass); SelectById(cbClass, newId); afterSave();
                }));
            cbClass = _cbClass;

            int eid = -1;

            Action reloadAll = () =>
            {
                LoadContractsToCombo(cbContract); LoadTypesToCombo(cbType);
                LoadSubjectsToCombo(cbSubject);   LoadClassesToCombo(cbClass);
            };

            AddSaveCancel(scroll, ref y,
                onSave: () =>
                {
                    if (string.IsNullOrWhiteSpace(tbTitle.Text) || cbContract.SelectedItem == null) { UIHelper.ShowError("Заполните обязательные поля."); return; }
                    int cid = ((ComboItem)cbContract.SelectedItem).Id;
                    object tid  = cbType.SelectedItem    != null ? (object)((ComboItem)cbType.SelectedItem).Id    : DBNull.Value;
                    object sid  = cbSubject.SelectedItem != null ? (object)((ComboItem)cbSubject.SelectedItem).Id : DBNull.Value;
                    object clid = cbClass.SelectedItem   != null ? (object)((ComboItem)cbClass.SelectedItem).Id   : DBNull.Value;
                    var prm = new[] {
                        new SqlParameter("@ti", tbTitle.Text.Trim()),
                        new SqlParameter("@is", NE(tbIsbn.Text)),
                        new SqlParameter("@co", cid),
                        new SqlParameter("@ty", tid),
                        new SqlParameter("@su", sid),
                        new SqlParameter("@cl", clid)
                    };
                    int res = eid == -1
                        ? DatabaseHelper.SmartInsert("Publication", "INSERT INTO Publication (title,isbn,contract_id,type_id,subject_id,class_id) VALUES (@ti,@is,@co,@ty,@su,@cl)", prm)
                        : DatabaseHelper.ExecuteNonQuery("UPDATE Publication SET title=@ti,isbn=@is,contract_id=@co,type_id=@ty,subject_id=@su,class_id=@cl WHERE publication_id=@id", Append(prm, new SqlParameter("@id", eid)));
                    if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
                },
                onCancel: () => editPanel.Visible = false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            btnAdd.Click  += (s, e) => { reloadAll(); tbTitle.Text=tbIsbn.Text=""; cbContract.SelectedIndex=cbType.SelectedIndex=cbSubject.SelectedIndex=cbClass.SelectedIndex=-1; eid=-1; lblH.Text="Новое издание"; editPanel.Visible=true; };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите издание."); return; }
                reloadAll(); var r=grid.SelectedRows[0]; eid=Convert.ToInt32(r.Cells["publication_id"].Value); lblH.Text="Редактировать издание";
                tbTitle.Text=r.Cells["title"].Value?.ToString(); tbIsbn.Text=r.Cells["isbn"].Value?.ToString();
                SelectById(cbContract, r.Cells["contract_id"].Value); SelectById(cbType, r.Cells["type_id"].Value);
                SelectById(cbSubject,  r.Cells["subject_id"].Value);  SelectById(cbClass, r.Cells["class_id"].Value);
                editPanel.Visible=true;
            };
            btnDel.Click  += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("Выберите издание."); return; }
                var r = grid.SelectedRows[0];
                if (!UIHelper.Confirm($"Удалить «{r.Cells["title"].Value}»?")) return;
                int res = DatabaseHelper.ExecuteNonQuery("DELETE FROM Publication WHERE publication_id=@id", new[] { new SqlParameter("@id", Convert.ToInt32(r.Cells["publication_id"].Value)) });
                if (res >= 0) { sb.Refresh(); editPanel.Visible = false; }
            };
            WireDoubleClick(grid, btnEdit);

            sb.Init(grid, (kw, sc) =>
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    $"SELECT p.publication_id,p.title,p.isbn,p.contract_id,p.type_id,p.subject_id,p.class_id,t.type_name,s.subject_name,cl.class_level,a.surname+' '+a.name AS author " +
                    $"FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Subject s ON s.subject_id=p.subject_id " +
                    $"LEFT JOIN Class cl ON cl.class_id=p.class_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id " +
                    $"WHERE p.title LIKE @k OR p.isbn LIKE @k OR a.surname LIKE @k ORDER BY {sc}",
                    new[] { new SqlParameter("@k", $"%{kw}%") });
                HideCols(grid, "publication_id","contract_id","type_id","subject_id","class_id");
                SetHeaders(grid, new[] { ("title","Название"),("isbn","ISBN"),("type_name","Тип"),("subject_name","Тематика"),("class_level","Класс"),("author","Автор") });
                return dt;
            }, new[] { "Название","ISBN","Автор" }, new[] { "p.title","p.isbn","a.surname" });
        }

        // ══════════════════════════════════════════════════════════════════════
        // STAGES
        // ══════════════════════════════════════════════════════════════════════
        private static readonly string[] Statuses = { "Запланирован", "В работе", "Завершён" };

        private void BuildStagesPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlStages, "Этапы подготовки");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Этап подготовки"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbName)  = AddRow(scroll, "Название *",    ref y);
            var (_,dtpStart)= AddDate(scroll, "Дата начала *", ref y);
            var (_,cbStatus)= AddCombo(scroll, "Статус *",    ref y);
            var (_,cbPub)   = AddComboWithAdd(scroll, "Издание *", ref y,
                onAddClick: afterSave => UIHelper.ShowInfo("Для добавления издания перейдите в раздел «Издания»."));
            cbStatus.Items.AddRange(Statuses);
            int eid=-1, prevIdx=-1;

            AddSaveCancel(scroll, ref y,
                onSave: ()=>
                {
                    if(string.IsNullOrWhiteSpace(tbName.Text)||cbStatus.SelectedIndex<0||cbPub.SelectedItem==null){UIHelper.ShowError("Заполните все обязательные поля.");return;}
                    int ni=cbStatus.SelectedIndex;
                    if(eid!=-1){if(ni<prevIdx){UIHelper.ShowError("Нельзя вернуть статус назад.");return;}if(ni>prevIdx+1){UIHelper.ShowError($"Следующий допустимый: «{Statuses[prevIdx+1]}»");return;}}
                    int pid=((ComboItem)cbPub.SelectedItem).Id;
                    var prm=new[]{new SqlParameter("@n",tbName.Text.Trim()),new SqlParameter("@d",dtpStart.Value.Date),new SqlParameter("@s",Statuses[ni]),new SqlParameter("@p",pid)};
                    int res=eid==-1?DatabaseHelper.SmartInsert("PreparationStage","INSERT INTO PreparationStage (stage_name,start_date,status,publication_id) VALUES (@n,@d,@s,@p)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE PreparationStage SET stage_name=@n,start_date=@d,status=@s,publication_id=@p WHERE stage_id=@id",Append(prm,new SqlParameter("@id",eid)));
                    if(res>=0){sb.Refresh();editPanel.Visible=false;}
                },
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);

            void LoadPubs(){cbPub.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id,title FROM Publication ORDER BY title").Rows)cbPub.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]),r["title"].ToString()));}

            btnAdd.Click+=(s,e)=>{LoadPubs();tbName.Text="";dtpStart.Value=DateTime.Today;cbStatus.SelectedIndex=0;cbPub.SelectedIndex=-1;eid=-1;prevIdx=-1;lblH.Text="Новый этап";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите этап.");return;}LoadPubs();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["stage_id"].Value);lblH.Text="Редактировать этап";tbName.Text=r.Cells["stage_name"].Value?.ToString();if(r.Cells["start_date"].Value!=DBNull.Value)dtpStart.Value=Convert.ToDateTime(r.Cells["start_date"].Value);prevIdx=Array.IndexOf(Statuses,r.Cells["status"].Value?.ToString());cbStatus.SelectedIndex=prevIdx;SelectById(cbPub,r.Cells["publication_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите этап.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm($"Удалить этап «{r.Cells["stage_name"].Value}»?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM PreparationStage WHERE stage_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["stage_id"].Value))});if(res>=0){sb.Refresh();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);

            sb.Init(grid,(kw,sc)=>{var dt=DatabaseHelper.ExecuteQuery($"SELECT ps.stage_id,ps.stage_name,ps.start_date,ps.status,p.title AS publication,ps.publication_id FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id WHERE ps.stage_name LIKE @k OR ps.status LIKE @k OR p.title LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")});HideCols(grid,"stage_id","publication_id");SetHeaders(grid,new[]{("stage_name","Название"),("start_date","Дата начала"),("status","Статус"),("publication","Издание")});return dt;},new[]{"Название","Статус","Дата"},new[]{"ps.stage_name","ps.status","ps.start_date"});
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPERTISE
        // ══════════════════════════════════════════════════════════════════════
        private void BuildExpertisePanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlExpertise, "Экспертиза");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Экспертиза"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,dtpDate) =AddDate(scroll,"Дата *",ref y);
            var (_,tbResult)=AddRow(scroll,"Результат *",ref y);
            var (_,dtpValid)=AddDate(scroll,"Действует до *",ref y);
            var (_,cbStage) =AddCombo(scroll,"Этап *",ref y);
            int eid=-1;

            void LoadStages(){cbStage.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT ps.stage_id,ps.stage_name+' ('+p.title+')' AS lbl FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id ORDER BY ps.stage_name").Rows)cbStage.Items.Add(new ComboItem(Convert.ToInt32(r["stage_id"]),r["lbl"].ToString()));}

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(string.IsNullOrWhiteSpace(tbResult.Text)||cbStage.SelectedItem==null){UIHelper.ShowError("Заполните все поля.");return;}if(dtpValid.Value.Date<=dtpDate.Value.Date){UIHelper.ShowError("Дата окончания должна быть позже даты экспертизы.");return;}int sid=((ComboItem)cbStage.SelectedItem).Id;var prm=new[]{new SqlParameter("@d",dtpDate.Value.Date),new SqlParameter("@r",tbResult.Text.Trim()),new SqlParameter("@v",dtpValid.Value.Date),new SqlParameter("@s",sid)};int res=eid==-1?DatabaseHelper.SmartInsert("Expertise","INSERT INTO Expertise (date,result,valid_until,stage_id) VALUES (@d,@r,@v,@s)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE Expertise SET date=@d,result=@r,valid_until=@v,stage_id=@s WHERE expertise_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.Refresh();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadStages();dtpDate.Value=DateTime.Today;tbResult.Text="";dtpValid.Value=DateTime.Today.AddYears(1);cbStage.SelectedIndex=-1;eid=-1;lblH.Text="Новая экспертиза";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите экспертизу.");return;}LoadStages();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["expertise_id"].Value);lblH.Text="Редактировать экспертизу";if(r.Cells["date"].Value!=DBNull.Value)dtpDate.Value=Convert.ToDateTime(r.Cells["date"].Value);tbResult.Text=r.Cells["result"].Value?.ToString();if(r.Cells["valid_until"].Value!=DBNull.Value)dtpValid.Value=Convert.ToDateTime(r.Cells["valid_until"].Value);SelectById(cbStage,r.Cells["stage_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите экспертизу.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm("Удалить экспертизу?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM Expertise WHERE expertise_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["expertise_id"].Value))});if(res>=0){sb.Refresh();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,(kw,sc)=>{var dt=DatabaseHelper.ExecuteQuery($"SELECT e.expertise_id,e.date,e.result,e.valid_until,ps.stage_name AS stage,e.stage_id FROM Expertise e JOIN PreparationStage ps ON ps.stage_id=e.stage_id WHERE e.result LIKE @k OR ps.stage_name LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")});HideCols(grid,"expertise_id","stage_id");SetHeaders(grid,new[]{("date","Дата"),("result","Результат"),("valid_until","Действует до"),("stage","Этап")});return dt;},new[]{"Дата","Результат"},new[]{"e.date","e.result"});
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRINT RUNS
        // ══════════════════════════════════════════════════════════════════════
        private void BuildPrintRunsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlPrintRuns, "Учёт тиражей");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Тираж"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbYear)  =AddRow(scroll,"Год *",ref y);
            var (_,tbQty)   =AddRow(scroll,"Количество *",ref y);

            // Формат — с кнопкой «+»
            ComboBox cbFormat = null;
            var (_,_cbFormat)=AddComboWithAdd(scroll,"Формат *",ref y,
                onAddClick: afterSave => ShowInlineAddFormat(editPanel, newId =>
                {
                    LoadFormatsToCombo(cbFormat); SelectById(cbFormat, newId); afterSave();
                }));
            cbFormat = _cbFormat;

            var (_,cbPub)   =AddCombo(scroll,"Издание *",ref y);
            int eid=-1;

            void LoadAll(){LoadFormatsToCombo(cbFormat);cbPub.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id,title FROM Publication ORDER BY title").Rows)cbPub.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]),r["title"].ToString()));}

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(!int.TryParse(tbYear.Text,out int yr)||yr<1900||yr>2100){UIHelper.ShowError("Введите корректный год.");return;}if(!int.TryParse(tbQty.Text,out int qty)||qty<=0){UIHelper.ShowError("Количество должно быть > 0.");return;}if(cbFormat.SelectedItem==null||cbPub.SelectedItem==null){UIHelper.ShowError("Выберите формат и издание.");return;}int fid=((ComboItem)cbFormat.SelectedItem).Id,pid=((ComboItem)cbPub.SelectedItem).Id;var prm=new[]{new SqlParameter("@y",yr),new SqlParameter("@q",qty),new SqlParameter("@f",fid),new SqlParameter("@p",pid)};int res=eid==-1?DatabaseHelper.SmartInsert("PrintRun","INSERT INTO PrintRun (year,quantity,format_id,publication_id) VALUES (@y,@q,@f,@p)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE PrintRun SET year=@y,quantity=@q,format_id=@f,publication_id=@p WHERE print_run_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.Refresh();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadAll();tbYear.Text=DateTime.Now.Year.ToString();tbQty.Text="";cbFormat.SelectedIndex=cbPub.SelectedIndex=-1;eid=-1;lblH.Text="Новый тираж";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите тираж.");return;}LoadAll();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["print_run_id"].Value);lblH.Text="Редактировать тираж";tbYear.Text=r.Cells["year"].Value?.ToString();tbQty.Text=r.Cells["quantity"].Value?.ToString();SelectById(cbFormat,r.Cells["format_id"].Value);SelectById(cbPub,r.Cells["publication_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите тираж.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm("Удалить тираж?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM PrintRun WHERE print_run_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["print_run_id"].Value))});if(res>=0){sb.Refresh();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,(kw,sc)=>{var dt=DatabaseHelper.ExecuteQuery($"SELECT pr.print_run_id,pr.year,pr.quantity,f.format_name,p.title AS publication,pr.format_id,pr.publication_id FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id WHERE p.title LIKE @k OR f.format_name LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")});HideCols(grid,"print_run_id","format_id","publication_id");SetHeaders(grid,new[]{("year","Год"),("quantity","Количество"),("format_name","Формат"),("publication","Издание")});return dt;},new[]{"Год","Количество","Издание"},new[]{"pr.year","pr.quantity","p.title"});
        }

        // ══════════════════════════════════════════════════════════════════════
        // REPORTS & SETTINGS
        // ══════════════════════════════════════════════════════════════════════
        private void BuildReportsPanel()
        {
            pnlReports.Controls.Clear();
            pnlReports.Controls.Add(new ReportsPanel { Dock = DockStyle.Fill });
        }

        private void BuildSettingsPanel()
        {
            pnlSettings.Controls.Clear(); pnlSettings.Padding = new Padding(20);
            var lblTitle = UIHelper.MakeSectionTitle("Настройки системы"); lblTitle.Location = new Point(0, 0);
            var sep = new Panel { BackColor = AppColors.PanelBorder, Size = new Size(500, 1), Location = new Point(0, 46) };
            var lblSub = new Label { Text = "Учётные данные пользователей", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Location = new Point(0, 58), AutoSize = true };
            int y = 94;
            foreach (var (role, login, pass) in new[] { ("Администратор","admin","admin123"),("Редактор","editor","editor123"),("Менеджер","manager","manager123") })
            {
                var card = new Panel { Size = new Size(480, 58), Location = new Point(0, y), BackColor = Color.White };
                card.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(AppColors.PanelBorder), 0, 0, card.Width - 1, card.Height - 1);
                card.Controls.Add(new Label { Text = role, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Location = new Point(12, 8), AutoSize = true });
                card.Controls.Add(new Label { Text = $"Логин: {login}    Пароль: {pass}", Font = new Font("Segoe UI", 9f), ForeColor = AppColors.TextSecondary, Location = new Point(12, 30), AutoSize = true });
                pnlSettings.Controls.Add(card); y += 66;
            }
            var note = UIHelper.MakeLabel("Для смены паролей откройте AppUsers.cs"); note.ForeColor = AppColors.TextSecondary; note.Location = new Point(0, y + 8);
            pnlSettings.Controls.AddRange(new Control[] { lblTitle, sep, lblSub, note });
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMBO LOADERS
        // ══════════════════════════════════════════════════════════════════════
        private static void LoadAuthorsToCombo(ComboBox cb)   { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT author_id,surname+' '+name AS fio FROM Author ORDER BY surname").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["author_id"]), r["fio"].ToString())); }
        private static void LoadContractsToCombo(ComboBox cb) { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT c.contract_id,a.surname+' '+a.name+' ('+CONVERT(varchar,c.signing_date,104)+')' AS lbl FROM Contract c JOIN Author a ON a.author_id=c.author_id ORDER BY c.signing_date DESC").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["contract_id"]), r["lbl"].ToString())); }
        private static void LoadTypesToCombo(ComboBox cb)     { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT type_id,type_name FROM Type ORDER BY type_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["type_id"]), r["type_name"].ToString())); }
        private static void LoadSubjectsToCombo(ComboBox cb)  { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT subject_id,subject_name FROM Subject ORDER BY subject_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["subject_id"]), r["subject_name"].ToString())); }
        private static void LoadClassesToCombo(ComboBox cb)   { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT class_id,class_level FROM Class ORDER BY class_level").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["class_id"]), r["class_level"].ToString())); }
        private static void LoadFormatsToCombo(ComboBox cb)   { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT format_id,format_name FROM Format ORDER BY format_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["format_id"]), r["format_name"].ToString())); }
    }
}
