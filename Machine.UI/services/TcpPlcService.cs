using HslCommunication;
using HslCommunication.Profinet.Melsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Machine.UI.services
{
    public class TcpPlcService
    {
        MelsecMcNet melsecPLC;
        private string ip; 
        private int port;
        public TcpPlcService(string ipAddress, int port)
        {
            this.ip = ipAddress;
            this.port = port;
        }
        public TcpPlcService(){}
        public async Task<bool> Connect(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            MessageBox.Show("port = " + port + "\n" + "ip = " + ip);
            try
            {
                // Khởi tạo đối tượng
                this.melsecPLC = new MelsecMcNet(this.ip, this.port);

                // Gọi hàm kết nối và kiểm tra kết quả từ OperateResult
                OperateResult result = await Task.Run(() => this.melsecPLC.ConnectServer());

                if (result.IsSuccess)
                {
                    // Kết nối thành công
                    return true;
                }
                else
                {
                    // Kết nối thất bại (sai IP, sai Port hoặc PLC đang offline)
                    MessageBox.Show($"Kết nối thất bại: {result.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống khi kết nối PLC: " + ex.Message);
                return false;
            }
        }


        private async void ConnectAndRead()
        {
            // 1. Mở kết nối
            OperateResult connect = await Task.Run(() => melsecPLC.ConnectServer());

            if (connect.IsSuccess)
            {
                // 2. Đọc dữ liệu từ thanh ghi D100 (kiểu Short/Int16)
                OperateResult<short> readD100 = await Task.Run(() => melsecPLC.ReadInt16("D100")); 
                if (readD100.IsSuccess)
                {
                    short value = readD100.Content;
                    // Xử lý giá trị đọc được ở đây
                }

                // 3. Ghi dữ liệu xuống thanh ghi D102
                short valueToWrite = 123;
                OperateResult writeD102 = await Task.Run(() => melsecPLC.Write("D102", valueToWrite));

                if (writeD102.IsSuccess)
                {
                    // Ghi thành công
                }
            }
            else
            {
                // Thông báo lỗi kết nối
            }
        }
        // 1. Đọc Bit (Dạng bool) - Ví dụ: "M100", "X0", "Y0"
        public bool ReadBit(string address)
        {
            OperateResult<bool> result = melsecPLC.ReadBool(address);


            return result.IsSuccess ? result.Content : false;
        }

        // 2. Đọc Word (Dạng Int16/Short) - Ví dụ: "D100"
        public short ReadWord(string address)
        {
            OperateResult<short> result = melsecPLC.ReadInt16(address);
            return result.IsSuccess ? result.Content : (short)0;
        }

        // 3. Đọc Double Word (Dạng Int32) - Ví dụ: "D200" (chiếm 2 thanh ghi D200-D201)
        public int ReadDWord(string address)
        {
            OperateResult<int> result = melsecPLC.ReadInt32(address);
            return result.IsSuccess ? result.Content : 0;
        }

        // 4. Đọc số thực (Float/Real) - Thường dùng cho các cảm biến, thông số kỹ thuật
        public float ReadFloat(string address)
        {
            OperateResult<float> result = melsecPLC.ReadFloat(address);
            return result.IsSuccess ? result.Content : 0.0f;
        }
        // 1. Ghi Bit
        public bool WriteBit(string address, bool value)
        {
            return melsecPLC.Write(address, value).IsSuccess;
        }

        // 2. Ghi Word (Int16)
        public bool WriteWord(string address, short value)
        {
            return melsecPLC.Write(address, value).IsSuccess;
        }

        // 3. Ghi Double Word (Int32)
        public bool WriteDWord(string address, int value)
        {
            return melsecPLC.Write(address, value).IsSuccess;
        }
        // 4. Ghi Float
        public bool WriteFloat(string address, float value)
        {
            return melsecPLC.Write(address, value).IsSuccess;
        }
    }
}
