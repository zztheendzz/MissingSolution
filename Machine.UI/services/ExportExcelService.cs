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
        private readonly Action<string> _log;
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
                    if (allData == null || !allData.Any())
                    {
                        var ws = wb.Worksheets.Add("NoData");
                        ws.Cell(1, 1).Value = "No data to export";

                        wb.SaveAs(path);

                        MessageBox.Show("No data to export.");
                        return;
                    }
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



    }
}