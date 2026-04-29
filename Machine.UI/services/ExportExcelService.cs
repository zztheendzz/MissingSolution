using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine.UI.services
{
    public class ExportExcelService
    {
        public void Export(DataGridView dgv, string filePath)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Tray");

            int rows = dgv.Rows.Count;
            int cols = dgv.Columns.Count;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var val = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";

                    // bỏ xuống dòng
                    val = val.Replace("\n", " ");

                    var cell = ws.Cell(i + 1, j + 1);
                    cell.Value = val;

                    // 🎨 màu theo kết quả
                    if (val.Contains("OK"))
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.LimeGreen;
                    }
                    else if (val.Contains("NG"))
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Red;
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                    else if (val.Contains("NONE"))
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Gray;
                    }
                    // căn giữa
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
            }

            // auto fit
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();

            wb.SaveAs(filePath);
        }
    }
}