using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine.UI.popupForm
{
    public partial class FormSummary : Form
    {
        public FormSummary(List<SummaryResults> data)
        {
            InitializeComponent();
            InitGrid();
            LoadData(data);
        }

        private DataGridView dgv;

        private void InitGrid()
        {
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(dgv);

            Text = "Summary Result";
            Width = 800;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void LoadData(List<SummaryResults> data)
        {
            // ===== TÍNH TOTAL =====
            var totalRow = new SummaryResults
            {
                Model = "TOTAL",
                Total = data.Sum(x => x.Total),
                Ok = data.Sum(x => x.Ok),
                Ng = data.Sum(x => x.Ng),
                None = data.Sum(x => x.None)
            };

            // tính % OK
            totalRow.OkRate = totalRow.Total == 0
                ? 0
                : Math.Round(totalRow.Ok * 100.0 / totalRow.Total, 2);

            // thêm vào cuối
            data.Add(totalRow);

            dgv.DataSource = null;
            dgv.DataSource = data;

            // format %
            dgv.Columns["OKRate"].DefaultCellStyle.Format = "0.00'%'";

            // căn giữa
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dgv.Columns["Model"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Ok"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Ng"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["None"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["OkRate"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 🔥 tô màu + highlight dòng total
            dgv.CellFormatting += (s, e) =>
            {
                var row = dgv.Rows[e.RowIndex];

                // dòng TOTAL
                if (row.Cells["Model"].Value?.ToString() == "TOTAL")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }

                // NG đỏ
                if (dgv.Columns[e.ColumnIndex].Name == "NG" && e.Value != null)
                {
                    if (Convert.ToInt32(e.Value) > 0)
                        e.CellStyle.ForeColor = Color.Red;
                }

                // OK xanh
                if (dgv.Columns[e.ColumnIndex].Name == "OK" && e.Value != null)
                {
                    if (Convert.ToInt32(e.Value) > 0)
                        e.CellStyle.ForeColor = Color.Green;
                }
            };
        }
    }
}
