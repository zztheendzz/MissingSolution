using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
namespace Machine.UI.configDB
{
    public static class configDB
    {
        // 🔥 đường dẫn DB của bạn
        public static string DbPath => Path.Combine(
            Application.StartupPath,   // folder exe
            "DB",
            "Missing.db"
        );
        // 🔹 connection string
        public static string ConnectionString => $"Data Source={DbPath};Version=3;";

        // 🔹 tạo connection
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}
