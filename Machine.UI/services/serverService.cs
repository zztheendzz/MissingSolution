using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    internal class serverService
    {
        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;

        private bool isRunning = false;

        public Action<string> OnReceive;
        public Action OnClientConnected;
        public Action OnClientDisconnected;
        public Action<string> OnStatus;

        public async Task Start(int port)
        {
            try
            {
                Console.WriteLine("Port robot socket = " + port);

                server = new TcpListener(IPAddress.Any, port);

                server.Start();
                isRunning = true;
                OnStatus?.Invoke("Server Started");
                _ = AcceptLoop();
            }
            catch (Exception ex)
            {
                OnStatus?.Invoke("Start Server Error: " + ex.Message);
            }
        }

        private async Task AcceptLoop()
        {
            while (isRunning)
            {
                try
                {
                    Console.WriteLine("Waiting client connect...");

                    client = await server.AcceptTcpClientAsync();

                    if (client != null)
                    {
                        client.NoDelay = true;

                        stream = client.GetStream();

                        Console.WriteLine("Client Connected");

                        OnClientConnected?.Invoke();
                        OnStatus?.Invoke("Robot Connected Socket");

                        _ = ReceiveLoop();
                    }
                    // chờ đến khi disconnect mới accept client mới
                    while (client != null && client.Connected)
                    {
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    OnStatus?.Invoke("Err Robot Connected Socket"+ ex.ToString());
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];

            while (isRunning && client != null)
            {
                try
                {
                    if (stream == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    int len = await stream.ReadAsync(buffer, 0, buffer.Length);

                    // disconnect
                    if (len == 0)
                    {
                        Console.WriteLine("Client Disconnected");

                        HandleDisconnect();

                        break;
                    }
                    //thread
                    string msg = Encoding.ASCII.GetString(buffer, 0, len);

                    msg = msg.Trim();

                    if (msg.StartsWith("trig"))
                    {
                        msg = msg.Substring(4);
                    }

                    Console.WriteLine("Receive: " + msg);

                    OnReceive?.Invoke(msg);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Receive IO Error: " + ex.Message);

                    HandleDisconnect();

                    break;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Socket Closed");

                    HandleDisconnect();

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Receive Error: " + ex.Message);

                    HandleDisconnect();

                    break;
                }
            }
        }

        public void SendToRobot(string msg)
        {
            try
            {
                if (client == null)
                {
                    OnStatus?.Invoke("Client null");
                    return;
                }

                if (!client.Connected)
                {
                    OnStatus?.Invoke("Client disconnected");
                    return;
                }

                if (stream == null)
                {
                    OnStatus?.Invoke("Stream null");
                    return;
                }

                if (!stream.CanWrite)
                {
                    OnStatus?.Invoke("Stream cannot write");
                    return;
                }
                string payload = msg.EndsWith("\r\n")
                    ? msg
                    : msg + "\r\n";

                byte[] data = Encoding.ASCII.GetBytes(payload);

                stream.WriteTimeout = 3000;

                stream.Write(data, 0, data.Length);

                stream.Flush();

                OnStatus?.Invoke($"[TCP → Robot] {payload.Replace("\r", "\\r").Replace("\n", "\\n")}");
            }
            catch (IOException ex)
            {
                OnStatus?.Invoke("Send IO Error: " + ex.Message);

                HandleDisconnect();
            }
            catch (ObjectDisposedException)
            {
                OnStatus?.Invoke("Socket disposed");

                HandleDisconnect();
            }
            catch (Exception ex)
            {
                OnStatus?.Invoke("Send Error: " + ex.Message);
                HandleDisconnect();
            }
        }

        private void HandleDisconnect()
        {
            try
            {
                stream?.Close();
                stream?.Dispose();
                OnStatus?.Invoke(" Robot Disconnected Socket");
            }
            catch(Exception ex){
                OnStatus?.Invoke("Err Robot Disconnected Socket" + ex.ToString());
            }

            try
            {
                client?.Close();
                client?.Dispose();
            }
            catch{}

            stream = null;
            client = null;
            OnClientDisconnected?.Invoke();
        }

        public void Disconnect()
        {
            isRunning = false;

            HandleDisconnect();

            try
            {
                server?.Stop();
            }
            catch
            {
            }
            server = null;
            OnStatus?.Invoke("Server Stopped");
        }
    }
}
