using Machine.UI.Services;
using Machine.UI.services;

using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

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

            dgvData.AllowUserToAddRows = false;

            dgvData.ReadOnly = true;

            dgvData.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
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

                // ===== TOTAL ROW =====
                if (dt.Rows.Count > 0)
                {
                    DataRow totalRow =
                        dt.NewRow();

                    totalRow["TrayName"] =
                        "TOTAL";

                    totalRow["Total"] =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(r["Total"]));

                    totalRow["OKCount"] =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(r["OKCount"]));

                    totalRow["NGCount"] =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(r["NGCount"]));

                    totalRow["NoneCount"] =
                        dt.AsEnumerable()
                          .Sum(r =>
                              Convert.ToInt32(r["NoneCount"]));

                    int total =
                        Convert.ToInt32(
                            totalRow["Total"]);

                    int ok =
                        Convert.ToInt32(
                            totalRow["OKCount"]);

                    totalRow["OKRate"] =
                        total > 0
                            ? Math.Round(
                                ok * 100.0 / total,
                                2)
                            : 0;

                    dt.Rows.Add(totalRow);
                }

                dgvData.DataSource =
                    dt;
                if (dgvData.Columns.Contains("OKRate"))
                {
                    dgvData.Columns["OKRate"].DefaultCellStyle.Format =
                        "0.00'%'";
                }
                lblCount.Text =
                    $"Rows: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
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
        private void btnExport_Click(
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

                    BackupViewerService service =
                        new BackupViewerService();

                    service.ExportFlatData(
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

        private void lblCount_Click(object sender, EventArgs e)
        {

        }

        private void ExportReportExcel_Click(object sender, EventArgs e)
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
    }
}