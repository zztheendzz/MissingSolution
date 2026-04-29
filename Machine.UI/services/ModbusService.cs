using Modbus.Device;
using Modbus.Extensions.Enron;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public class ModbusService
    {
        private TcpClient client;
        private ModbusIpMaster master;
        private bool isRunning = false;
        private bool lastTrigger = false;
        public Action<int> OnIndexReceived;
        public int index = 0;

        public Action<int, int> OnPositionReceived;
        public Action OnTrayCompleted;
        public async Task<bool> Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();

                await client.ConnectAsync(ip, port);

                master = ModbusIpMaster.CreateIp(client);

                isRunning = true;
                _ = ReadLoop();

                Console.WriteLine("Connected Modbus Robot");

                return true;
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect error: " + ex.Message);
            }

            return false;
        }
        public void WriteIndex(int index)
        {
            try {

                master.WriteSingleRegister(1, 2, (ushort)index);
            } catch (Exception ex) {
                Console.WriteLine("WriteIndex error: " + ex.Message);
            }

        }

        public void WriteBatch(int[] values)
        {
            try {

                ushort[] data = values.Select(v => (ushort)v).ToArray();

                master.WriteMultipleRegisters(1, data);
            } catch (Exception ex) {
                Console.WriteLine("WriteBatch error: " + ex.Message);
            }

        }
        public void WriteStringToRobot(ushort register ,string msg)
        {
            try {
                if (master == null) return;

                ushort[] data = msg
                    .Select(c => (ushort)(c - '0')) // 🔥 '1' → 1
                    .ToArray();

                master.WriteMultipleRegisters(register, data);

            }
            catch (Exception ex) {

                Console.WriteLine("WriteStringToRobot error: " + ex.Message + " Register: " + register + " Msg: " + msg);


            }

        }
        private async Task ReadLoop()
        {
            while (isRunning)
            {
                try
                {
                    ushort[] regs = master.ReadHoldingRegisters(1, 4, 1);
                    bool trigger = regs[0] == 1;

                    // 👇 chỉ ăn 1 lần khi 0 -> 1
                    if (trigger && !lastTrigger)
                    {
                        Console.WriteLine("Tray DONE");

                        // xử lý logic của bạn
                        OnTrayCompleted?.Invoke();

                        // 🔥 reset về 0 NGAY
                        master.WriteSingleRegister(1, 4, 0);
                    }

                    lastTrigger = trigger;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Modbus error: " + ex.Message);
                }

                await Task.Delay(50);
            }
        }

        public void Disconnect()
        {
            isRunning = false;
            client?.Close();
        }
    }
}

