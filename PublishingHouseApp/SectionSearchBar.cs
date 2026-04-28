using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    // Встроенная строка поиска для каждого раздела.
    // Размещается над таблицей данных (DockStyle.Top).
    // При поиске не скрывает строки, а подсвечивает совпадения жёлтым цветом.
    // Кэширует полный набор данных — повторные запросы к БД только при смене сортировки.
    public class SectionSearchBar : Panel
    {
        private TextBox       _tbSearch;
        private ComboBox      _cbSort;
        private Button        _btnSearch;
        private Button        _btnClear;
        private Label         _lblCount;

        private DataGridView  _targetGrid;
        private Func<string, string, DataTable> _queryFunc; // (keyword, sortCol) => DataTable
        private string[]      _sortLabels;
        private string[]      _sortCols;
        private DataTable     _fullData;    // полный набор данных без фильтрации
        private string        _lastSort;    // последняя применённая сортировка
        private (string col, string header)[] _headers;    // маппинг col → заголовок
        private string[]      _hiddenCols;  // колонки которые нужно скрыть (ID и т.д.)

        // Цвета подсветки
        private static readonly Color HighlightColor = Color.FromArgb(255, 243, 176); // жёлтый
        private static readonly Color NormalColor    = Color.White;
        private static readonly Color AltColor       = Color.FromArgb(245, 248, 250); // чередование

        public SectionSearchBar()
        {
            Height    = 44;
            Dock      = DockStyle.Top;
            BackColor = AppColors.FormBackground;
            Padding   = new Padding(8, 6, 8, 6);
            Build();
        }

        // ── Инициализация ─────────────────────────────────────────────────────
        // Привязывает строку поиска к таблице.
        // headers — маппинг технических имён колонок на русские заголовки.
        // hiddenCols — список колонок которые нужно скрыть (например ID).
        public void Init(DataGridView grid,
                         Func<string, string, DataTable> queryFunc,
                         string[] sortLabels,
                         string[] sortColumns,
                         (string col, string header)[] headers = null,
                         string[] hiddenCols = null)
        {
            _targetGrid  = grid;
            _queryFunc   = queryFunc;
            _sortLabels  = sortLabels;
            _sortCols    = sortColumns;
            _headers     = headers;
            _hiddenCols  = hiddenCols;

            _cbSort.Items.Clear();
            _cbSort.Items.AddRange(sortLabels);
            _cbSort.SelectedIndex = 0;

            // Подписываемся на перекраску строк (отписываемся чтобы не дублировать)
            _targetGrid.RowPrePaint -= OnRowPrePaint;
            _targetGrid.RowPrePaint += OnRowPrePaint;

            Refresh();
        }

        // Сбрасывает кэш и перезагружает данные из БД.
        // Вызывается после CRUD-операций (добавление, редактирование, удаление).
        public void ReloadData()
        {
            _fullData = null;
            Refresh();
        }

        // ── Построение UI ─────────────────────────────────────────────────────
        private void Build()
        {
            // Поле ввода с placeholder-текстом
            _tbSearch = new TextBox
            {
                Width       = 220,
                Height      = 28,
                Font        = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White,
                ForeColor   = AppColors.TextPrimary
            };
            _tbSearch.GotFocus  += (s, e) => { if (_tbSearch.Text == "Поиск...") { _tbSearch.Text = ""; _tbSearch.ForeColor = AppColors.TextPrimary; } };
            _tbSearch.LostFocus += (s, e) => { if (string.IsNullOrEmpty(_tbSearch.Text)) { _tbSearch.Text = "Поиск..."; _tbSearch.ForeColor = AppColors.TextSecondary; } };
            _tbSearch.Text      = "Поиск...";
            _tbSearch.ForeColor = AppColors.TextSecondary;
            _tbSearch.KeyDown   += (s, e) => { if (e.KeyCode == Keys.Enter) Refresh(); };
            _tbSearch.TextChanged += (s, e) => { if (_tbSearch.ForeColor != AppColors.TextSecondary) Refresh(); };

            var lblSort = new Label
            {
                Text      = "Сортировка:",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true
            };

            // Список колонок для сортировки
            _cbSort = new ComboBox
            {
                Width         = 150,
                Font          = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            _cbSort.SelectedIndexChanged += (s, e) => Refresh();

            // Кнопка запуска поиска
            _btnSearch = new Button
            {
                Text      = "🔍",
                Width     = 32,
                Height    = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.ButtonPrimary,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += (s, e) => Refresh();

            // Кнопка сброса фильтра (появляется только когда что-то введено)
            _btnClear = new Button
            {
                Text      = "✕",
                Width     = 28,
                Height    = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.FormBackground,
                ForeColor = AppColors.TextSecondary,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.Click += (s, e) =>
            {
                _tbSearch.Text      = "Поиск...";
                _tbSearch.ForeColor = AppColors.TextSecondary;
                _btnClear.Visible   = false;
                Refresh();
            };

            // Счётчик найденных записей
            _lblCount = new Label
            {
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true
            };

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = AppColors.FormBackground
            };

            void SetMargin(Control c, int l, int t, int r, int b) =>
                c.Margin = new Padding(l, t, r, b);

            SetMargin(_tbSearch,  0, 1, 4, 0);
            SetMargin(_btnSearch, 0, 1, 2, 0);
            SetMargin(_btnClear,  0, 1, 8, 0);
            SetMargin(lblSort,    0, 5, 4, 0);
            SetMargin(_cbSort,    0, 1, 8, 0);
            SetMargin(_lblCount,  0, 7, 0, 0);

            flow.Controls.Add(_tbSearch);
            flow.Controls.Add(_btnSearch);
            flow.Controls.Add(_btnClear);
            flow.Controls.Add(lblSort);
            flow.Controls.Add(_cbSort);
            flow.Controls.Add(_lblCount);

            Controls.Add(flow);
        }

        // ── Поиск ────────────────────────────────────────────────────────────
        // Если поисковая строка пуста — показывает все записи без подсветки.
        // Если введено слово — все записи остаются видимыми,
        // строки с совпадением подсвечиваются жёлтым.
        public new void Refresh()
        {
            if (_targetGrid == null || _queryFunc == null) return;

            try
            {
                string keyword = _tbSearch.ForeColor == AppColors.TextSecondary
                    ? "" : _tbSearch.Text.Trim();

                string sortCol = (_cbSort.SelectedIndex >= 0 && _sortCols != null &&
                                   _cbSort.SelectedIndex < _sortCols.Length)
                    ? _sortCols[_cbSort.SelectedIndex]
                    : (_sortCols?.Length > 0 ? _sortCols[0] : "1");

                _btnClear.Visible = !string.IsNullOrEmpty(keyword);

                // Перезагружаем из БД только если нет кэша или изменилась сортировка
                if (_fullData == null || _lastSort != sortCol)
                {
                    // Запрашиваем ВСЕ строки (пустой keyword = без фильтра WHERE)
                    _fullData = _queryFunc("", sortCol);
                    _lastSort = sortCol;
                }

                // Всегда показываем полный набор строк
                _targetGrid.DataSource = _fullData;

                // Применяем русские заголовки и скрываем служебные колонки
                ApplyHeaders();

                // Сохраняем ключевое слово — оно используется при перекраске строк
                _currentKeyword = keyword;

                // Обновляем счётчик
                int total = _fullData?.Rows.Count ?? 0;
                if (string.IsNullOrEmpty(keyword))
                {
                    _lblCount.Text = total == 0
                        ? "Нет записей"
                        : $"{total} {Pluralize(total, "запись", "записи", "записей")}";
                }
                else
                {
                    // Считаем сколько строк содержат ключевое слово
                    int matched = CountMatches(_fullData, keyword);
                    _lblCount.Text = matched == 0
                        ? "Совпадений нет"
                        : $"{matched} из {total} совпадают";
                }

                // Запускаем перерисовку строк с новой подсветкой
                _targetGrid.Invalidate();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Ошибка поиска:\n{ex.Message}");
            }
        }

        // ── Применение заголовков ─────────────────────────────────────────────
        // Вызывается каждый раз после DataSource = — иначе при смене источника
        // колонки пересоздаются и теряют русские заголовки
        private void ApplyHeaders()
        {
            if (_targetGrid == null) return;

            // Скрываем служебные колонки (ID и т.д.)
            if (_hiddenCols != null)
                foreach (var col in _hiddenCols)
                    if (_targetGrid.Columns.Contains(col))
                        _targetGrid.Columns[col].Visible = false;

            // Устанавливаем русские заголовки
            if (_headers != null)
                foreach (var (col, hdr) in _headers)
                    if (_targetGrid.Columns.Contains(col))
                        _targetGrid.Columns[col].HeaderText = hdr;
        }

        // Текущее ключевое слово — используется обработчиком перекраски строк
        private string _currentKeyword = "";

        // ── Перекраска строк ──────────────────────────────────────────────────
        // Вызывается перед отрисовкой каждой строки.
        // Совпадающие строки красим в жёлтый, остальные — в белый/серый.
        private void OnRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                if (_targetGrid == null || e.RowIndex < 0 ||
                    e.RowIndex >= _targetGrid.Rows.Count) return;

                var row = _targetGrid.Rows[e.RowIndex];

                if (string.IsNullOrEmpty(_currentKeyword))
                {
                    // Поиск не активен — обычное чередование строк
                    row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                        ? NormalColor : AltColor;
                    return;
                }

                // Подсвечиваем строку если хоть одна видимая ячейка содержит слово
                bool match = RowContains(row, _currentKeyword);
                row.DefaultCellStyle.BackColor = match ? HighlightColor : NormalColor;
            }
            catch { /* некритичная ошибка перекраски — игнорируем */ }
        }

        // Проверяет: содержит ли хоть одна видимая ячейка строки ключевое слово
        private static bool RowContains(DataGridViewRow row, string keyword)
        {
            string kw = keyword.ToLowerInvariant();
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (!cell.Visible) continue;
                string val = cell.Value?.ToString() ?? "";
                if (val.ToLowerInvariant().Contains(kw)) return true;
            }
            return false;
        }

        // Считает количество строк DataTable содержащих ключевое слово
        private static int CountMatches(DataTable dt, string keyword)
        {
            if (dt == null || string.IsNullOrEmpty(keyword)) return 0;
            string kw = keyword.ToLowerInvariant();
            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    string val = row[col]?.ToString() ?? "";
                    if (val.ToLowerInvariant().Contains(kw)) { count++; break; }
                }
            }
            return count;
        }

        // Правильное склонение слова в зависимости от числа (1 запись, 3 записи, 5 записей)
        private static string Pluralize(int n, string one, string few, string many)
        {
            int abs = Math.Abs(n) % 100;
            int rem = abs % 10;
            if (abs >= 11 && abs <= 19) return many;
            if (rem == 1) return one;
            if (rem >= 2 && rem <= 4) return few;
            return many;
        }
    }
}
