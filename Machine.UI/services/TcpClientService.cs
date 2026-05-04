using DocumentFormat.OpenXml.Spreadsheet;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cell = Machine.UI.model.Cell;

namespace Machine.UI.services
{
    public class TcpClientService
    {
        
        public Action<int, int, string> OnData;
        public Action<List<string>> OnBatchData;
        private int currentBatch = 0;

        private TcpClient client;
        private NetworkStream stream;
        public async Task Connect(string ip, int port)
        {
           

                client = new TcpClient();
                await client.ConnectAsync(ip, port);
                stream = client.GetStream();

                _ = ReceiveData(); // chạy background
        }

        public Action<string> OnRawData; // 👈 thay vì List<string>

        private async Task ReceiveData()
        {
            byte[] buffer = new byte[1024];
            string cache = "";

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                cache += Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Cache: " + cache);

                while (cache.Count(c => c == ',') >= 4)
                {
                    int idx = GetIndexOfNthComma(cache, 4);

                    // 👉 tìm luôn ký tự sau dấu , cuối (tức là số thứ 5)
                    int end = idx + 2; // dấu , + 1 ký tự số

                    if (cache.Length < end) break; // chưa đủ dữ liệu

                    string packet = cache.Substring(0, end);
                    cache = cache.Substring(end);

                    Console.WriteLine("Packet: " + packet);

                    string clean = packet.Replace(",", "");

                    OnRawData?.Invoke(clean); // "11111"
                }
            }
        }
        private int GetIndexOfNthComma(string str, int n)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ',')
                {
                    count++;
                    if (count == n)
                        return i;
                }
            }
            return -1;
        }
        public void Disconnect()
        {
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }
        }

    }
}
