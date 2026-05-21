using Modbus.Device;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
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

        public ushort triggered =3; // thông báo Trigger chụp ảnh từ robot (địa chỉ 3)
        public ushort trayDone = 4;// thông báo hoàn thành Tray từ robot (địa chỉ 4)
        public ushort receiveResult = 5; // thông báo nhận kết quả ng/ok  gửi cho robot (địa chỉ 5)

        public Action<int, int> OnPositionReceived;
        public Action OnTrayCompleted;
        public Action Trigger;

        CancellationTokenSource cts;
        public async Task<bool> Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();

                await client.ConnectAsync(ip, port);

                master = ModbusIpMaster.CreateIp(client);

                isRunning = true;
                // _ = WorkerLoop();
                // _ = writeLoop1();
                // _=readLoop1();
               // ReadAllBits();
               // WriteAllOn();
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
                if (!IsConnected())
                {
                    Console.WriteLine("Modbus not connected");
                    return;
                }
                master.WriteSingleRegister(1, 2, (ushort)index);
            } catch (Exception ex) {
                Console.WriteLine("WriteIndex error: " + ex.Message);
            }

        }

        public void WriteBatch(int[] values)
        {
            try {

                ushort[] data = values.Select(v => (ushort)v).ToArray();

                master.WriteMultipleRegisters(1, receiveResult, data);
            } catch (Exception ex) {
                Console.WriteLine("WriteBatch error: " + ex.Message);
            }

        }

        public void sendTrayToRobot(ushort currentTray)
        {
            try
            {
                // 30032 -> 31 
                ushort address = 31;// địa chỉ gửi tray của robot

                // 1111111111111111
                //31 - gửi mã tray
                ushort value = (ushort)( ushort.MinValue + currentTray);

                if (!IsConnected())
                {
                    Console.WriteLine("Modbus not connected");
                    return;
                }

                master.WriteSingleRegister(address, value);

                Console.WriteLine("Write OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write tray err");
            }
        }

        public void sendToRobot(ushort value, ushort address)
        {
            try
            {
                // 30032 -> 31 
             //   ushort address = 31;// địa chỉ gửi tray của robot

                // 1111111111111111
                //31 - gửi mã tray
                ushort _value = (ushort)(ushort.MinValue + value);

                if (!IsConnected())
                {
                    Console.WriteLine("Modbus not connected");
                    return;
                }

                master.WriteSingleRegister(address, _value);

                Console.WriteLine("Write OK value = " + _value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write tray err");
            }
        }

        public void ReadAllBits(ushort startAddress)
        {
            try
            {
                if (!IsConnected())
                {
                    Console.WriteLine("Modbus not connected");
                    return;
                }
             //    startAddress = 32; // 40032 -> offset 31
                ushort totalRegisters = 128;

                int maxPerRead = 125;
                int currentOffset = 0;

                    ushort[] registers = master.ReadInputRegisters(
                       1, startAddress, 0);
                Console.WriteLine("doc pickup register = " + string.Join(", ", registers));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read Modbus Error: {ex.Message}");
            }
        }



        //public void WriteAllOn()
        //{
        //    try
        //    {
        //        // 30032 -> 31
        //        ushort address = 31;
        //        if (!IsConnected())
        //        {
        //            Console.WriteLine("Modbus not connected");
        //            return;
        //        }
        //        // 1111111111111111
        //        //31 - gửi mã tray
        //        ushort value = ushort.MinValue+9;

        //        master.WriteSingleRegister(address, value);

        //        Console.WriteLine("Write OK");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
        //public void ReadAllBits()
        //{
        //    try
        //    {
        //        if (!IsConnected())
        //        {
        //            Console.WriteLine("Modbus not connected");
        //            return;
        //        }
        //        ushort startAddress = 31; // 40032 -> offset 31
        //        ushort totalRegisters = 128;

        //        int maxPerRead = 125;
        //        int currentOffset = 0;

        //        while (currentOffset < totalRegisters)
        //        {
        //            // ushort quantity = (ushort)Math.Min(maxPerRead, totalRegisters - currentOffset);
        //            ushort quantity = (ushort)Math.Min(maxPerRead, totalRegisters - currentOffset);

        //            ushort[] registers = master.ReadInputRegisters(
        //               1,31,0);

        //            for (int regIndex = 0; regIndex < registers.Length; regIndex++)
        //            {
        //                ushort value = registers[regIndex];

        //                int registerNumber = 40032 + currentOffset + regIndex;

        //                Console.WriteLine($"\nRegister {registerNumber} = {value}");

        //                // Read all 16 bits in the register
        //                for (int bit = 0; bit < 16; bit++)
        //                {
        //                    bool state = (value & (1 << bit)) != 0;

        //                    int fieldbusBit =
        //                        512 + ((currentOffset + regIndex) * 16) + bit;

        //                    Console.WriteLine(
        //                        $"Bit {fieldbusBit} = {(state ? 1 : 0)}");
        //                }
        //            }

        //            currentOffset += quantity;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Read Modbus Error: {ex.Message}");
        //    }
        //}
        public bool IsConnected()
        {
            try
            {
                if (client == null) return false;

                if (!client.Connected) return false;

                if (client.Client.Poll(0, SelectMode.SelectRead) && client.Available == 0)
                {
                    return false; // disconnected
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        //private async Task readLoop1()
        //{
        //    ushort a = 1;
        //    ushort b;
        //    while (a<159)
        //    {
        //        try
        //        {
        //          ushort[] r=  master.ReadHoldingRegisters (1, 31, a);
        //            //bool[] coil=master.ReadCoils (1, 512, a);
        //           Console.WriteLine("write r = " + r[0]);
        //           // Console.WriteLine("read coil = " + coil[0] + "length= " + coil.Count());
        //            a++;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Modbus Loop Error: " + ex.Message);
        //            Disconnect();
        //            // Nếu lỗi mất kết nối, có thể xử lý reconnect ở đây
        //        }

        //        // Nghỉ 50ms để Robot không bị quá tải và dành CPU cho việc xử lý ảnh
        //        await Task.Delay(50);
        //    }
        //}
        //private async Task writeLoop1()
        //{
        //    ushort a = 32;
        //    while (a < 159)
        //    {
        //        try
        //        {
        //            // Đọc gộp từ địa chỉ 3 (Trigger) và 4 (Tray Done) - Đọc 2 thanh ghi cùng lúc
        //            // Slave ID mặc định là 1.
        //            ushort[] triger = master.ReadHoldingRegisters(1, (ushort)40032, a);//30032
        //            Console.WriteLine(triger[0]);
        //            a++;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Modbus Loop Error: " + ex.Message);
        //            Disconnect();
        //            // Nếu lỗi mất kết nối, có thể xử lý reconnect ở đây
        //        }

        //        // Nghỉ 50ms để Robot không bị quá tải và dành CPU cho việc xử lý ảnh
        //        await Task.Delay(50);
        //    }
        //}
        //private async Task WorkerLoop()
        //{
        //    while (isRunning)
        //    {
        //        try
        //        {
        //            // Đọc gộp từ địa chỉ 3 (Trigger) và 4 (Tray Done) - Đọc 2 thanh ghi cùng lúc
        //            // Slave ID mặc định là 1.
        //            ushort[] triger = master.ReadHoldingRegisters(1, triggered, 1);
        //            ushort[] tray =  master.ReadHoldingRegisters(1, trayDone, 1);

        //            // 1. Xử lý Trigger chụp ảnh (Địa chỉ 3)
        //            if (triger[0] == 1)
        //            {
        //                Trigger?.Invoke();
        //                // Reset ngay lập tức để Robot biết PC đã nhận lệnh
        //                master.WriteSingleRegister(1, triggered, 0);
        //            }

        //            // 2. Xử lý Hoàn thành Tray (Địa chỉ 4)
        //            bool currentTrayStatus = (tray[0] == 1);
        //            if (currentTrayStatus )
        //            {

        //                OnTrayCompleted?.Invoke();
        //                // Reset ngay lập tức
        //                master.WriteSingleRegister(1, trayDone, 0);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Modbus Loop Error: " + ex.Message);
        //            Disconnect();
        //            // Nếu lỗi mất kết nối, có thể xử lý reconnect ở đây
        //        }

        //        // Nghỉ 50ms để Robot không bị quá tải và dành CPU cho việc xử lý ảnh
        //        await Task.Delay(50);
        //    }
        //}

        public void Disconnect()
        {
            try
            {
                isRunning = false;

                master?.Dispose();

                client?.Close();
                client?.Dispose();
            }
            catch
            {
            }

            master = null;
            client = null;

            Console.WriteLine("Modbus Disconnected");
        }
    }
}

