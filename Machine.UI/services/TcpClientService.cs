using DocumentFormat.OpenXml.Spreadsheet;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
            byte[] buffer = new byte[5];

            while (true)
            {
                int totalRead = 0;

                while (totalRead < 5)
                {
                    int read = await stream.ReadAsync(buffer, totalRead, 5 - totalRead);
                    if (read == 0) return;
                    totalRead += read;
                }

                string msg = Encoding.ASCII.GetString(buffer);
                Console.WriteLine("Raw: " + msg);

                OnRawData?.Invoke(msg); // 👈 chỉ bắn raw
            }
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
