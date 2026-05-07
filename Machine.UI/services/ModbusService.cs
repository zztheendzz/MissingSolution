using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Modbus.Device;
using Modbus.Extensions.Enron;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

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
                _ = WorkerLoop();
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

                master.WriteMultipleRegisters(1, receiveResult, data);
            } catch (Exception ex) {
                Console.WriteLine("WriteBatch error: " + ex.Message);
            }

        }
        public void WriteStringToRobot(string msg)//ghi thông báo kết quả từ PC đến robot (địa chỉ 5)
        {
            try {
                if (master == null) return;

                ushort[] data = msg
                    .Select(c => (ushort)(c - '0')) // 🔥 '1' → 1
                    .ToArray();

                master.WriteMultipleRegisters(1, receiveResult, data);

            }
            catch (Exception ex) {

                Console.WriteLine("WriteStringToRobot error: " + ex.Message + " Register: " + receiveResult + " Msg: " + msg);
            }

        }

        //private async Task TriggerNotice()
        //{

        //    while (isRunning)
        //    {
        //        try
        //        {
        //            //đọc từ robot xác nhận đến vị trí chụp
        //            ushort[] regs = master.ReadHoldingRegisters(1, 3, 1);
        //            bool trigger = regs[0] == 1;
        //            // 👇 chỉ ăn 1 lần khi 0 -> 1
        //            if (trigger)
        //            {
        //                // xử lý logic của bạn
        //                Trigger?.Invoke();
        //                // 🔥 reset về 0 NGAY
        //                master.WriteSingleRegister(1, 3, 0);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Modbus error: " + ex.Message);
        //        }
        //        await Task.Delay(50);
        //    }
        //}
        private async Task WorkerLoop()
        {
            while (isRunning)
            {
                try
                {
                    // Đọc gộp từ địa chỉ 3 (Trigger) và 4 (Tray Done) - Đọc 2 thanh ghi cùng lúc
                    // Slave ID mặc định là 1.
                    ushort[] triger = master.ReadHoldingRegisters(1, triggered, 1);
                    ushort[] tray =  master.ReadHoldingRegisters(1, trayDone, 1);

                    // 1. Xử lý Trigger chụp ảnh (Địa chỉ 3)
                    if (triger[0] == 1)
                    {
                        Trigger?.Invoke();
                        // Reset ngay lập tức để Robot biết PC đã nhận lệnh
                        master.WriteSingleRegister(1, triggered, 0);
                    }

                    // 2. Xử lý Hoàn thành Tray (Địa chỉ 4)
                    bool currentTrayStatus = (tray[0] == 1);
                    if (currentTrayStatus )
                    {

                        OnTrayCompleted?.Invoke();
                        // Reset ngay lập tức
                        master.WriteSingleRegister(1, trayDone, 0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Modbus Loop Error: " + ex.Message);
                    Disconnect();
                    // Nếu lỗi mất kết nối, có thể xử lý reconnect ở đây
                }

                // Nghỉ 50ms để Robot không bị quá tải và dành CPU cho việc xử lý ảnh
                await Task.Delay(50);
            }
        }
        //private async Task ReadLoop()
        //{
        //    while (isRunning)
        //    {
        //        try
        //        {
        //            //đọc từ robot xác nhận xong tray hiện tại
        //            ushort[] regs = master.ReadHoldingRegisters(1, 4, 1);
        //            bool trigger = regs[0] == 1;

        //            // 👇 chỉ ăn 1 lần khi 0 -> 1
        //            if (trigger && !lastTrigger)
        //            {
        //                Console.WriteLine("Tray DONE");

        //                // xử lý logic của bạn
        //                OnTrayCompleted?.Invoke();

        //                // 🔥 reset về 0 NGAY
        //                master.WriteSingleRegister(1, 4, 0);
        //            }

        //            lastTrigger = trigger;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Modbus error: " + ex.Message);
        //        }

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
        private void LogToBox(string message)
        {
            // Kiểm tra nếu gọi từ thread khác thì dùng Invoke
            //if (richTextBox2.InvokeRequired)
            //{
            //    richTextBox2.Invoke(new Action(() => LogToBox(message)));
            //    return;
            //}

            //// Giới hạn số dòng để không làm treo app nếu chạy lâu
            //if (richTextBox2.Lines.Length > 100) richTextBox2.Clear();

            //richTextBox2.SelectionStart = richTextBox2.TextLength;
            //richTextBox2.SelectionLength = 0;

            //string timestamp = DateTime.Now.ToString("HH:mm:ss");
            //richTextBox2.AppendText($"[{timestamp}] {message}{Environment.NewLine}");

            //// Tự động cuộn xuống dòng mới nhất
            //richTextBox2.ScrollToCaret();
        }
    }
}

