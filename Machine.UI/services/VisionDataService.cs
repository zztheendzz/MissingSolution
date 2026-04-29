using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public class VisionDataService
    {
        private readonly string connectionString;
        string conn = @"Server=(localdb)\MSSQLLocalDB;Database=VisionResult;Trusted_Connection=True;";
        public VisionDataService(string conn)
        {
            connectionString = conn;
        }

        // ✅ Insert 1 record
        public void Insert(VisionData data)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                INSERT INTO VisionData (TrayId, Row, Col, Result)
                VALUES (@TrayId, @Row, @Col, @Result)
            ", conn);

                cmd.Parameters.AddWithValue("@TrayId", data.TrayId);
                cmd.Parameters.AddWithValue("@Row", data.Row);
                cmd.Parameters.AddWithValue("@Col", data.Col);
                cmd.Parameters.AddWithValue("@Result", data.Result);

                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Insert batch (rất quan trọng cho vision)
        public void InsertBatch(List<VisionData> list)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (var tran = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand(@"
                INSERT INTO VisionData (TrayId, Row, Col, Result)
                VALUES (@TrayId, @Row, @Col, @Result)
            ", conn, tran);

                    cmd.Parameters.Add("@TrayId", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@Row", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@Col", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@Result", System.Data.SqlDbType.Int);

                    foreach (var data in list)
                    {
                        cmd.Parameters["@TrayId"].Value = data.TrayId;
                        cmd.Parameters["@Row"].Value = data.Row;
                        cmd.Parameters["@Col"].Value = data.Col;
                        cmd.Parameters["@Result"].Value = data.Result;

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

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                SELECT * FROM VisionData WHERE TrayId = @TrayId
            ", conn);

                cmd.Parameters.AddWithValue("@TrayId", trayId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new VisionData
                        {
                            Id = (int)reader["Id"],
                            TrayId = (int)reader["TrayId"],
                            Row = (int)reader["Row"],
                            Col = (int)reader["Col"],
                            Result =(int) reader["Result"],
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        });
                    }
                }
            }

            return result;
        }



    }
}
