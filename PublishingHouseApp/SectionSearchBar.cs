using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    /// <summary>
    /// Встроенная строка поиска для любого раздела.
    /// Размещается в toolbar над таблицей.
    /// При вводе текста фильтрует данные в привязанном DataGridView.
    /// </summary>
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

        public SectionSearchBar()
        {
            Height    = 44;
            Dock      = DockStyle.Top;
            BackColor = AppColors.FormBackground;
            Padding   = new Padding(8, 6, 8, 6);
            Build();
        }

        // ── Инициализация ─────────────────────────────────────────────────────
        public void Init(DataGridView grid,
                         Func<string, string, DataTable> queryFunc,
                         string[] sortLabels,
                         string[] sortColumns)
        {
            _targetGrid  = grid;
            _queryFunc   = queryFunc;
            _sortLabels  = sortLabels;
            _sortCols    = sortColumns;

            _cbSort.Items.Clear();
            _cbSort.Items.AddRange(sortLabels);
            _cbSort.SelectedIndex = 0;

            Refresh();
        }

        // ── Build UI ──────────────────────────────────────────────────────────
        private void Build()
        {
            _tbSearch = new TextBox
            {
                Width       = 220,
                Height      = 28,
                Font        = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White,
                ForeColor   = AppColors.TextPrimary
            };
            // Placeholder
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

            _cbSort = new ComboBox
            {
                Width         = 150,
                Font          = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat
            };
            _cbSort.SelectedIndexChanged += (s, e) => Refresh();

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

            _lblCount = new Label
            {
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppColors.TextSecondary,
                AutoSize  = true
            };

            // Layout через FlowLayoutPanel
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

        // ── Refresh (execute search) ──────────────────────────────────────────
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

                var dt = _queryFunc(keyword, sortCol);
                _targetGrid.DataSource = dt;

                int count = dt?.Rows.Count ?? 0;
                _lblCount.Text = count == 0
                    ? "Нет записей"
                    : $"{count} {Pluralize(count, "запись", "записи", "записей")}";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Ошибка поиска:\n{ex.Message}");
            }
        }

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
