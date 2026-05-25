using Machine.UI.Services;
using Machine.UI.services;
using Machine.UI.Services;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
namespace Machine.UI.popupForm
{
    public partial class BackupViewerForm : Form
    {
        private BackupViewerService backupService =
            new BackupViewerService();

        public BackupViewerForm()
        {
            InitializeComponent();
        }

        private void BackupViewerForm_Load(
            object sender,
            EventArgs e)
        {


            dtFrom.Value =
                DateTime.Now.AddDays(-3);

            dtTo.Value =
                DateTime.Now;

            dgvData.AutoGenerateColumns = true;

            dgvData.AutoGenerateColumns =
                true;

            dgvData.AllowUserToAddRows =
                false;

            dgvData.AllowUserToDeleteRows =
                false;

            dgvData.ReadOnly =
                true;

            dgvData.MultiSelect =
                false;

            dgvData.SelectionMode =
                DataGridViewSelectionMode
                .FullRowSelect;

            dgvData.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode
                .Fill;

            dgvData.RowHeadersVisible =
                false;

            dgvData.RowTemplate.Height =
                34;
        }

        private void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                Cursor =
                    Cursors.WaitCursor;

                DateTime from =
                    dtFrom.Value;

                DateTime to =
                    dtTo.Value;

                DataTable dt =
                    backupService.SearchVisionData(
                        from,
                        to);

                // =====================================
                // TOTAL ROW
                // =====================================

                if (dt.Rows.Count > 0)
                {
                    DataRow totalRow =
                        dt.NewRow();

                    totalRow["TrayName"] =
                        "TOTAL";

                    if (dt.Columns.Contains("Model"))
                    {
                        totalRow["Model"] =
                            "-";
                    }

                    int total =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(
                                  r["Total"]));

                    int ok =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(
                                  r["OKCount"]));

