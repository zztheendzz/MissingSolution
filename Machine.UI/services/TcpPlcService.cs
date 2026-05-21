using HslCommunication;
using HslCommunication.Profinet.Melsec;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Machine.UI.services
{
    public class TcpPlcService
    {
        private MelsecMcNet melsecPLC;

        public bool IsConnected { get; private set; }

        public Action<string> OnStatus;
        public async Task<bool> Connect(string ip, int port)
        {
            try
            {
                melsecPLC = new MelsecMcNet(ip, port);

                melsecPLC.ConnectTimeOut = 3000;
                melsecPLC.ReceiveTimeOut = 3000;

                OperateResult result =
                    await Task.Run(() => melsecPLC.ConnectServer());

                IsConnected = result.IsSuccess;

                OnStatus?.Invoke($"PLC Connection: {(result.IsSuccess ? "Connected" : "Disconnected")}");

                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                {
                    IsConnected = false;
                    OnStatus?.Invoke("Err PLC Connection: Disconnected" + ex.ToString() );
                    return false;
                }
            }
        }

        private bool _isReadingM1002 = false;//thể hiện gắp hàng bị lỗi 

        private CancellationTokenSource _m1002Token;

        public Action<bool> OnM1002Changed;

        public async Task StartReadM1002()
        {
            if (_m1002Token != null)
                return;

            _m1002Token = new CancellationTokenSource();

            CancellationToken token = _m1002Token.Token;

            bool lastValue = false;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (IsConnected && melsecPLC != null)
                    {
                        var result = melsecPLC.ReadBool("M1002");

                        if (result.IsSuccess)
                        {
                            bool current = result.Content;

                            if (current != lastValue)
                            {
                                lastValue = current;

                                OnM1002Changed?.Invoke(current);
                            }
                        }
                        else
                        {
                            IsConnected = false;
                            OnStatus?.Invoke(result.Message);
                        }
                    }

                    await Task.Delay(100, token);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnStatus?.Invoke(ex.Message);
            }
            finally
            {
                _m1002Token = null;
            }
        }

        public void StopReadM1002()
        {
            _m1002Token?.Cancel();
            _m1002Token = null;
        }

        public void Disconnect()
        {
            try
            {
                melsecPLC?.ConnectClose();
                OnStatus?.Invoke("PLC Disconnected" );
            }
            catch { }

            IsConnected = false;
        }

        public short ReadWord(string address)
        {
            try
            {
                if (!IsConnected || melsecPLC == null)
                    return 0;

                var result = melsecPLC.ReadInt16(address);

                return result.IsSuccess ? result.Content : (short)0;
            }
            catch
            {
                IsConnected = false;
                return 0;
            }
        }

        public bool WriteWord(string address, short value)
        {
            try
            {
                if (!IsConnected || melsecPLC == null)
                    return false;

                return melsecPLC.Write(address, value).IsSuccess;
            }
            catch
            {
                IsConnected = false;
                return false;
            }
        }
        // Đọc bit PLC
        public bool ReadBit(string address)
        {
            try
            {
                if (melsecPLC == null)
                    return false;

                OperateResult<bool> result = melsecPLC.ReadBool(address);

                if (result.IsSuccess)
                {
                    return result.Content;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Ghi bit PLC
        public bool WriteBit(string address, bool value)
        {
            try
            {
                if (melsecPLC == null)
                    return false;

                OperateResult result = melsecPLC.Write(address, value);

                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }

}

