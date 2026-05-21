using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public class SocketService
    {
        private TcpClient client;
        private NetworkStream stream;

        public async Task<bool> Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();

                await client.ConnectAsync(ip, port);

                stream = client.GetStream();

                Console.WriteLine("Socket Connected");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect error: " + ex.Message);
                return false;
            }
        }

        public async Task Send(string message)
        {
            try
            {
                if (stream == null) return;

                byte[] data = Encoding.ASCII.GetBytes(message);

                await stream.WriteAsync(data, 0, data.Length);

                Console.WriteLine("Send: " + message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send error: " + ex.Message);
            }
        }

        public async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];

            while (client != null && client.Connected)
            {
                try
                {
                    int len = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (len == 0)
                    {
                        Console.WriteLine("Disconnected");
                        break;
                    }

                    string msg = Encoding.ASCII.GetString(buffer, 0, len);

                    Console.WriteLine("Receive: " + msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Receive error: " + ex.Message);
                    break;
                }
            }
        }

        public void Disconnect()
        {
            stream?.Close();
            client?.Close();

            stream = null;
            client = null;
        }
    }
}