                    int ng =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(
                                  r["NGCount"]));

                    int none =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(
                                  r["NoneCount"]));

                    totalRow["Total"] =
                        total;

                    totalRow["OKCount"] =
                        ok;

                    totalRow["NGCount"] =
                        ng;

                    totalRow["NoneCount"] =
                        none;

                    totalRow["OKRate"] =
                        total > 0
                            ? Math.Round(
                                ok * 100.0 / total,
                                2)
                            : 0;

                    if (dt.Columns.Contains("NGRate"))
                    {
                        totalRow["NGRate"] =
                            total > 0
                            ? Math.Round(
                                ng * 100.0 / total,
                                2)
                            : 0;
                    }

                    if (dt.Columns.Contains("NoneRate"))
                    {
                        totalRow["NoneRate"] =
                            total > 0
                            ? Math.Round(
                                none * 100.0 / total,
                                2)
                            : 0;
                    }

                    dt.Rows.Add(totalRow);
                }

                // =====================================
                // BIND DATA
                // =====================================

                dgvData.DataSource =
                    dt;

                // =====================================
                // GRID STYLE
                // =====================================

                dgvData.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment
                    .MiddleCenter;

                dgvData.ColumnHeadersDefaultCellStyle.Alignment =
                    DataGridViewContentAlignment
                    .MiddleCenter;

                dgvData.ColumnHeadersHeight =
                    42;

                dgvData.DefaultCellStyle.Font =
                    new Font(
                        "Segoe UI",
                        10F);

                dgvData.ColumnHeadersDefaultCellStyle.Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold);

                dgvData.EnableHeadersVisualStyles =
                    false;

                dgvData.ColumnHeadersDefaultCellStyle.BackColor =
                    Color.FromArgb(
                        30,
                        144,
                        255);

                dgvData.ColumnHeadersDefaultCellStyle.ForeColor =
                    Color.White;

                dgvData.GridColor =
                    Color.Gainsboro;

                dgvData.BackgroundColor =
                    Color.White;

                dgvData.BorderStyle =
                    BorderStyle.None;

                dgvData.AlternatingRowsDefaultCellStyle.BackColor =
                    Color.FromArgb(
                        248,
                        250,
                        252);

                // =====================================
                // FORMAT %
                // =====================================

                string[] rateCols =
                {
                    "OKRate",
                    "NGRate",
                    "NoneRate"
                };

                foreach (string col in rateCols)
                {
                    if (dgvData.Columns.Contains(col))
                {
                        dgvData.Columns[col]
                            .DefaultCellStyle.Format =
                        "0.00'%'";
                }
                }

                // =====================================
                // COLUMN WIDTH
                // =====================================

                if (dgvData.Columns.Contains("TrayName"))
                {
                    dgvData.Columns["TrayName"]
                        .FillWeight = 180;
                }

                if (dgvData.Columns.Contains("Model"))
                {
                    dgvData.Columns["Model"]
                        .FillWeight = 150;
                }

                // =====================================
                // ROW COLOR
                // =====================================

                foreach (DataGridViewRow row
                         in dgvData.Rows)
                {
                    string tray =
                        row.Cells["TrayName"]
                           .Value?
                           .ToString();

                    // TOTAL ROW

                    if (tray == "TOTAL")
                    {
                        row.DefaultCellStyle.BackColor =
                            Color.LightSkyBlue;

                        row.DefaultCellStyle.Font =
                            new Font(
                                "Segoe UI",
                                10F,
                                FontStyle.Bold);

                        continue;
                    }

                    // =============================
                    // OK RATE COLOR
                    // =============================

                    if (dgvData.Columns
                               .Contains("OKRate"))
                    {
                        double okRate = 0;

                        double.TryParse(
                            row.Cells["OKRate"]
                               .Value?
                               .ToString(),
                            out okRate);

                        if (okRate >= 95)
                        {
                            row.Cells["OKRate"]
                               .Style.BackColor =
                                Color.LightGreen;
                        }
                        else if (okRate >= 80)
                        {
                            row.Cells["OKRate"]
                               .Style.BackColor =
                                Color.Khaki;
                        }
                        else
                        {
                            row.Cells["OKRate"]
                               .Style.BackColor =
                                Color.LightCoral;
                        }
                    }

                    // =============================
                    // NG RATE COLOR
                    // =============================

                    if (dgvData.Columns
                               .Contains("NGRate"))
                    {
                        double ngRate = 0;

                        double.TryParse(
                            row.Cells["NGRate"]
                               .Value?
                               .ToString(),
                            out ngRate);

                        if (ngRate > 10)
                        {
                            row.Cells["NGRate"]
                               .Style.BackColor =
                                Color.LightCoral;
                        }
                    }

                    // =============================
                    // NONE RATE COLOR
                    // =============================

                    if (dgvData.Columns
                               .Contains("NoneRate"))
                    {
                        double noneRate = 0;

                        double.TryParse(
                            row.Cells["NoneRate"]
                               .Value?
                               .ToString(),
                            out noneRate);

                        if (noneRate > 5)
                        {
                            row.Cells["NoneRate"]
                               .Style.BackColor =
                                Color.Orange;
                        }
                    }
                }

                // =====================================
                // COUNT
                // =====================================

                lblCount.Text =
                    $"Rows: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor =
                    Cursors.Default;
            }
        }
        BackupViewerService service =
    new BackupViewerService();
        private CancellationTokenSource exportCts;

        private async void btnExport_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd =
                       new SaveFileDialog())
                {
                    sfd.Filter =
                        "Excel File|*.xlsx";

                    sfd.FileName =
                        "BackupExport_"
                        + DateTime.Now.ToString(
                            "yyyyMMdd_HHmmss")
                        + ".xlsx";

                    if (sfd.ShowDialog()
                        != DialogResult.OK)
                    {
                        return;
                    }

                    exportCts =
                        new CancellationTokenSource();

                    using (LoadingForm loading =
                        new LoadingForm())
                    {
                        loading.CancelRequested += () =>
                        {
                            exportCts.Cancel();
                        };

                        loading.Show();

                        try
                        {
                            await Task.Run(() =>
                            {
                    service.ExportFlatData(
                        sfd.FileName,
                        dtFrom.Value,
                                    dtTo.Value,
                                    exportCts.Token);
                            });

                            loading.Close();

                    MessageBox.Show(
                        "Export success.");
                }
                        catch (
                            OperationCanceledException)
                        {
                            loading.Close();

                            MessageBox.Show(
                                "Export cancelled.");
            }
            catch (Exception ex)
            {
                            loading.Close();

                MessageBox.Show(
                    ex.ToString());
            }
        }
                }
            }
            catch (Exception ex)
        {
                MessageBox.Show(
                    ex.ToString());
            }
        }
        private void ExportReportExcel_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd =
                       new SaveFileDialog())
                {
                    sfd.Filter =
                        "Excel Report File|*.xlsx";

                    sfd.FileName =
                        "ExportReport_"
                        + DateTime.Now.ToString(
                            "yyyyMMdd_HHmmss")
                        + ".xlsx";

                    if (sfd.ShowDialog()
                        != DialogResult.OK)
                    {
                        return;
                    }

                    BackupViewerService service =
                        new BackupViewerService();

                    service.ExportSummaryData(
                        sfd.FileName,
                        dtFrom.Value,
                        dtTo.Value);

                    MessageBox.Show(
                        "Export success.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString());
            }
        }

        private void lblCount_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}