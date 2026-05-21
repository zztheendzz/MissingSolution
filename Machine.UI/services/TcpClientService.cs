using DocumentFormat.OpenXml.Spreadsheet;
using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cell = Machine.UI.model.Cell;

namespace Machine.UI.services
{
    public class TcpClientService
    {
        private TcpClient client;
        private NetworkStream stream;

        private CancellationTokenSource cts;

        private readonly object _lock = new object();

        private string cache = "";

        private ReceiveMode currentMode;

        public Action<string> OnRawData;
        public int modeData=5;
        public async Task Connect(string ip, int port)
        {
            client = new TcpClient();

            await client.ConnectAsync(ip, port);

            stream = client.GetStream();

            _ = ReceiveData();
        }

        private async Task ReceiveData()
        {
            byte[] buffer = new byte[1024];
            string cache = "";
            cts = new CancellationTokenSource();
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                var check = token.IsCancellationRequested;
                Console.WriteLine("check: " + check);
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                cache += Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Cache first: " + cache);
                if (modeData==5) {
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
                        Console.WriteLine("clean: " + clean);
                        OnRawData?.Invoke(clean); // 
                    }

                }
                else
                {
                    while (cache.Count(c => c == ',') >= 3)
                    {
                        int idx = GetIndexOfNthComma(cache, 3);
                        Console.WriteLine("idx: " + idx);
                        // 👉 tìm luôn ký tự sau dấu , cuối (tức là số thứ 5)
                        int end = idx + 2; // dấu , + 1 ký tự số

                        if (cache.Length < end) break; // chưa đủ dữ liệu

                        string packet = cache.Substring(0, end);
                        cache = cache.Substring(end);

                        Console.WriteLine("Packet: " + packet);

                        string clean = packet.Replace(",", "");
                        Console.WriteLine("clean: " + clean);
                        OnRawData?.Invoke(clean); // 
                    }

                }


            }
        }

        private void ProcessReceivedData(string data)
        {
            lock (_lock)
            {
                cache += data;

                Console.WriteLine("Cache: " + cache);

                int expectedComma =
                    currentMode == ReceiveMode.FiveField
                    ? 4
                    : 3;

                while (cache.Count(c => c == ',') >= expectedComma)
                {
                    int idx = GetIndexOfNthComma(
                        cache,
                        expectedComma
                    );

                    if (idx < 0)
                        return;

                    string packet = cache.Substring(0, idx + 2);

                    cache = cache.Substring(idx + 2);

                    Console.WriteLine("Packet: " + packet);
                    Console.WriteLine("cache: " + cache);

                    string clean = packet.TrimEnd(',');
                    OnRawData?.Invoke(packet);

                }
            }
        }
        public void CancelToken()
        {
            if (cts != null)
            {
                MessageBox.Show("cancel token");
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
            else { MessageBox.Show("cancel = null"); }
        }
        public void SetMode(ReceiveMode mode)
        {
            lock (_lock)
            {
                cache = "";

                currentMode = mode;

                Console.WriteLine($"Change mode: {mode}");
            }
        }
        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                stream?.Close();
                stream?.Dispose();
                client?.Close();
                stream = null;
                client = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
    }
    public enum ReceiveMode
    {
        FourField,
        FiveField
    }

}
