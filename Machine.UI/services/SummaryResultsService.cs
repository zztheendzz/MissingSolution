using Dapper;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public class SummaryResultsService
    {
        private readonly string _conn;

        public SummaryResultsService(string conn)
        {
            _conn = conn;
        }

        public List<SummaryResults> GetSummaryByModel(DateTime from, DateTime to)
        {
            try
            {
                using (var conn = new SQLiteConnection(_conn))
                {
                    conn.Open();

                    // lấy hết ngày cuối
                    to = to.Date.AddDays(1).AddTicks(-1);

                    var data = conn.Query<SummaryResults>(@"
                SELECT 
                    t.TrayName AS Model,

                    COUNT(*) AS Total,

                    SUM(CASE WHEN v.Result = 1 THEN 1 ELSE 0 END) AS Ok,
                    SUM(CASE WHEN v.Result = 0 THEN 1 ELSE 0 END) AS Ng,
                    SUM(CASE WHEN v.Result IS NULL OR v.Result NOT IN (0,1) THEN 1 ELSE 0 END) AS None,

                    ROUND(
                        SUM(CASE WHEN v.Result = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*),
                        2
                    ) AS OkRate

                FROM VisionData v
                JOIN TrayRun t ON v.TrayId = t.Id

                WHERE datetime(v.CreatedAt) BETWEEN @from AND @to

                GROUP BY t.TrayName
                ORDER BY t.TrayName;
            ", new
                    {
                        from = from.ToString("yyyy-MM-dd HH:mm:ss"),
                        to = to.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList();

                    return data;
                }
            }
            catch (Exception ex)
            {
                // nếu bạn có richtextbox thì log vào đây
                // richTextBox1.AppendText(ex.ToString());

                throw; // hoặc return null;
            }
        }
    }
}
