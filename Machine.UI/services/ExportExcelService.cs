using ClosedXML.Excel;
using Dapper;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace Machine.UI.services
{
    public class ExportExcelService
    {
        private readonly string _conn;

        public ExportExcelService(string conn)
        {
            _conn = conn;
        }

        public void ExportFlatData(string path, DateTime from, DateTime to)
        {
            try
            {
                using (var wb = new XLWorkbook())
                using (var conn = new SQLiteConnection(_conn))
                {
                    conn.Open();

                    // 🔥 lấy hết ngày cuối
                    to = to.Date.AddDays(1).AddTicks(-1);

                    // ===== TRAY =====
                    var trays = conn.Query<TrayRun>("SELECT * FROM TrayRun").ToList();
                    var trayDict = trays.ToDictionary(t => t.Id, t => t);

                    // ===== DATA =====
                    var allData = conn.Query<VisionData>(@"
                SELECT 
                    Id,
                    TrayId,
                    Row,
                    Col,
                    Result,
                    datetime(CreatedAt) as CreatedAt
                FROM VisionData
                WHERE datetime(CreatedAt) BETWEEN @from AND @to
            ", new
                    {
                        from = from.ToString("yyyy-MM-dd HH:mm:ss"),
                        to = to.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList();

                    // ===== GROUP THEO TRAY =====
                    var grouped = allData
                        .Where(d => trayDict.ContainsKey(d.TrayId))
                        .GroupBy(d => d.TrayId);

                    foreach (var g in grouped)
                    {
                        var tray = trayDict[g.Key];

                        // 🔥 xử lý tên sheet an toàn
                        string sheetName = tray.TrayName;
                        sheetName = System.Text.RegularExpressions.Regex
                            .Replace(sheetName, @"[\\\/\?\*\[\]]", "_");

                        // tránh trùng tên sheet
                      //  if (wb.Worksheets.Any(x => x.Name == sheetName))
                            sheetName = sheetName + "_" + tray.Id;

                        var ws = wb.Worksheets.Add(sheetName);

                        int r = 1;

                        // ===== HEADER =====
                        ws.Cell(r, 1).Value = "Date";
                        ws.Cell(r, 2).Value = "Time";
                        ws.Cell(r, 3).Value = "TrayName";
                        ws.Cell(r, 4).Value = "TrayId";
                        ws.Cell(r, 5).Value = "ProductId";
                        ws.Cell(r, 6).Value = "Result";

                        ws.Range(1, 1, 1, 6).Style.Font.Bold = true;
                        r++;

                        // ===== DATA (SORT PRODUCTID) =====
                        foreach (var d in g.OrderBy(d => d.Row * tray.Col + d.Col))
                        {
                            DateTime dt = d.CreatedAt; // 🔥 không cần parse lại

                            int productId = d.Row * tray.Col + d.Col;

                            ws.Cell(r, 1).Value = dt.ToString("yyyy-MM-dd");
                            ws.Cell(r, 2).Value = dt.ToString("HH:mm:ss");
                            ws.Cell(r, 3).Value = tray.TrayName;
                            ws.Cell(r, 4).Value = tray.Id;
                            ws.Cell(r, 5).Value = productId;

                            string resultText =
                                d.Result == 1 ? "OK" :
                                d.Result == 0 ? "NG" : "EMPTY";

                            ws.Cell(r, 6).Value = resultText;

                            r++;
                        }

                        // format
                        ws.Columns().AdjustToContents();
                        ws.RangeUsed().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // ===== SAVE =====
                    wb.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi export: " + ex.ToString());
            }
        }

        // ================== GET DATA ==================
        public List<VisionData> GetByTray(int trayId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_conn))
                {
                    conn.Open();
                    string sql = "SELECT * FROM VisionData WHERE TrayId = @trayId";
                    return conn.Query<VisionData>(sql, new { trayId }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy dữ liệu: " + ex.Message);
                return new List<VisionData>();
            }
        }

        // ================== BUILD MATRIX ==================
        private string[,] BuildTrayMatrix(TrayRun tray, List<VisionData> data)
        {
            var matrix = new string[tray.Row, tray.Col];

            for (int i = 0; i < tray.Row; i++)
                for (int j = 0; j < tray.Col; j++)
                    matrix[i, j] = "";

            foreach (var item in data)
            {
                // tránh crash index
                if (item.Row < tray.Row && item.Col < tray.Col)
                {
                    matrix[item.Row, item.Col] = item.Result == 1 ? "OK" : "NG";
                }
            }

            return matrix;
        }

        // ================== EXPORT TRAY ==================
        public void ExportTray(TrayRun tray, List<VisionData> data, string path)
        {
            try
            {
                var matrix = BuildTrayMatrix(tray, data);

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Tray");

                    // ===== HEADER =====
                    ws.Cell(1, 1).Value = $"Tray: {tray.TrayName}";
                    ws.Cell(2, 1).Value = $"Start: {tray.StartTime}";
                    ws.Cell(3, 1).Value = $"End: {tray.EndTime}";

                    // ===== THỐNG KÊ =====
                    int total = data.Count;
                    int ok = data.Count(x => x.Result == 1);
                    int ng = total - ok;
                    double yield = total == 0 ? 0 : Math.Round((double)ok / total * 100, 2);

                    ws.Cell(1, 5).Value = $"OK: {ok}";
                    ws.Cell(2, 5).Value = $"NG: {ng}";
                    ws.Cell(3, 5).Value = $"Yield: {yield}%";

                    int startRow = 5;

                    // ===== GRID =====
                    for (int i = 0; i < tray.Row; i++)
                    {
                        for (int j = 0; j < tray.Col; j++)
                        {
                            var cell = ws.Cell(startRow + i, j + 1);
                            var val = matrix[i, j];

                            cell.Value = val;

                            // căn giữa
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            // border
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // màu
                            if (val == "OK")
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.Green;
                            }
                            else if (val == "NG")
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.Red;
                                cell.Style.Font.FontColor = XLColor.White;
                            }
                            else
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    ws.Rows().AdjustToContents();

                    wb.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi export Excel: " + ex.Message);
            }
        }

        // ================== EXPORT DATAGRIDVIEW ==================
        public void Export(DataGridView dgv, string filePath)
        {
            try
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Tray");

                    int rows = dgv.Rows.Count;
                    int cols = dgv.Columns.Count;

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            var val = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                            val = val.Replace("\n", " ");

                            var cell = ws.Cell(i + 1, j + 1);
                            cell.Value = val;

                            // căn giữa
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            // border
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // màu
                            if (val.Contains("OK"))
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.LimeGreen;
                            }
                            else if (val.Contains("NG"))
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.Red;
                                cell.Style.Font.FontColor = XLColor.White;
                            }
                            else
                            {
                                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    ws.Rows().AdjustToContents();

                    wb.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi export DataGridView: " + ex.Message);
            }
        }
    }
}