using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public class TrayRunService
    {
        private readonly string connectionString;

        public TrayRunService(string conn)
        {
            connectionString = conn;
        }

        public int Create(TrayRun tray)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                INSERT INTO TrayRun (TrayName, Row, Col, StartTime)
                OUTPUT INSERTED.Id
                VALUES (@TrayName, @Row, @Col, @StartTime)
            ", conn);

                cmd.Parameters.Add("@TrayName", SqlDbType.VarChar).Value = tray.TrayName;
                cmd.Parameters.Add("@Row", SqlDbType.Int).Value = tray.Row;
                cmd.Parameters.Add("@Col", SqlDbType.Int).Value = tray.Col;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = tray.StartTime;

                return (int)cmd.ExecuteScalar();
            }
        }

        public void UpdateEndTime(int trayId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                UPDATE TrayRun
                SET EndTime = @EndTime
                WHERE Id = @Id
            ", conn);

                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = trayId;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
