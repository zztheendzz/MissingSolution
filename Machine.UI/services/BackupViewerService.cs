using ClosedXML.Excel;
using Dapper;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace Machine.UI.services
{
    public class BackupViewerService
    {
        private readonly string backupRoot =
            @"D:\BackupData";
        private readonly string currentDb =
    Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "DB",
        "Missing.db");
        public DataTable SearchVisionData(
            DateTime from,
            DateTime to)
        {
            List<string> dbFiles =
                GetCandidateDbFiles(
                    from,
                    to);

            return LoadVisionData(
                dbFiles,
                from,
                to);
        }

        private List<string> GetCandidateDbFiles(
            DateTime from,
            DateTime to)
        {
            List<string> result =
                new List<string>();

            // =========================
            // BACKUP DB
            // =========================

            if (Directory.Exists(backupRoot))
            {
                // backup 3 ngày/lần
            DateTime searchFrom =
                from.AddDays(-3);

            DateTime searchTo =
                to.AddDays(3);

            string[] folders =
                Directory.GetDirectories(
                    backupRoot);

            foreach (string folder in folders)
            {
                try
                {
                    string folderName =
                        Path.GetFileName(folder);

                    DateTime backupTime =
                        DateTime.ParseExact(
                            folderName,
                            "yyyy_MM_dd_HH_mm_ss",
                            CultureInfo.InvariantCulture);

                    if (backupTime >= searchFrom &&
                        backupTime <= searchTo)
                    {
                        string dbFile =
                            Directory.GetFiles(
                                folder,
                                "*.db")
                            .FirstOrDefault();

                            if (!string.IsNullOrEmpty(dbFile) &&
                                File.Exists(dbFile))
                        {
                            result.Add(dbFile);
                        }
                    }
                }
                catch
                {

                }
                }
            }

            // =========================
            // CURRENT DB
            // =========================

                if (File.Exists(currentDb))
                {
                    result.Add(currentDb);
                }

            // =========================
            // REMOVE DUPLICATE
            // =========================

                result = result
                    .Distinct()
                    .ToList();
            }

            return result;
        }

        //    public void ExportSummaryData(
        //string path,
        //DateTime from,
        //DateTime to)
        //    {
        //        try
        //        {
        //            // lấy full ngày cuối
        //            to = to.Date
        //                   .AddDays(1)
        //                   .AddTicks(-1);

        //            // lấy data giống LoadVisionData
        //            DataTable dt =
        //                SearchVisionData(
        //                    from,
        //                    to);

        //            using (var wb = new XLWorkbook())
        //            {
        //                var ws =
        //                    wb.Worksheets.Add(
        //                        "VisionSummary");

        //                // =========================
        //                // HEADER
        //                // =========================

        //                for (int c = 0;
        //                     c < dt.Columns.Count;
        //                     c++)
        //                {
        //                    ws.Cell(1, c + 1)
        //                      .Value =
        //                        dt.Columns[c].ColumnName;

        //                    ws.Cell(1, c + 1)
        //                      .Style.Font.Bold = true;

        //                    ws.Cell(1, c + 1)
        //                      .Style.Fill.BackgroundColor =
        //                        XLColor.LightBlue;

        //                    ws.Cell(1, c + 1)
        //                      .Style.Alignment.Horizontal =
        //                        XLAlignmentHorizontalValues.Center;
        //                }

        //                // =========================
        //                // DATA
        //                // =========================

        //                int row = 2;

        //                foreach (DataRow dr in dt.Rows)
        //                {
        //                    for (int c = 0;
        //                         c < dt.Columns.Count;
        //                         c++)
        //                    {
        //                        ws.Cell(row, c + 1)
        //                          .Value =
        //                            dr[c]?.ToString();

        //                        ws.Cell(row, c + 1)
        //                          .Style.Alignment.Horizontal =
        //                            XLAlignmentHorizontalValues.Center;

        //                        ws.Cell(row, c + 1)
        //                          .Style.Alignment.Vertical =
        //                            XLAlignmentVerticalValues.Center;
        //                    }

        //                    // =====================
        //                    // COLOR OK RATE
        //                    // =====================

        //                    double okRate = 0;

        //                    double.TryParse(
        //                        dr["OKRate"]?.ToString(),
        //                        out okRate);

        //                    var rateCell =
        //                        ws.Cell(
        //                            row,
        //                            dt.Columns["OKRate"]
        //                              .Ordinal + 1);

        //                    if (okRate >= 95)
        //                    {
        //                        rateCell.Style.Fill
        //                            .BackgroundColor =
        //                            XLColor.LightGreen;
        //                    }
        //                    else if (okRate >= 80)
        //                    {
        //                        rateCell.Style.Fill
        //                            .BackgroundColor =
        //                            XLColor.LightYellow;
        //                    }
        //                    else
        //                    {
        //                        rateCell.Style.Fill
        //                            .BackgroundColor =
        //                            XLColor.LightPink;
        //                    }

        //                    row++;
        //                }

        //                // =========================
        //                // STYLE
        //                // =========================

        //                ws.Columns()
        //                  .AdjustToContents();

        //                ws.RangeUsed()
        //                  .Style.Border.OutsideBorder =
        //                    XLBorderStyleValues.Thin;

        //                ws.RangeUsed()
        //                  .Style.Border.InsideBorder =
        //                    XLBorderStyleValues.Thin;

        //                // freeze header
        //                ws.SheetView.FreezeRows(1);

        //                // =========================
        //                // SAVE
        //                // =========================

        //                wb.SaveAs(path);

        //                MessageBox.Show(
        //                    "Export summary success.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(
        //                "Export summary lỗi: "
        //                + ex.ToString());
        //        }
        //    }


        public void ExportSummaryData(
            string path,
            DateTime from,
            DateTime to)
        {
            try
            {
                // full ngày cuối
                to = to.Date
                       .AddDays(1)
                       .AddTicks(-1);

                // =========================
                // LOAD RAW DATA
                // =========================

                DataTable raw =
                    SearchVisionData(
                        from,
                        to);

                // =========================
                // GROUP BY TRAY NAME
                // =========================

                var grouped =
                    raw.AsEnumerable()
                       .GroupBy(r =>
                            r["TrayName"]
                            .ToString());

                DataTable dt =
                    new DataTable();

                dt.Columns.Add("TrayName");
                dt.Columns.Add("Total", typeof(int));
                dt.Columns.Add("OKCount", typeof(int));
                dt.Columns.Add("NGCount", typeof(int));
                dt.Columns.Add("NoneCount", typeof(int));
                dt.Columns.Add("OKRate", typeof(double));

                foreach (var g in grouped)
                {
                    int total =
                        g.Sum(x =>
                            Convert.ToInt32(
                                x["Total"]));

                    int ok =
                        g.Sum(x =>
                            Convert.ToInt32(
                                x["OKCount"]));

                    int ng =
                        g.Sum(x =>
                            Convert.ToInt32(
                                x["NGCount"]));

                    int none =
                        g.Sum(x =>
                            Convert.ToInt32(
                                x["NoneCount"]));

                    double okRate =
                        total == 0
                        ? 0
                        : Math.Round(
                            ok * 100.0 / total,
                            2);

                    DataRow row =
                        dt.NewRow();

                    row["TrayName"] =
                        g.Key;

                    row["Total"] =
                        total;

                    row["OKCount"] =
                        ok;

                    row["NGCount"] =
                        ng;

                    row["NoneCount"] =
                        none;

                    row["OKRate"] =
                        okRate;

                    dt.Rows.Add(row);
                }

                // =========================
                // EXPORT EXCEL
                // =========================

                using (var wb = new XLWorkbook())
                {
                    var ws =
                        wb.Worksheets.Add(
                            "VisionSummary");

                    // =====================
                    // FILTER TIME
                    // =====================

                    string filterText =
                        $"Filter Time: " +
                        $"{from:yyyy-MM-dd HH:mm:ss} " +
                        $"~ " +
                        $"{to:yyyy-MM-dd HH:mm:ss}";

                    ws.Cell(1, 1).Value =
                        filterText;

                    ws.Range(1, 1, 1, 6)
                      .Merge();

                    ws.Cell(1, 1)
                      .Style.Font.Bold = true;

                    ws.Cell(1, 1)
                      .Style.Font.FontSize = 14;

                    ws.Cell(1, 1)
                      .Style.Fill.BackgroundColor =
                        XLColor.LightYellow;

                    ws.Cell(1, 1)
                      .Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    // =====================
                    // HEADER
                    // =====================

                    for (int c = 0;
                         c < dt.Columns.Count;
                         c++)
                    {
                        ws.Cell(3, c + 1)
                          .Value =
                            dt.Columns[c].ColumnName;

                        ws.Cell(3, c + 1)
                          .Style.Font.Bold = true;

                        ws.Cell(3, c + 1)
                          .Style.Fill.BackgroundColor =
                            XLColor.LightBlue;

                        ws.Cell(3, c + 1)
                          .Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        ws.Cell(3, c + 1)
                          .Style.Alignment.Vertical =
                            XLAlignmentVerticalValues.Center;
                    }

                    // =====================
                    // DATA
                    // =====================

                    int r = 4;

                    foreach (DataRow dr in dt.Rows)
                    {
                        for (int c = 0;
                             c < dt.Columns.Count;
                             c++)
                        {
                            object value =
                                dr[c];

                            if (value is int intValue)
                            {
                                ws.Cell(r, c + 1)
                                  .Value = intValue;
                            }
                            else if (value is double doubleValue)
                            {
                                ws.Cell(r, c + 1)
                                  .Value = doubleValue;
                            }
                            else
                            {
                                ws.Cell(r, c + 1)
                                  .Value =
                                    value?.ToString();
                            }

                            ws.Cell(r, c + 1)
                              .Style.Alignment.Horizontal =
                                XLAlignmentHorizontalValues.Center;

                            ws.Cell(r, c + 1)
                              .Style.Alignment.Vertical =
                                XLAlignmentVerticalValues.Center;
                        }

                        // =================
                        // COLOR OK RATE
                        // =================

                        double rate =
                            Convert.ToDouble(
                                dr["OKRate"]);

                        var rateCell =
                            ws.Cell(
                                r,
                                dt.Columns["OKRate"]
                                  .Ordinal + 1);

                        if (rate >= 95)
                        {
                            rateCell.Style.Fill
                                .BackgroundColor =
                                XLColor.LightGreen;
                        }
                        else if (rate >= 80)
                        {
                            rateCell.Style.Fill
                                .BackgroundColor =
                                XLColor.LightYellow;
                        }
                        else
                        {
                            rateCell.Style.Fill
                                .BackgroundColor =
                                XLColor.LightPink;
                        }

                        r++;
                    }

                    // =====================
                    // STYLE
                    // =====================

                    ws.Columns()
                      .AdjustToContents();

                    ws.RangeUsed()
                      .Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;

                    ws.RangeUsed()
                      .Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    ws.SheetView.FreezeRows(3);

                    // =====================
                    // SAVE
                    // =====================

                    wb.SaveAs(path);
                }

                MessageBox.Show(
                    "Export summary success.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Export summary lỗi:\n"
                    + ex);
            }
        }



        private DataTable LoadVisionData(
            List<string> dbFiles,
            DateTime from,
            DateTime to)
        {
            DataTable finalTable =
                new DataTable();

            bool schemaCreated = false;

            foreach (string dbPath in dbFiles)
            {
                try
                {
                    using (SQLiteConnection conn =
                           new SQLiteConnection(
                               $"Data Source={dbPath};Version=3;"))
                    {
                        conn.Open();

                        string query = @"
                            SELECT
                                tr.Id as TrayRunId,

                                tr.TrayName,

                                datetime(tr.StartTime) as StartTime,

                                datetime(tr.EndTime) as EndTime,

                                COUNT(*) as Total,

                                SUM(CASE
                                        WHEN vd.Result = 1
                                        THEN 1
                                        ELSE 0
                                    END) as OKCount,

                                SUM(CASE
                                        WHEN vd.Result = 0
                                        THEN 1
                                        ELSE 0
                                    END) as NGCount,

                                SUM(CASE
                                        WHEN vd.Result = 2
                                        THEN 1
                                        ELSE 0
                                    END) as NoneCount,

                                ROUND(
                                    SUM(CASE
                                            WHEN vd.Result = 1
                                            THEN 1.0
                                            ELSE 0
                                        END)
                                    * 100.0
                                    / COUNT(*),
                                    2
                        ) as OKRate,

                        ROUND(
                            SUM(CASE
                                    WHEN vd.Result = 0
                                    THEN 1.0
                                    ELSE 0
                                END)
                            * 100.0
                            / COUNT(*),
                            2
                        ) as NGRate,

                        ROUND(
                            SUM(CASE
                                    WHEN vd.Result = 2
                                    THEN 1.0
                                    ELSE 0
                                END)
                            * 100.0
                            / COUNT(*),
                            2
                        ) as NoneRate

                            FROM VisionData vd

                            INNER JOIN TrayRun tr
                                ON vd.TrayId = tr.Id

                            WHERE tr.StartTime >= @from
                            AND tr.StartTime <= @to

                    GROUP BY
                        tr.TrayName


                    ORDER BY
                        tr.TrayName

                            ";

                        using (SQLiteCommand cmd =
                               new SQLiteCommand(
                                   query,
                                   conn))
                        {
                            cmd.Parameters.AddWithValue(
                                "@from",
                                from.ToString(
                                    "yyyy-MM-dd HH:mm:ss"));

                            cmd.Parameters.AddWithValue(
                                "@to",
                                to.ToString(
                                    "yyyy-MM-dd HH:mm:ss"));

                            SQLiteDataAdapter da =
                                new SQLiteDataAdapter(cmd);

                            DataTable temp =
                                new DataTable();

                            da.Fill(temp);

                            if (!schemaCreated)
                            {
                                finalTable = temp.Clone();
                                schemaCreated = true;
                            }

                            foreach (DataRow row in temp.Rows)
                            {
                                finalTable.ImportRow(row);
                            }
                        }

                        conn.Close();
                    }
                }
                catch(Exception ex)     
                {
                    MessageBox.Show(
    ex.ToString());
                }
            }

            // =========================
            // GỘP TẤT CẢ DB
            // =========================

            var grouped =
                finalTable.AsEnumerable()
                .GroupBy(x => new
                {
                    TrayName = x["TrayName"].ToString()
                });

            DataTable result =
                new DataTable();

            result.Columns.Add("TrayName");


            result.Columns.Add("Total", typeof(int));

            result.Columns.Add("OKCount", typeof(int));
            result.Columns.Add("NGCount", typeof(int));
            result.Columns.Add("NoneCount", typeof(int));

            result.Columns.Add("OKRate", typeof(double));
            result.Columns.Add("NGRate", typeof(double));
            result.Columns.Add("NoneRate", typeof(double));

            foreach (var g in grouped)
            {
                int total =
                    g.Sum(x =>
                        Convert.ToInt32(x["Total"]));

                int ok =
                    g.Sum(x =>
                        Convert.ToInt32(x["OKCount"]));

                int ng =
                    g.Sum(x =>
                        Convert.ToInt32(x["NGCount"]));

                int none =
                    g.Sum(x =>
                        Convert.ToInt32(x["NoneCount"]));

                double okRate =
                    total == 0
                    ? 0
                    : Math.Round(
                        ok * 100.0 / total,
                        2);

                double ngRate =
                    total == 0
                    ? 0
                    : Math.Round(
                        ng * 100.0 / total,
                        2);

                double noneRate =
                    total == 0
                    ? 0
                    : Math.Round(
                        none * 100.0 / total,
                        2);

                DataRow row =
                    result.NewRow();

                row["TrayName"] =
                    g.Key.TrayName;

                row["Total"] =
                    total;

                row["OKCount"] =
                    ok;

                row["NGCount"] =
                    ng;

                row["NoneCount"] =
                    none;

                row["OKRate"] =
                    okRate;

                row["NGRate"] =
                    ngRate;

                row["NoneRate"] =
                    noneRate;

                result.Rows.Add(row);
        }

            return result;
        }
        public void ExportFlatData(
    string path,
    DateTime from,
            DateTime to,
            CancellationToken token)
        {
            try
            {
                // full ngày cuối
                to = to.Date
                       .AddDays(1)
                       .AddTicks(-1);

                List<string> dbFiles =
                    GetCandidateDbFiles(from, to);

                if (dbFiles.Count == 0)
                {
                    MessageBox.Show(
                        "No backup database found.");

                    return;
                }

                using (var wb = new XLWorkbook())
                {
                    // giảm overhead
                    wb.CalculateMode =
                        XLCalculateMode.Manual;

                    foreach (string dbPath in dbFiles)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            using (var conn =
                                   new SQLiteConnection(
                                       $"Data Source={dbPath};Version=3;"))
                            {
                                conn.Open();

                                // =========================
                                // LOAD TRAY
                                // =========================

                                var trayDict =
                                    conn.Query<TrayRun>(
                                        "SELECT * FROM TrayRun")
                                    .ToDictionary(
                                        x => x.Id,
                                        x => x);

                                var trayDict =
                                    trays.ToDictionary(
                                        t => t.Id,
                                        t => t);

                                // =========================
                                // LOAD DATA
                                // =========================

                                var allData =
                                    conn.Query<VisionData>(@"
                                        SELECT 
                                    vd.Id,
                                    vd.TrayId,
                                    vd.Row,
                                    vd.Col,
                                    vd.Result,
                                    datetime(vd.CreatedAt) as CreatedAt

                                FROM VisionData vd

                                INNER JOIN TrayRun tr
                                    ON vd.TrayId = tr.Id

                                WHERE datetime(tr.StartTime)
                                        BETWEEN datetime(@from)
                                        AND datetime(@to)
                                        ",
                                    new
                                    {
                                        from = from.ToString(
                                            "yyyy-MM-dd HH:mm:ss"),

                                        to = to.ToString(
                                            "yyyy-MM-dd HH:mm:ss")
                                    })
                                    .ToList();

                                if (allData == null ||
                                    allData.Count == 0)
                                {
                                    continue;
                                }

                                // =========================
                                // GROUP
                                // =========================

                                var grouped =
                                    allData
                                    .Where(x =>
                                        trayDict.ContainsKey(
                                            x.TrayId))
                                    .GroupBy(x =>
                                        x.TrayId);

                                foreach (var g in grouped)
                                {
                                    token.ThrowIfCancellationRequested();
                                    var tray =
                                        trayDict[g.Key];

                                    // =========================
                                    // SHEET NAME
                                    // =========================

                                    string sheetName =
                                        tray.TrayName;

                                    sheetName =
                                        System.Text
                                        .RegularExpressions
                                        .Regex.Replace(
                                            sheetName,
                                            @"[\\\/\?\*\[\]]",
                                            "_");

                                    // excel limit 31 char
                                    if (sheetName.Length > 31)
                                    {
                                        sheetName =
                                            sheetName.Substring(
                                                0,
                                                31);
                                    }

                                    // =========================
                                    // GET OR CREATE SHEET
                                    // =========================

                                    IXLWorksheet ws;

                                    bool isNewSheet =
                                        !wb.Worksheets
                                           .Any(x =>
                                               x.Name ==
                                               sheetName);

                                    if (isNewSheet)
                                    {
                                        ws =
                                            wb.Worksheets
                                              .Add(sheetName);
                                    }
                                    else
                                    {
                                        ws =
                                            wb.Worksheet(
                                                sheetName);
                                    }

                                    // =========================
                                    // START COLUMN
                                    // =========================

                                    int startCol = 1;

                                    if (!isNewSheet)
                                    {
                                        startCol =
                                            (ws.LastColumnUsed()
                                               ?.ColumnNumber()
                                             ?? 0)
                                            + 2;
                                    }

                                    int r = 1;

                                    // =========================
                                    // HEADER
                                    // =========================

                                    string[] headers =
                                    {
                                "STT",
                                "Date",
                                "Time",
                                "TrayName",
                                "TrayId",
                                "ProductId",
                                "Row",
                                "Col",
                                "Result"
                            };

                                    for (int i = 0;
                                        i < headers.Length;
                                        i++)
                                    {
                                        ws.Cell(
                                        1,
                                            startCol + i)
                                          .Value =
                                            headers[i];
                                    }

                                    var headerRange =
                                    ws.Range(
                                        1,
                                        startCol,
                                        1,
                                            startCol + 8);

                                    headerRange.Style
                                               .Font.Bold = true;

                                    headerRange.Style
                                               .Fill
                                      .BackgroundColor =
                                        XLColor.LightBlue;

                                    // =========================
                                    // BUILD DATA
                                    // =========================

                                    List<object[]> rows =
                                        new List<object[]>(
                                            g.Count());

                                    int stt = 1;

                                    // =========================
                                    // DATA
                                    // =========================

                                    foreach (var d in g
                                        .OrderBy(x =>
                                            x.Row *
                                            tray.Col +
                                            x.Col))
                                    {
                                        token.ThrowIfCancellationRequested();
                                        DateTime dt =
                                            d.CreatedAt;

                                        int productId =
                                            d.Row *
                                            tray.Col +
                                            d.Col + 1;

                                        string resultText =
                                            d.Result == 1
                                            ? "OK"
                                            : d.Result == 0
                                                ? "NG"
                                                : "EMPTY";

                                        rows.Add(
                                            new object[]
                                            {
                                        stt,
                                            dt.ToString(
                                            "yyyy-MM-dd"),

                                        ws.Cell(r, startCol + 2)
                                          .Value =
                                            dt.ToString(
                                            "HH:mm:ss"),

                                        tray.TrayName,

                                        tray.Id,

                                        productId,

                                        d.Row,

                                        d.Col,

                                        resultText
                                            });

                                        stt++;
                                    }

                                    // =========================
                                    // INSERT DATA
                                    // =========================

                                    ws.Cell(2, startCol)
                                      .InsertData(rows);

                                    int lastRow =
                                        rows.Count + 1;

                                    // =========================
                                    // DATA RANGE
                                    // =========================

                                    var dataRange =
                                        ws.Range(
                                            1,
                                            startCol,
                                            lastRow,
                                            startCol + 8);

                                    // alignment
                                    dataRange.Style.Alignment
                                             .Horizontal =
                                        XLAlignmentHorizontalValues
                                        .Center;

                                    dataRange.Style.Alignment
                                             .Vertical =
                                        XLAlignmentVerticalValues
                                        .Center;

                                    // border
                                    dataRange.Style.Border
                                             .OutsideBorder =
                                        XLBorderStyleValues
                                        .Thin;

                                    dataRange.Style.Border
                                             .InsideBorder =
                                        XLBorderStyleValues
                                        .Thin;

                                    // =========================
                                    // RESULT COLOR
                                    // =========================

                                    for (int r = 2;
                                        r <= lastRow;
                                        r++)
                                        {
                                        var cell =
                                            ws.Cell(
                                                r,
                                                startCol + 8);

                                        string value =
                                            cell.GetString();

                                        if (value == "OK")
                                        {
                                            cell.Style.Fill
                                              .BackgroundColor =
                                                XLColor.LightGreen;
                                        }
                                        else if (value == "NG")
                                        {
                                            cell.Style.Fill
                                              .BackgroundColor =
                                                XLColor.LightPink;
                                        }
                                        else
                                        {
                                            cell.Style.Fill
                                              .BackgroundColor =
                                                XLColor.LightGray;
                                        }

                                        r++;
                                        stt++;
                                    }

                                    // =========================
                                    // WIDTH
                                    // =========================

                                    ws.Columns(
                                        startCol,
                                        startCol + 8)
                                      .Width = 15;
                                }

                                conn.Close();
                            }
                        }
                        catch
                        {

                        }
                    }

                    // =========================
                    // NO DATA
                    // =========================

                    if (wb.Worksheets.Count == 0)
                    {
                        var ws =
                            wb.Worksheets.Add(
                                "NoData");

                        ws.Cell(1, 1).Value =
                            "No data to export";
                    }

                    // =========================
                    // SAVE
                    // =========================

                    wb.SaveAs(path);

                    //MessageBox.Show(
                    //    "Export success.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi export:\n"
                    + ex);
            }
        }

    }
}