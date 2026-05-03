using Machine.UI.model;
using System;
using System.Data.SQLite;

namespace Machine.UI.services
{
    public class TrayRunService
    {
        private readonly string connectionString;

        public TrayRunService(string conn)
        {
            connectionString = conn;
        }

        // ✅ Create + lấy Id (SQLite 
        public int Create(TrayRun tray)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SQLiteCommand(@"
                    INSERT INTO TrayRun (TrayName, Row, Col, StartTime)
                    VALUES (@TrayName, @Row, @Col, @StartTime);
                    SELECT last_insert_rowid();
                ", conn);

                cmd.Parameters.AddWithValue("@TrayName", tray.TrayName);
                cmd.Parameters.AddWithValue("@Row", tray.Row);
                cmd.Parameters.AddWithValue("@Col", tray.Col);
                cmd.Parameters.AddWithValue("@StartTime", tray.StartTime);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ✅ Update EndTime
        public void UpdateEndTime(int trayId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SQLiteCommand(@"
                    UPDATE TrayRun
                    SET EndTime = @EndTime
                    WHERE Id = @Id
                ", conn);

                cmd.Parameters.AddWithValue("@EndTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", trayId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}