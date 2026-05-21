using System;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace Machine.UI.Services
{
    public class BackupDbService
    {
        private readonly string dbPath;

        // thư mục backup chính
        private readonly string backupRootFolder = @"D:\BackupData";

        // file json nằm trong project
        private readonly string configPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "configDB",
                "backup_config.json");

        public BackupDbService()
        {
            // db trong project
            dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "DB",
                "Missing.db");

            // tạo thư mục backup nếu chưa có
            if (!Directory.Exists(backupRootFolder))
            {
                Directory.CreateDirectory(backupRootFolder);
            }

            // tạo thư mục config nếu chưa có
            string configFolder = Path.GetDirectoryName(configPath);

            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
        }

        public void CheckAndBackup()
        {
            BackupConfig config = LoadConfig();

            // chưa backup lần nào
            if (config.LastBackupTime == DateTime.MinValue)
            {
                BackupAndReset();
                return;
            }

            // quá 3 ngày
            if (DateTime.Now >= config.LastBackupTime.AddDays(3))
            {
                BackupAndReset();
            }
        }

        private void BackupAndReset()
        {
            try
            {
                if (!File.Exists(dbPath))
                    return;

                // tên thời gian
                string backupTime =
                    DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

                // thư mục backup
                string backupFolder =
                    Path.Combine(backupRootFolder, backupTime);

                Directory.CreateDirectory(backupFolder);

                // tên file backup trùng tên thư mục
                string backupFileName =
                    $"{Path.GetFileNameWithoutExtension(dbPath)}_{backupTime}.db";

                string backupPath =
                    Path.Combine(backupFolder, backupFileName);

                // copy db
                File.Copy(dbPath, backupPath, true);

                // reset data
                ResetDatabase();

                // lưu thời gian backup mới nhất
                SaveConfig(new BackupConfig
                {
                    LastBackupTime = DateTime.Now
                });

                Console.WriteLine($"Backup success: {backupPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ResetDatabase()
        {
            try
            {
                using (SQLiteConnection conn =
                       new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            DELETE FROM TrayRun;
                            DELETE FROM VisionData;
                            VACUUM;
                        ";

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private BackupConfig LoadConfig()
        {
            try
            {
                if (!File.Exists(configPath))
                    return new BackupConfig();

                string json = File.ReadAllText(configPath);

                return JsonSerializer.Deserialize<BackupConfig>(json)
                       ?? new BackupConfig();
            }
            catch
            {
                return new BackupConfig();
            }
        }

        private void SaveConfig(BackupConfig config)
        {
            string json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(configPath, json);
        }
    }

    public class BackupConfig
    {
        public DateTime LastBackupTime { get; set; }
    }
}