using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    // ══════════════════════════════════════════════════════════════════════════
    // EDITOR FORM
    // ══════════════════════════════════════════════════════════════════════════
    public class EditorForm : RoleFormBase
    {
        private Panel pnlAuthors, pnlContracts, pnlPublications, pnlStages, pnlExpertise;
        private static readonly string[] Statuses = { "Запланирован", "В работе", "Завершён" };

        public EditorForm() : base("Редактор", "Редактор", UserRole.Editor)
        {
            BuildNav();
            BuildAllPanels();
        }

        private void BuildNav()
        {
            AddDashboardNav();   // 1. Главная — первая

            pnlAuthors      = AddContentPanel();
            pnlContracts    = AddContentPanel();
            pnlPublications = AddContentPanel();
            pnlStages       = AddContentPanel();
            pnlExpertise    = AddContentPanel();

            // Порядок сверху вниз
            pnlNav.Controls.Add(MakeNavButton("Авторы",           pnlAuthors));
            pnlNav.Controls.Add(MakeNavButton("Договоры",         pnlContracts));
            pnlNav.Controls.Add(MakeNavButton("Издания",          pnlPublications));
            pnlNav.Controls.Add(MakeNavButton("Этапы подготовки", pnlStages));
            pnlNav.Controls.Add(MakeNavButton("Экспертиза",       pnlExpertise));
        }

        private void BuildAllPanels()
        {
            try { BuildAuthorsPanel(); }      catch (Exception ex) { ShowErr(pnlAuthors, ex); }
            try { BuildContractsPanel(); }    catch (Exception ex) { ShowErr(pnlContracts, ex); }
            try { BuildPublicationsPanel(); } catch (Exception ex) { ShowErr(pnlPublications, ex); }
            try { BuildStagesPanel(); }       catch (Exception ex) { ShowErr(pnlStages, ex); }
            try { BuildExpertisePanel(); }    catch (Exception ex) { ShowErr(pnlExpertise, ex); }
        }

        private void ShowErr(Panel p, Exception ex)
        {
            var l = UIHelper.MakeLabel($"Ошибка загрузки:\n{ex.Message}");
            l.ForeColor = AppColors.ButtonDanger; l.Location = new Point(16, 16); p.Controls.Add(l);
        }

        // ── Authors ───────────────────────────────────────────────────────────
        private void BuildAuthorsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlAuthors, "Управление авторами");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Автор"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbS)=AddRow(scroll,"Фамилия *",ref y); var (_,tbN)=AddRow(scroll,"Имя *",ref y);
            var (_,tbP)=AddRow(scroll,"Отчество",ref y);  var (_,tbE)=AddRow(scroll,"Email",ref y);
            var (_,tbPh)=AddRow(scroll,"Телефон",ref y);  var (_,tbT)=AddRow(scroll,"ИНН",ref y);
            int eid=-1;

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(string.IsNullOrWhiteSpace(tbS.Text)||string.IsNullOrWhiteSpace(tbN.Text)){UIHelper.ShowError("Заполните обязательные поля.");return;}var prm=new[]{new SqlParameter("@s",tbS.Text.Trim()),new SqlParameter("@n",tbN.Text.Trim()),new SqlParameter("@p",NE(tbP.Text)),new SqlParameter("@e",NE(tbE.Text)),new SqlParameter("@ph",NE(tbPh.Text)),new SqlParameter("@t",NE(tbT.Text))};int res=eid==-1?DatabaseHelper.SmartInsert("Author","INSERT INTO Author (surname,name,patronymic,email,phone,tax_id) VALUES (@s,@n,@p,@e,@ph,@t)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE Author SET surname=@s,name=@n,patronymic=@p,email=@e,phone=@ph,tax_id=@t WHERE author_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{tbS.Text=tbN.Text=tbP.Text=tbE.Text=tbPh.Text=tbT.Text="";eid=-1;lblH.Text="Новый автор";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите автора.");return;}var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["author_id"].Value);lblH.Text="Редактировать автора";tbS.Text=r.Cells["surname"].Value?.ToString();tbN.Text=r.Cells["name"].Value?.ToString();tbP.Text=r.Cells["patronymic"].Value?.ToString();tbE.Text=r.Cells["email"].Value?.ToString();tbPh.Text=r.Cells["phone"].Value?.ToString();tbT.Text=r.Cells["tax_id"].Value?.ToString();editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите автора.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm($"Удалить «{r.Cells["surname"].Value} {r.Cells["name"].Value}»?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM Author WHERE author_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["author_id"].Value))});if(res>=0){sb.ReloadData();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT author_id,surname,name,patronymic,email,phone,tax_id FROM Author WHERE surname LIKE @k OR name LIKE @k OR email LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Фамилия","Имя","Email"},
            new[]{"surname","name","email"},
            new[] { ("surname","Фамилия"), ("name","Имя"), ("patronymic","Отчество"), ("email","Email"), ("phone","Телефон"), ("tax_id","ИНН") },
            new[] { "author_id" });
        }

        // ── Contracts ─────────────────────────────────────────────────────────
        private void BuildContractsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlContracts, "Управление договорами");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Договор"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,dSign)=AddDate(scroll,"Дата подписания *",ref y); var (_,dValid)=AddDate(scroll,"Действует до *",ref y);
            var (_,tbAmt)=AddRow(scroll,"Сумма *",ref y);
            ComboBox cbA = null;
            var (_,_cbA)=AddComboWithAdd(scroll,"Автор *",ref y,
                onAddClick: afterSave => ShowInlineAddAuthor(editPanel, newId =>
                {
                    LoadA(cbA); SelectById(cbA, newId); afterSave();
                }));
            cbA = _cbA;
            int eid=-1;

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(cbA.SelectedItem==null||string.IsNullOrWhiteSpace(tbAmt.Text)){UIHelper.ShowError("Заполните все поля.");return;}if(dValid.Value.Date<=dSign.Value.Date){UIHelper.ShowError("Дата окончания должна быть позже даты подписания.");return;}if(!decimal.TryParse(tbAmt.Text.Replace(',','.'),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out decimal amt)){UIHelper.ShowError("Сумма должна быть числом.");return;}int aid=((ComboItem)cbA.SelectedItem).Id;var prm=new[]{new SqlParameter("@sd",dSign.Value.Date),new SqlParameter("@vu",dValid.Value.Date),new SqlParameter("@am",amt),new SqlParameter("@ai",aid)};int res=eid==-1?DatabaseHelper.SmartInsert("Contract","INSERT INTO Contract (signing_date,valid_until,amount,author_id) VALUES (@sd,@vu,@am,@ai)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE Contract SET signing_date=@sd,valid_until=@vu,amount=@am,author_id=@ai WHERE contract_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadA(cbA);dSign.Value=DateTime.Today;dValid.Value=DateTime.Today.AddYears(1);tbAmt.Text="";cbA.SelectedIndex=-1;eid=-1;lblH.Text="Новый договор";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите договор.");return;}LoadA(cbA);var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["contract_id"].Value);lblH.Text="Редактировать договор";if(r.Cells["signing_date"].Value!=DBNull.Value)dSign.Value=Convert.ToDateTime(r.Cells["signing_date"].Value);if(r.Cells["valid_until"].Value!=DBNull.Value)dValid.Value=Convert.ToDateTime(r.Cells["valid_until"].Value);tbAmt.Text=r.Cells["amount"].Value?.ToString();SelectById(cbA,r.Cells["author_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите договор.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm($"Удалить договор №{r.Cells["contract_id"].Value}?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM Contract WHERE contract_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["contract_id"].Value))});if(res>=0){sb.ReloadData();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT c.contract_id,c.signing_date,c.valid_until,c.amount,a.surname+' '+a.name AS author,c.author_id FROM Contract c JOIN Author a ON a.author_id=c.author_id WHERE a.surname LIKE @k OR a.name LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Дата","Сумма","Автор"},
            new[]{"c.signing_date","c.amount","a.surname"},
            new[] { ("signing_date","Дата подписания"), ("valid_until","Действует до"), ("amount","Сумма"), ("author","Автор") },
            new[] { "contract_id", "author_id" });
        }

        // ── Publications ──────────────────────────────────────────────────────
        private void BuildPublicationsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlPublications, "Управление изданиями");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Издание"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbTi)=AddRow(scroll,"Название *",ref y); var (_,tbIs)=AddRow(scroll,"ISBN",ref y);
            ComboBox cbCo = null;
            var (_,_cbCo)=AddComboWithAdd(scroll,"Договор *",ref y, onAddClick: afterSave => ShowInlineAddContract(editPanel, newId => { LoadC(cbCo); SelectById(cbCo,newId); afterSave(); }));
            cbCo = _cbCo;
            ComboBox cbTy = null;
            var (_,_cbTy)=AddComboWithAdd(scroll,"Тип",ref y,       onAddClick: afterSave => ShowInlineAddType(editPanel,    newId => { LoadT(cbTy); SelectById(cbTy,newId); afterSave(); }));
            cbTy = _cbTy;
            ComboBox cbSu = null;
            var (_,_cbSu)=AddComboWithAdd(scroll,"Тематика",ref y,  onAddClick: afterSave => ShowInlineAddSubject(editPanel, newId => { LoadS(cbSu); SelectById(cbSu,newId); afterSave(); }));
            cbSu = _cbSu;
            ComboBox cbCl = null;
            var (_,_cbCl)=AddComboWithAdd(scroll,"Класс",ref y,     onAddClick: afterSave => ShowInlineAddClass(editPanel,   newId => { LoadCl(cbCl); SelectById(cbCl,newId); afterSave(); }));
            cbCl = _cbCl;
            int eid=-1;

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(string.IsNullOrWhiteSpace(tbTi.Text)||cbCo.SelectedItem==null){UIHelper.ShowError("Заполните обязательные поля.");return;}int cid=((ComboItem)cbCo.SelectedItem).Id;object tid=cbTy.SelectedItem!=null?(object)((ComboItem)cbTy.SelectedItem).Id:DBNull.Value;object sid=cbSu.SelectedItem!=null?(object)((ComboItem)cbSu.SelectedItem).Id:DBNull.Value;object clid=cbCl.SelectedItem!=null?(object)((ComboItem)cbCl.SelectedItem).Id:DBNull.Value;var prm=new[]{new SqlParameter("@ti",tbTi.Text.Trim()),new SqlParameter("@is",NE(tbIs.Text)),new SqlParameter("@co",cid),new SqlParameter("@ty",tid),new SqlParameter("@su",sid),new SqlParameter("@cl",clid)};int res=eid==-1?DatabaseHelper.SmartInsert("Publication","INSERT INTO Publication (title,isbn,contract_id,type_id,subject_id,class_id) VALUES (@ti,@is,@co,@ty,@su,@cl)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE Publication SET title=@ti,isbn=@is,contract_id=@co,type_id=@ty,subject_id=@su,class_id=@cl WHERE publication_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadC(cbCo);LoadT(cbTy);LoadS(cbSu);LoadCl(cbCl);tbTi.Text=tbIs.Text="";cbCo.SelectedIndex=cbTy.SelectedIndex=cbSu.SelectedIndex=cbCl.SelectedIndex=-1;eid=-1;lblH.Text="Новое издание";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите издание.");return;}LoadC(cbCo);LoadT(cbTy);LoadS(cbSu);LoadCl(cbCl);var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["publication_id"].Value);lblH.Text="Редактировать издание";tbTi.Text=r.Cells["title"].Value?.ToString();tbIs.Text=r.Cells["isbn"].Value?.ToString();SelectById(cbCo,r.Cells["contract_id"].Value);SelectById(cbTy,r.Cells["type_id"].Value);SelectById(cbSu,r.Cells["subject_id"].Value);SelectById(cbCl,r.Cells["class_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите издание.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm($"Удалить «{r.Cells["title"].Value}»?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM Publication WHERE publication_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["publication_id"].Value))});if(res>=0){sb.ReloadData();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT p.publication_id,p.title,p.isbn,p.contract_id,p.type_id,p.subject_id,p.class_id,t.type_name,s.subject_name,cl.class_level,a.surname+' '+a.name AS author FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Subject s ON s.subject_id=p.subject_id LEFT JOIN Class cl ON cl.class_id=p.class_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id WHERE p.title LIKE @k OR p.isbn LIKE @k OR a.surname LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Название","ISBN","Автор"},
            new[]{"p.title","p.isbn","a.surname"},
            new[] { ("title","Название"), ("isbn","ISBN"), ("type_name","Тип"), ("subject_name","Тематика"), ("class_level","Класс"), ("author","Автор") },
            new[] { "publication_id", "contract_id", "type_id", "subject_id", "class_id" });
        }

        // ── Stages ────────────────────────────────────────────────────────────
        private void BuildStagesPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlStages, "Этапы подготовки");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Этап"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbNm)=AddRow(scroll,"Название *",ref y); var (_,dtpSt)=AddDate(scroll,"Дата начала *",ref y);
            var (_,cbSt)=AddCombo(scroll,"Статус *",ref y); var (_,cbPu)=AddCombo(scroll,"Издание *",ref y);
            cbSt.Items.AddRange(Statuses); int eid=-1,pi=-1;

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(string.IsNullOrWhiteSpace(tbNm.Text)||cbSt.SelectedIndex<0||cbPu.SelectedItem==null){UIHelper.ShowError("Заполните все поля.");return;}int ni=cbSt.SelectedIndex;if(eid!=-1){if(ni<pi){UIHelper.ShowError("Нельзя вернуть статус назад.");return;}if(ni>pi+1){UIHelper.ShowError($"Следующий: «{Statuses[pi+1]}»");return;}}int pid=((ComboItem)cbPu.SelectedItem).Id;var prm=new[]{new SqlParameter("@n",tbNm.Text.Trim()),new SqlParameter("@d",dtpSt.Value.Date),new SqlParameter("@s",Statuses[ni]),new SqlParameter("@p",pid)};int res=eid==-1?DatabaseHelper.SmartInsert("PreparationStage","INSERT INTO PreparationStage (stage_name,start_date,status,publication_id) VALUES (@n,@d,@s,@p)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE PreparationStage SET stage_name=@n,start_date=@d,status=@s,publication_id=@p WHERE stage_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            void LoadP(){cbPu.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id,title FROM Publication ORDER BY title").Rows)cbPu.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]),r["title"].ToString()));}
            btnAdd.Click+=(s,e)=>{LoadP();tbNm.Text="";dtpSt.Value=DateTime.Today;cbSt.SelectedIndex=0;cbPu.SelectedIndex=-1;eid=-1;pi=-1;lblH.Text="Новый этап";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите этап.");return;}LoadP();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["stage_id"].Value);lblH.Text="Редактировать этап";tbNm.Text=r.Cells["stage_name"].Value?.ToString();if(r.Cells["start_date"].Value!=DBNull.Value)dtpSt.Value=Convert.ToDateTime(r.Cells["start_date"].Value);pi=Array.IndexOf(Statuses,r.Cells["status"].Value?.ToString());cbSt.SelectedIndex=pi;SelectById(cbPu,r.Cells["publication_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите этап.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm($"Удалить «{r.Cells["stage_name"].Value}»?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM PreparationStage WHERE stage_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["stage_id"].Value))});if(res>=0){sb.ReloadData();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT ps.stage_id,ps.stage_name,ps.start_date,ps.status,p.title AS publication,ps.publication_id FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id WHERE ps.stage_name LIKE @k OR ps.status LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Название","Статус","Дата"},
            new[]{"ps.stage_name","ps.status","ps.start_date"},
            new[] { ("stage_name","Название этапа"), ("start_date","Дата начала"), ("status","Статус"), ("publication","Издание") },
            new[] { "stage_id", "publication_id" });
        }

        // ── Expertise ─────────────────────────────────────────────────────────
        private void BuildExpertisePanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlExpertise, "Экспертиза");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140); var btnDel=MakeDangerToolBtn("Удалить",279);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit,btnDel});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Экспертиза"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,dtpD)=AddDate(scroll,"Дата *",ref y); var (_,tbR)=AddRow(scroll,"Результат *",ref y);
            var (_,dtpV)=AddDate(scroll,"Действует до *",ref y); var (_,cbSt)=AddCombo(scroll,"Этап *",ref y);
            int eid=-1;

            void LoadS(){cbSt.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT ps.stage_id,ps.stage_name+' ('+p.title+')' AS lbl FROM PreparationStage ps JOIN Publication p ON p.publication_id=ps.publication_id ORDER BY ps.stage_name").Rows)cbSt.Items.Add(new ComboItem(Convert.ToInt32(r["stage_id"]),r["lbl"].ToString()));}

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(string.IsNullOrWhiteSpace(tbR.Text)||cbSt.SelectedItem==null){UIHelper.ShowError("Заполните все поля.");return;}if(dtpV.Value.Date<=dtpD.Value.Date){UIHelper.ShowError("Дата окончания должна быть позже.");return;}int sid=((ComboItem)cbSt.SelectedItem).Id;var prm=new[]{new SqlParameter("@d",dtpD.Value.Date),new SqlParameter("@r",tbR.Text.Trim()),new SqlParameter("@v",dtpV.Value.Date),new SqlParameter("@s",sid)};int res=eid==-1?DatabaseHelper.SmartInsert("Expertise","INSERT INTO Expertise (date,result,valid_until,stage_id) VALUES (@d,@r,@v,@s)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE Expertise SET date=@d,result=@r,valid_until=@v,stage_id=@s WHERE expertise_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadS();dtpD.Value=DateTime.Today;tbR.Text="";dtpV.Value=DateTime.Today.AddYears(1);cbSt.SelectedIndex=-1;eid=-1;lblH.Text="Новая экспертиза";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите экспертизу.");return;}LoadS();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["expertise_id"].Value);lblH.Text="Редактировать экспертизу";if(r.Cells["date"].Value!=DBNull.Value)dtpD.Value=Convert.ToDateTime(r.Cells["date"].Value);tbR.Text=r.Cells["result"].Value?.ToString();if(r.Cells["valid_until"].Value!=DBNull.Value)dtpV.Value=Convert.ToDateTime(r.Cells["valid_until"].Value);SelectById(cbSt,r.Cells["stage_id"].Value);editPanel.Visible=true;};
            btnDel.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите экспертизу.");return;}var r=grid.SelectedRows[0];if(!UIHelper.Confirm("Удалить экспертизу?"))return;int res=DatabaseHelper.ExecuteNonQuery("DELETE FROM Expertise WHERE expertise_id=@id",new[]{new SqlParameter("@id",Convert.ToInt32(r.Cells["expertise_id"].Value))});if(res>=0){sb.ReloadData();editPanel.Visible=false;}};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT e.expertise_id,e.date,e.result,e.valid_until,ps.stage_name AS stage,e.stage_id FROM Expertise e JOIN PreparationStage ps ON ps.stage_id=e.stage_id WHERE e.result LIKE @k OR ps.stage_name LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Дата","Результат"},
            new[]{"e.date","e.result"},
            new[] { ("date","Дата"), ("result","Результат"), ("valid_until","Действует до"), ("stage","Этап") },
            new[] { "expertise_id", "stage_id" });
        }

        private static void LoadA(ComboBox cb)  { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT author_id,surname+' '+name AS fio FROM Author ORDER BY surname").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["author_id"]), r["fio"].ToString())); }
        private static void LoadC(ComboBox cb)  { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT c.contract_id,a.surname+' '+a.name+' ('+CONVERT(varchar,c.signing_date,104)+')' AS lbl FROM Contract c JOIN Author a ON a.author_id=c.author_id ORDER BY c.signing_date DESC").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["contract_id"]), r["lbl"].ToString())); }
        private static void LoadT(ComboBox cb)  { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT type_id,type_name FROM Type ORDER BY type_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["type_id"]), r["type_name"].ToString())); }
        private static void LoadS(ComboBox cb)  { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT subject_id,subject_name FROM Subject ORDER BY subject_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["subject_id"]), r["subject_name"].ToString())); }
        private static void LoadCl(ComboBox cb) { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT class_id,class_level FROM Class ORDER BY class_level").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["class_id"]), r["class_level"].ToString())); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MANAGER FORM
    // ══════════════════════════════════════════════════════════════════════════
    public class ManagerForm : RoleFormBase
    {
        private Panel pnlPublications, pnlPrintRuns, pnlReports;

        public ManagerForm() : base("Менеджер", "Менеджер", UserRole.Manager)
        {
            BuildNav();
            BuildAllPanels();
        }

        private void BuildNav()
        {
            AddDashboardNav();   // 1. Главная

            pnlPublications = AddContentPanel();
            pnlPrintRuns    = AddContentPanel();
            pnlReports      = AddContentPanel();

            pnlNav.Controls.Add(MakeNavButton("Издания", pnlPublications));
            pnlNav.Controls.Add(MakeNavButton("Тиражи",  pnlPrintRuns));
            pnlNav.Controls.Add(MakeNavButton("Отчёты",  pnlReports));
        }

        private void BuildAllPanels()
        {
            try { BuildPublicationsPanel(); } catch (Exception ex) { ShowErr(pnlPublications, ex); }
            try { BuildPrintRunsPanel(); }    catch (Exception ex) { ShowErr(pnlPrintRuns, ex); }
            BuildReportsPanel();
        }

        private void ShowErr(Panel p, Exception ex)
        {
            var l = UIHelper.MakeLabel($"Ошибка загрузки:\n{ex.Message}");
            l.ForeColor = AppColors.ButtonDanger; l.Location = new Point(16, 16); p.Controls.Add(l);
        }

        private void BuildPublicationsPanel()
        {
            var (grid, _, toolbar, sb) = BuildSplitLayout(pnlPublications, "Издания");
            var note = UIHelper.MakeLabel("Просмотр изданий"); note.ForeColor = AppColors.TextSecondary; note.Location = new Point(8, 12);
            toolbar.Controls.Add(note);
            sb.Init(grid, (kw, sc) =>
            {
                return DatabaseHelper.ExecuteQuery(
                    $"SELECT p.title AS Название,p.isbn AS ISBN,t.type_name AS Тип,s.subject_name AS Тематика,cl.class_level AS Класс,a.surname+' '+a.name AS Автор " +
                    $"FROM Publication p LEFT JOIN Type t ON t.type_id=p.type_id LEFT JOIN Subject s ON s.subject_id=p.subject_id LEFT JOIN Class cl ON cl.class_id=p.class_id LEFT JOIN Contract c ON c.contract_id=p.contract_id LEFT JOIN Author a ON a.author_id=c.author_id WHERE p.title LIKE @k OR a.surname LIKE @k ORDER BY {sc}",
                    new[] { new SqlParameter("@k", $"%{kw}%") });
            }, new[] { "Название","Автор" }, new[] { "p.title","a.surname" });
        }

        private void BuildPrintRunsPanel()
        {
            var (grid, editPanel, toolbar, sb) = BuildSplitLayout(pnlPrintRuns, "Учёт тиражей");
            var btnAdd=MakeToolBtn("+ Добавить",8,115); var btnEdit=MakeToolBtn("Редактировать",131,140);
            toolbar.Controls.AddRange(new Control[]{btnAdd,btnEdit});

            var scroll=new Panel{AutoScroll=true,Dock=DockStyle.Fill,Padding=new Padding(16)};
            var lblH=UIHelper.MakeSectionTitle("Тираж"); lblH.Location=new Point(0,0);
            int y=40;
            var (_,tbYr)=AddRow(scroll,"Год *",ref y); var (_,tbQt)=AddRow(scroll,"Количество *",ref y);
            ComboBox cbFm = null;
            var (_,_cbFm)=AddComboWithAdd(scroll,"Формат *",ref y,
                onAddClick: afterSave => ShowInlineAddFormat(editPanel, newId => { LoadFm(cbFm); SelectById(cbFm,newId); afterSave(); }));
            cbFm = _cbFm;
            var (_,cbPb)=AddCombo(scroll,"Издание *",ref y);
            int eid=-1;

            void LoadAll(){LoadFm(cbFm);cbPb.Items.Clear();foreach(DataRow r in DatabaseHelper.ExecuteQuery("SELECT publication_id,title FROM Publication ORDER BY title").Rows)cbPb.Items.Add(new ComboItem(Convert.ToInt32(r["publication_id"]),r["title"].ToString()));}

            AddSaveCancel(scroll,ref y,
                onSave:()=>{if(!int.TryParse(tbYr.Text,out int yr)||yr<1900||yr>2100){UIHelper.ShowError("Введите корректный год.");return;}if(!int.TryParse(tbQt.Text,out int qty)||qty<=0){UIHelper.ShowError("Количество должно быть > 0.");return;}if(cbFm.SelectedItem==null||cbPb.SelectedItem==null){UIHelper.ShowError("Выберите формат и издание.");return;}int fid=((ComboItem)cbFm.SelectedItem).Id,pid=((ComboItem)cbPb.SelectedItem).Id;var prm=new[]{new SqlParameter("@y",yr),new SqlParameter("@q",qty),new SqlParameter("@f",fid),new SqlParameter("@p",pid)};int res=eid==-1?DatabaseHelper.SmartInsert("PrintRun","INSERT INTO PrintRun (year,quantity,format_id,publication_id) VALUES (@y,@q,@f,@p)",prm):DatabaseHelper.ExecuteNonQuery("UPDATE PrintRun SET year=@y,quantity=@q,format_id=@f,publication_id=@p WHERE print_run_id=@id",Append(prm,new SqlParameter("@id",eid)));if(res>=0){sb.ReloadData();editPanel.Visible=false;}},
                onCancel:()=>editPanel.Visible=false);

            scroll.Controls.Add(lblH); editPanel.Controls.Add(scroll);
            btnAdd.Click+=(s,e)=>{LoadAll();tbYr.Text=DateTime.Now.Year.ToString();tbQt.Text="";cbFm.SelectedIndex=cbPb.SelectedIndex=-1;eid=-1;lblH.Text="Новый тираж";editPanel.Visible=true;};
            btnEdit.Click+=(s,e)=>{if(grid.SelectedRows.Count==0){UIHelper.ShowError("Выберите тираж.");return;}LoadAll();var r=grid.SelectedRows[0];eid=Convert.ToInt32(r.Cells["print_run_id"].Value);lblH.Text="Редактировать тираж";tbYr.Text=r.Cells["year"].Value?.ToString();tbQt.Text=r.Cells["quantity"].Value?.ToString();SelectById(cbFm,r.Cells["format_id"].Value);SelectById(cbPb,r.Cells["publication_id"].Value);editPanel.Visible=true;};
            WireDoubleClick(grid,btnEdit);
            sb.Init(grid,
            (kw, sc) => DatabaseHelper.ExecuteQuery($"SELECT pr.print_run_id,pr.year,pr.quantity,f.format_name,p.title AS publication,pr.format_id,pr.publication_id FROM PrintRun pr JOIN Format f ON f.format_id=pr.format_id JOIN Publication p ON p.publication_id=pr.publication_id WHERE p.title LIKE @k OR f.format_name LIKE @k ORDER BY {sc}",new[]{new SqlParameter("@k",$"%{kw}%")}),
            new[]{"Год","Количество","Издание"},
            new[]{"pr.year","pr.quantity","p.title"},
            new[] { ("year","Год"), ("quantity","Количество"), ("format_name","Формат"), ("publication","Издание") },
            new[] { "print_run_id", "format_id", "publication_id" });
        }

        private void BuildReportsPanel()
        {
            pnlReports.Controls.Clear();
            pnlReports.Controls.Add(new ReportsPanel { Dock = DockStyle.Fill });
        }

        private static void LoadFm(ComboBox cb) { cb.Items.Clear(); foreach (DataRow r in DatabaseHelper.ExecuteQuery("SELECT format_id,format_name FROM Format ORDER BY format_name").Rows) cb.Items.Add(new ComboItem(Convert.ToInt32(r["format_id"]), r["format_name"].ToString())); }
    }
}
