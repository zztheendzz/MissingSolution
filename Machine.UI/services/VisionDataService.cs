using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Machine.UI.services
{
    public class VisionDataService
    {
        private readonly string connectionString;

        public VisionDataService(string conn)
        {
            connectionString = conn;
        }

        // ✅ Insert 1 record
        public void Insert(VisionData data)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SQLiteCommand(@"
                    INSERT INTO VisionData (TrayId, Row, Col, Result, CreatedAt)
                    VALUES (@TrayId, @Row, @Col, @Result, @CreatedAt)
                ", conn);

                cmd.Parameters.AddWithValue("@TrayId", data.TrayId);
                cmd.Parameters.AddWithValue("@Row", data.Row);
                cmd.Parameters.AddWithValue("@Col", data.Col);
                cmd.Parameters.AddWithValue("@Result", data.Result);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Insert batch (rất quan trọng)
        public void InsertBatch(List<VisionData> list)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                using (var tran = conn.BeginTransaction())
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO VisionData (TrayId, Row, Col, Result, CreatedAt)
                        VALUES (@TrayId, @Row, @Col, @Result, @CreatedAt)
                    ", conn, tran);

                    cmd.Parameters.Add("@TrayId", System.Data.DbType.Int32);
                    cmd.Parameters.Add("@Row", System.Data.DbType.Int32);
                    cmd.Parameters.Add("@Col", System.Data.DbType.Int32);
                    cmd.Parameters.Add("@Result", System.Data.DbType.Int32);
                    cmd.Parameters.Add("@CreatedAt", System.Data.DbType.String);

                    foreach (var data in list)
                    {
                        cmd.Parameters["@TrayId"].Value = data.TrayId;
                        cmd.Parameters["@Row"].Value = data.Row;
                        cmd.Parameters["@Col"].Value = data.Col;
                        cmd.Parameters["@Result"].Value = data.Result;
                        cmd.Parameters["@CreatedAt"].Value = data.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                        Console.WriteLine("data.Result importDB = " + data.Result);

                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
            }
        }

        // ✅ Get theo Tray
        public List<VisionData> GetByTray(int trayId)
        {
            var result = new List<VisionData>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SQLiteCommand(@"
            SELECT * FROM VisionData WHERE TrayId = @TrayId
        ", conn);

                cmd.Parameters.AddWithValue("@TrayId", trayId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 🔥 xử lý CreatedAt an toàn
                        DateTime createdAt = DateTime.MinValue;
                        var raw = reader["CreatedAt"]?.ToString();

                        if (!string.IsNullOrEmpty(raw))
                        {
                            // 1. thử parse dạng string datetime
                            if (!DateTime.TryParse(raw, out createdAt))
                            {
                                // 2. fallback nếu là số (dữ liệu cũ)
                                if (long.TryParse(raw, out var num))
                                {
                                    try
                                    {
                                        createdAt = DateTimeOffset.FromUnixTimeSeconds(num).DateTime;
                                    }
                                    catch
                                    {
                                        createdAt = DateTime.MinValue;
                                    }
                                }
                            }
                        }

                        result.Add(new VisionData
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TrayId = Convert.ToInt32(reader["TrayId"]),
                            Row = Convert.ToInt32(reader["Row"]),
                            Col = Convert.ToInt32(reader["Col"]),
                            Result = Convert.ToInt32(reader["Result"]),
                            CreatedAt = createdAt
                        });
                    }
                }
            }

            return result;
        }
    }
}