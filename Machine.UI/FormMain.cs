
using DocumentFormat.OpenXml.Bibliography;
using HslCommunication.Profinet.Melsec;
using Machine.UI.model;
using Machine.UI.popupForm;
using Machine.UI.services;
using Machine.UI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using VM.Core;
using VM.PlatformSDKCS;
namespace Machine.UI
{
    public partial class FormMain : Form
    {

        //port pc 8001, ip = xxxx.xxx.xx.81 
        MelsecMcServer plcServer = new MelsecMcServer();//plc
        ExportExcelService excelService;
        CameraService cam = new CameraService();
        bool isRunning = false;
        TcpClientService vision = new TcpClientService();//vision
        ModbusService robot = new ModbusService();//modbus robot : đọc ghi - xác định trạng thái
        serverService serverService = new serverService();// server cho robot - send data
        TcpPlcService plc = new TcpPlcService();
        SocketService socketService = new SocketService();
// gửi socket cho robot - cổng 8000
        int positions = 0;
        SummaryResultsService summaryResultsService;
        //chuỗi gửi từ camera -> app dạng : 1,1,1,1,1

        //string ipVision = "192.168.10.81";
        string ipVision = "192.168.10.81";
        string ipRobot = "192.168.10.80";
        string ipPlc = "192.168.10.10";

        TrayModel currentTray;
        TrayProcessor processor;
        public BackupDbService backupDbService = new BackupDbService();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        VisionDataService visionDb;
        TrayRunService trayRunService;
        List<VisionData> _trayBuffer = new List<VisionData>();
        TrayRun _currentTrayRun;
        bool _isInitializing = true;

        int portVision = 8001;
        int portRobot = 502;
        int portPlc = 5000;
        int portSocketRobot = 8005;

        int currentTrayId; // xác định xem tray nào đang chạy để insert vào db
        int _previousIndex = -1; // biến xác định ng dùng có đổi tray mới k - cacche tray
        bool OnOffpick = false;

        int total = 0; // biến cục bộ hiển thị tổng số hàng đã quét
        int ok = 0;// biến cục bộ hiển thị tổng số hàng ok
        int ng = 0;// biến cục bộ hiển thị tổng số hàng ng
        int none = 0;// biến cục bộ hiển thị tổng số hàng none
         
        public bool checkCycle = true;
        PopupWarning popup ;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        VmProcedure _flow;
        string _lastTcpMsg = null;
        bool _isVisionRunning = false;

        List<Model1> trays;
        public FormMain()
        {
            InitializeComponent();

            //========giả lập server plc để test robot, nếu muốn test thật thì comment dòng này đi=========

            // plcServer.ServerStart(6000);
            //========end giả lập server plc để test robot, nếu muốn test thật thì comment dòng này đi=====
            backupDbService.CheckAndBackup();
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            string conn = configDB.configDB.ConnectionString;
            excelService = new ExportExcelService(conn);
            visionDb = new VisionDataService(conn);

            trayRunService = new TrayRunService(conn);
            summaryResultsService = new SummaryResultsService(conn);
            TimeReal();
            this.FormBorderStyle = FormBorderStyle.None;
            tableLayoutPanel1.MouseDown += tableLayoutPanel1_MouseDown;// kéo thả - di chuyển vị trí trên màn hình


        }

        public async void socketConnect(string ip, int port)
        {
            await socketService.Connect(ip, port);

        }


        private async void FormMain_Load(object sender, EventArgs e)
        {
            flowLayoutPanel4.Visible = false;//ẩn menu ở setting
            flowLayoutPanel6.Visible = false;//ẩn menu ở data

            string path = Path.Combine(Application.StartupPath, "configDB", "models.json");//path file json các loại tray
            string json = File.ReadAllText(path);
            trays = JsonSerializer.Deserialize<List<Model1>>(json);// lấy ds tray từ file json

            //  xử lý sự kiện nhận index từ robot ??? cần thiết ??
            robot.OnIndexReceived = (index) =>
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox2, $" Index: {index}");
                }));
            };
            serverService.OnReceive = (msg) =>
            {
                this.Invoke(new Action(async () =>
                {

                    switch (msg)
                    {
                        case "0":
                         // _flow?.Run();
                            AppendLog(richTextBox2, "run vision, trig= " + msg);
                            break;
                        case "1":
                            // _flow?.Run();
                            if (checkCycle)
                            {
                                timer2.Start();
                                Cycle = 0;
                                //  checkCycle = false;
                            }
                            

                            plc.WriteBit("M1003", false);
                            popup?.Close();
                           popup=null;
                            cleanData();
                            AppendLog(richTextBox2, "Start tray, msg=" + msg);
                            AppendLog(richTextBox3, "Start tray, msg=" + msg);
                            ClearTray();// 🔥 reset UI
                            processor.Reset(); // 🔥 reset data ở code
                            StartTray();
                            plc.StopReadM1002();
                            break;

                        case "2":
                            // _flow?.Run();
                            
                            timer2.Stop();
                           // Cycle = 0;
                            CycleTime.Text = Cycle.ToString();
                            _currentTrayRun.EndTime = DateTime.Now;
                            currentTrayId = trayRunService.Create(_currentTrayRun);

                            foreach (var item in _trayBuffer)
                            {
                                item.TrayId = currentTrayId;
                            }
                            if (_trayBuffer.Count > 0)
                            {
                                visionDb.InsertBatch(_trayBuffer);
                                AppendLog(richTextBox2, $"💾 Saved {_trayBuffer.Count} records");
                                _trayBuffer.Clear();
                            }
                            await plc.StartReadM1002();

                            // trayRunService.UpdateEndTime(currentTrayId);// thêm thời gian kết thúc tray vào db
                            AppendLog(richTextBox2, "End Tray, trig=" + msg);
                            AppendLog(richTextBox3, "End Tray, trig=" + msg);
                            break;

                        default:
                            AppendLog(richTextBox2, "data từ robot sai hoặc k nhận được, msg = " + msg);
                            break;
                    }
                }));
            };

            formModel.ResetListModel += () =>
            {

                string path1 = Path.Combine(
                    Application.StartupPath,
                    "configDB",
                    "models.json");

                string jsonReload = File.ReadAllText(path1);

                trays = JsonSerializer.Deserialize<List<Model1>>(jsonReload)
                         ?? new List<Model1>();

                InitComboBox();

                AppendLog(richTextBox2, "Reload model list");
            };

            //sự kiện xử lý lỗi err pickup
            plc.OnM1002Changed += async (value) =>
            {
                // chỉ xử lý khi M1002 = 1
                if (!value)
                {
                    plc.WriteBit("M1003", false);
                    return;
                }

                this.BeginInvoke(new Action(() =>
                {
                    if (popup == null || popup.IsDisposed)
                    {
                        popup = new PopupWarning("Lỗi gắp hàng");

                        popup.FormClosed += (_, args) =>
                        {
                            plc.WriteBit("M1003", true);
                        };

                        popup.Show();

                    }
                }));
            };

            // sự kiên dc gọi khi nhận dc data từ vision, parse ra kết quả, gửi cho robot, update UI, insert vào db
            vision.OnRawData = (msg) =>
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox3, "msg results= " + msg);
                    var results = VisionParser.Parse(msg); //nhận dữ liệu từ vision
                    ////////////////////////////////////////////////////, parse ra kết quả, update UI, insert vào db
                    string appendTextRs = $"[{DateTime.Now:HH:mm:ss}] ";
                    foreach (var item in results)
                    {
                        appendTextRs += item + "\t";
                    }
                    AppendLog(richTextBox3, appendTextRs);
                    var cells = processor.ProcessBatch(results);

                    InsertData(cells);
                    foreach (var cell in cells)
                    {
                        if (!string.IsNullOrEmpty(cell.Result))
                        {
                            UpdateTray(cell.Row, cell.Col, cell.Result);
                        }
                    }
                    Console.WriteLine("sap gui data");
                    string result = string.Join(",", msg.ToCharArray());
                    result = result + ",";
                    Console.WriteLine("vua gui data");
                    serverService.SendToRobot(result);//gửi data cho robot
                    Console.WriteLine("vua gui data cho robot result = " + result);
                    ////////////////////////////////////////////////////
                }));
            };
            serverService.OnStatus= (status) =>
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox2, $"Robot: {status}");
                }));
            };
            plc.OnStatus = (status) =>
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox2, $"PLC: {status}");
                }));
            };
            serverService.OnClientDisconnected += () =>
            {
               // ClearTray();// 🔥 reset UI
              //  processor.Reset(); // 🔥 reset data ở code
              //  StartTray();
                Console.WriteLine("Robot Disconnected");
                AppendLog(richTextBox2, "Robot Disconnected");
            };

            robot.OnTrayCompleted = () =>
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox2, "✅ Tray DONE");

                    trayRunService.UpdateEndTime(currentTrayId);// thêm thời gian kết thúc tray vào db
                    // 🔥 reset UI
                    ClearTray();
                    // 🔥 reset data ở code
                    processor.Reset();
                }));
            };

            InitComboBox();
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            //LoadTray(comboBox1.SelectedIndex);// load tray đầu tiên 0 - tray đầu tiên của combobox
            //_previousIndex = 0;
            //comboBox1.SelectedIndex = 0;
            comboBox1.BringToFront();
            comboBox1.Dock = DockStyle.None;
            _isInitializing = false;
            await Task.WhenAll(
          );
            //  await vision.Connect(ipVision, portVision);
            AppendLog(richTextBox2, "🔌 Connecting modbus robot...");
            await robot.Connect(ipRobot, portRobot);
            AppendLog(richTextBox2, "🔌 open server for robot...");
            await serverService.Start(8005);
            AppendLog(richTextBox2, "🔌 Connecting Plc...");
            var ok = plc.Connect(ipPlc, portPlc);

            if (!await ok)
            {
                AppendLog(richTextBox2, "❌ Không kết nối được Plc");
            }
            else
            {
                AppendLog(richTextBox2, "🔌 Connected Plc");
            }
            OnOffpick = plc.ReadBit("M1001");
            UpdatePickupButton();
        }
        async Task<bool> ConnectVisionSafe()
        {
            try
            {
                //richTextBox2.AppendText("🔌 Connecting Vision...\n");
                AppendLog(richTextBox2, "🔌 Connecting Vision...");
                var connectTask = vision.Connect(ipVision, portVision);

                // ⏱ timeout 3s (tránh treo)
                if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
                {
                    throw new TimeoutException("Vision connect timeout");
                }

                await connectTask;

                //   richTextBox2.AppendText("✅ Vision Connected\n");
                AppendLog(richTextBox2, "✅ Vision Connected");
                return true;
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, $"❌ Vision Error: {ex.Message}");
                return false;
            }
        }
        public void InsertData(List<Machine.UI.model.Cell> cells)
        {
            var list = new List<VisionData>();

            foreach (var cell in cells)
            {
                if (!string.IsNullOrEmpty(cell.Result))
                {
                    total++;

                    if (cell.Result == "OK")
                        ok++;
                    else if (cell.Result == "NG")
                        ng++;
                    else

                        none++;

                    list.Add(new VisionData
                    {
                        TrayId = currentTrayId,
                        Row = cell.Row,
                        Col = cell.Col,
                        Result = cell.Result == "OK" ? 1 : cell.Result == "NG" ? 0 : 2,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // 🔥 UPDATE UI ĐÚNG THREAD
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateUI));
            }
            else
            {
                UpdateUI();
            }

            if (list.Count > 0)
            {
                _trayBuffer.AddRange(list);
            }
        }
        void UpdateUI()
        {


            labelOk.Text = ok.ToString();
            labelNg.Text = ng.ToString();
            labelNone.Text = none.ToString();
            labelTotal.Text = total.ToString();

            labelNgPer.Text = ((double)ng / total * 100).ToString("F2") + "%";
            labelNonePer.Text = ((double)none / total * 100).ToString("F2") + "%";
            labelOkPer.Text = (100
              - (double)ng / total * 100
              - (double)none / total * 100).ToString("F2") + "%";
        }
        // ================== COMBOBOX ==================
        private void InitComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            for (int i = 0; i < trays.Count; i++)
            {
                comboBox1.Items.Add($"{trays[i].Index}:Tray {trays[i].Name}");
            }

            // chọn tray đầu tiên
            comboBox1.SelectedIndex = -1;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;
            if (_isInitializing) return; // 🔥 chặn lần đầu
            string trayName = trays[comboBox1.SelectedIndex].Name;

            var result = MessageBox.Show(
              $"Bạn có chắc chắn đổi sang tray {trayName} không?",
              "Xác nhận đổi tray",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                // 🔥 rollback về giá trị cũ
                comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
                comboBox1.SelectedIndex = _previousIndex;
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

                AppendLog(richTextBox2, "lựa chon tray " + trayName);
             //   robot.sendTrayToRobot((ushort)trays[comboBox1.SelectedIndex].Index);

                return;
            }

            // ✅ user đồng ý → update index cũ
            _previousIndex = comboBox1.SelectedIndex;

            // 🔥 reset processor
            processor?.Reset();

            // 🔥 clear UI
            ClearTray();
            int currenTray = trays[comboBox1.SelectedIndex].Index;
            checkV = true;

            AppendLog(richTextBox2, "lựa chon tray " + trayName);
            robot.sendTrayToRobot((ushort)trays[comboBox1.SelectedIndex].Index);
            vision.modeData = trays[comboBox1.SelectedIndex].VisionCount;

            AppendLog(richTextBox2, "send tray " + trayName + " to robot");
            AppendLog(richTextBox2, "current tray index= " + currenTray);
            AppendLog(richTextBox2, "current tray name= " + trayName);
            // 🔥 load tray mới
           LoadTray(comboBox1.SelectedIndex);
            robot.sendTrayToRobot((ushort)trays[comboBox1.SelectedIndex].Index);
            AppendLog(richTextBox2, "lựa chon tray " + trayName);
            processor.batchSize = trays[comboBox1.SelectedIndex].VisionCount;
        }

        // ================== LOAD TRAY ==================
        bool checkV = false;
        private async void LoadTray(int index)
        {
            if (index < 0 || index >= trays.Count) return;

            var model = trays[index];

            currentTray = model.ToTrayModel();//tray hiện tại

            processor = new TrayProcessor(currentTray);
            processor.Reset();//

            //  //lưu tray chuẩn bị chạy vào db

            //===========start render các ô ở gridviewtable thành ô===========================
            int rows = model.Row;
            int cols = model.Col;

            DataTable table = new DataTable();

            for (int j = 0; j < cols; j++)
                table.Columns.Add("C" + (j + 1));

            for (int i = 0; i < rows; i++)
            {
                DataRow row = table.NewRow();
                for (int j = 0; j < cols; j++)
                    row[j] = "";

                table.Rows.Add(row);
            }
            //===========end===========================

            dataGridView1.DataSource = table;
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].DisplayIndex =
                    dataGridView1.Columns.Count - 1 - i;
            }
            SetupGridStyle();

            // ===== 3. LOAD VISION (🔥 QUAN TRỌNG) =====
            if (!File.Exists(model.ProgramVision))
            {
                AppendLog(richTextBox2, $"📂 Path: {model.ProgramVision}");
                AppendLog(richTextBox2, "❌File does not exist - recheck the path.");
                return;
            }

            try
            {
                AppendLog(richTextBox2, $"📂 Load: {model.ProgramVision}");
                string dir = Path.GetDirectoryName(model.ProgramVision);
                Directory.SetCurrentDirectory(dir);
                if (_cts.Token.IsCancellationRequested)
                    return;

                    try
                    {
                        AppendLog(richTextBox2, "⏳ Đang nạp chương trình Vision...");

                        // 1. Dừng các luồng đang chạy nếu có (Tránh xung đột memory)
                        // Bạn có thể thêm lệnh dừng ContinuousRun tại đây nếu cần
                       // vision.Disconnect();

                        await Task.Delay(1000);
                        await Task.Run(() =>
                        {
                            System.Threading.Thread.Sleep(100);
                            //khi lỗi đổi chương trình thì tăng thời gian sleep lên và bỏ comment  dòng dưới để dừng hoàn toàn vision, sau đó load lại chương trình mới

                            //  VmSolution.Instance.ContinuousRunEnable = false
                            //    VmSolution.Instance.DisableModulesCallback();
                            // 2. Đóng solution cũ
                            //   VmSolution.Instance.CloseSolution();

                            // 3. Load solution mới
                            // Tham số: (Đường dẫn, Mật khẩu, Ghi đè Global)
                            VmSolution.Load(model.ProgramVision, "", false);
                        });

                        AppendLog(richTextBox2, "✅ Load thành công: " + Path.GetFileName(model.ProgramVision));
                    }
                    catch (Exception ex)
                    {
                        // Lấy thông tin lỗi chi tiết từ VisionMaster
                        var vmEx = VmSolution.GetVmException(ex);
                        string errorMsg = vmEx != null ? $"[{vmEx.errorCode:X}] {vmEx.Message}" : ex.Message;

                        AppendLog(richTextBox2, "❌ Load fail: " + errorMsg);
                    }
                

                if (VmSolution.Instance == null)
                {
                    AppendLog(richTextBox2, "❌ Load fail");
                    return;
                }

                _flow = VmSolution.Instance["Flow1"] as VmProcedure;

                if (_flow == null)
                {
                    AppendLog(richTextBox2, "❌ Không có Flow1");
                    return;
                }

                vmRenderControl1.ModuleSource = _flow;

                _flow.OnWorkEndStatusCallBack -= OnVisionDone;
                _flow.OnWorkEndStatusCallBack += OnVisionDone;
                //  _flow.Run();




                AppendLog(richTextBox2, $"✅ Vision OK: {model.Name}");

                // connect vision sau khi load
                bool connected = await ConnectVisionSafe();

                if (!connected)
                {
                    AppendLog(richTextBox2, "❌ Vision connect fail after load");
                    return;
                }

                //check kết nối vision, nếu không kết nối được thì log ra richtextbox2
                _ = Task.Run(async () =>
                {
                    if (_cts.Token.IsCancellationRequested)
                        return;
  
                }
        );
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, "❌ " + ex.ToString());
            }

        }
        private void OnVisionDone(object sender, EventArgs e)
        {
            var proc = sender as VmProcedure;

            this.Invoke(new Action(() =>
            {
                AppendLog(richTextBox2, "🔥 Vision Done");
            }));

            if (proc?.ModuResult == null)
            {
                this.Invoke(new Action(() =>
                {
                    AppendLog(richTextBox2, "❌ ModuResult NULL");
                }));

                _isVisionRunning = false;
                return;
            }
            // =========================
            // 🔥 2. DEBUG OUTPUT VM (nếu cần)
            // =========================
            var outputs = proc.ModuResult.GetAllOutputNameInfo();

            this.Invoke(new Action(() =>
            {
                AppendLog(richTextBox2, $"Output count = {outputs?.Count}");
            }));

            // =========================
            // 🔥 DONE → unlock
            // =========================
            _isVisionRunning = false;
        }

        // ================== STYLE  datagirdtable==================
        private void SetupGridStyle()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;
            this.BeginInvoke(new Action(() =>
            {
                dataGridView1.ClearSelection();
                dataGridView1.CurrentCell = null;
            }));
            dataGridView1.SelectionChanged += (s, e) =>
            {

                dataGridView1.CurrentCell = null;
                dataGridView1.ClearSelection();
            };
            dataGridView1.ScrollBars = ScrollBars.None; // 🔥 quan trọng để không lệch

            int rows = dataGridView1.Rows.Count;
            int cols = dataGridView1.Columns.Count;

            if (rows == 0 || cols == 0) return;

            int totalWidth = dataGridView1.ClientSize.Width;
            int totalHeight = dataGridView1.ClientSize.Height;

            int colWidth = totalWidth / cols;
            int rowHeight = totalHeight / rows;

            // set width
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.Width = colWidth;
            }

            // set height
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = rowHeight;
            }
        }
        private void SetupGridStyle1()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;

            dataGridView1.ScrollBars = ScrollBars.None;

            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            int rows = dataGridView1.Rows.Count;
            int cols = dataGridView1.Columns.Count;

            if (rows == 0 || cols == 0) return;

            // 🔥 trừ border
            int totalWidth = dataGridView1.ClientSize.Width - 2;
            int totalHeight = dataGridView1.ClientSize.Height - 2;

            int colWidth = totalWidth / cols;
            int rowHeight = totalHeight / rows;

            // set width
            for (int i = 0; i < cols; i++)
            {
                dataGridView1.Columns[i].Width = colWidth;
            }

            // 🔥 cột cuối fill phần dư
            dataGridView1.Columns[cols - 1].Width =
                totalWidth - (colWidth * (cols - 1));

            // set height
            for (int i = 0; i < rows; i++)
            {
                dataGridView1.Rows[i].Height = rowHeight;
            }

            // 🔥 hàng cuối fill phần dư
            dataGridView1.Rows[rows - 1].Height =
                totalHeight - (rowHeight * (rows - 1));
        }
        private void FormMain_Resize(object sender, EventArgs e)
        {
            SetupGridStyle1();
        }
        // ================== UPDATE kết quả chụp, hiển thị lên datagridview==================
        private void UpdateTray(int row, int col, string result)
        {
            if (row < 0 || col < 0) return;
            if (row >= dataGridView1.Rows.Count) return;
            if (col >= dataGridView1.Columns.Count) return;

            string text;

            if (result == "EMPTY")
                text = $"EMPTY";
            else
                text = $"{result}";
            string location = "("+row.ToString()+","+col.ToString()+")";
            text = location + text;
            dataGridView1.Rows[row].Cells[col].Value   = text;

            dataGridView1.Rows[row].Cells[col].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.InvalidateCell(col, row);
        }
        private void ClearTray()//
        {
            richTextBox3.Clear();
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(ClearTray));
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = ""; // reset về rỗng
                }
            }
        }

        void StartTray()        //lưu tray chuẩn bị chạy vào db
        {
            _trayBuffer.Clear();
            if (comboBox1.SelectedIndex < 0)
            {
                AppendLog(richTextBox2, "chưa chọn tray");
                return;
            }
            var model = trays[comboBox1.SelectedIndex];

            if (model.VisionCount == 5)
            {
                vision.SetMode(ReceiveMode.FiveField);
            }
            else
            {
                vision.SetMode(ReceiveMode.FourField);
            }
            _currentTrayRun = new TrayRun
            {
                TrayName = model.Name,
                Row = model.Row,
                Col = model.Col,
                StartTime = DateTime.Now
            };

            //currentTrayId = trayRunService.Create(new TrayRun
            //{
            //    TrayName = model.Name,
            //    Row = model.Row,
            //    Col = model.Col,
            //    StartTime = DateTime.Now
            //});
        }
        // ================== COLOR cho từng cell khi nhận kết quả ==================
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;

            string val = e.Value.ToString();

            if (val.Contains("OK"))
            {
                e.CellStyle.BackColor = System.Drawing.Color.LimeGreen;
            }
            else if (val.Contains("NG"))
            {
                e.CellStyle.BackColor = System.Drawing.Color.Red;
                e.CellStyle.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                e.CellStyle.BackColor = System.Drawing.Color.LightGray;
            }
        }

        // ================== UI EVENTS (GIỮ NGUYÊN) ==================

        private void button6_Click(object sender, EventArgs e)
        {
            if (cam.InitCamera())
            {     //cam.Start(pictureBox1);
            }
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            flowLayoutPanel4.Visible = !flowLayoutPanel4.Visible;
        }

        private async void ConnectPlc_Click(object sender, EventArgs e)
        {
            AppendLog(richTextBox2, "🔌 Connecting Plc...");
            var ok = plc.Connect(ipPlc, portPlc);

            if (!await ok)
            {
                AppendLog(richTextBox2, "❌ Không kết nối được Plc");
            }
            else
            {
                AppendLog(richTextBox2, "🔌 Connected Plc");
            }
        }




        //kết nối robot
        
        private async void button8_Click(object sender, EventArgs e)
        {
            AppendLog(richTextBox2, "🔌 Connecting...");
            var ok = robot.Connect(ipRobot, portRobot);

            if (!await ok)
            {
                AppendLog(richTextBox2, "❌ Không kết nối được Modbus");
            }
            else
            {
                AppendLog(richTextBox2, "🔌 Connected");
            }

        }
        // ngắt kết nối robot
        private void button9_Click(object sender, EventArgs e)
        {
            robot.Disconnect();
            AppendLog(richTextBox2, "🔌 Disconnected");
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {
            //showdata vision
            // richTextBox1.AppendText(results + "\n");
        }
        // kết nối vision
        private async void button10_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox2.AppendText("🔌 Connecting Vision...\n");
                await vision.Connect(ipVision, portVision);
                AppendLog(richTextBox2, "✅ Vision Connected");
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, "❌ Vision Connect Error: " + ex.Message);
            }
        }
        // ngắt kết nối vision
        private void button11_Click(object sender, EventArgs e)
        {
            //disconnect vision
            vision.Disconnect();
            AppendLog(richTextBox2, "🔌 Vision Disconnected");
        }

        // hiện popup tổng hợp kết quả chạy máy từ DB theo thời gian
        private void SummaryResult_Click(object sender, EventArgs e)
        {
            DateTime from = dateTimePickerFrom.Value;
            DateTime to = dateTimePickerTo.Value;
            string safeFrom = from.ToString("yyyyMMdd_HHmmss");
            string safeTo = to.ToString("yyyyMMdd_HHmmss");
            try
            {
                var data = summaryResultsService.GetSummaryByModel(from, to);
                var f = new FormSummary(data);
                f.ShowDialog(); // popup modal
                AppendLog(richTextBox2, "Get SummaryResultsService done : ");
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, "❌ Lỗi SummaryResultsService : " + ex.ToString());
            }

        }
        //xuất excel tất cả tray theo thời gian từ DB
        private void button12_Click(object sender, EventArgs e)
        {
            DateTime from = dateTimePickerFrom.Value;
            DateTime to = dateTimePickerTo.Value;
            string safeFrom = from.ToString("yyyyMMdd_HHmmss");
            string safeTo = to.ToString("yyyyMMdd_HHmmss");
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files|*.xlsx";
                sfd.FileName = $"all_tray_{safeFrom}_{safeTo}";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        excelService.ExportFlatData(sfd.FileName, from, to);

                        AppendLog(richTextBox2, "✅ Export ALL tray thành công");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi export" + ex.Message);
                    }
                }
            }
        }

        //chạy chương trình vision
        private void button2_Click(object sender, EventArgs e)
        {
            var flow = VmSolution.Instance["Flow1"] as VmProcedure;
            try
            {
                flow?.Run();
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, "ERR run vision " + ex.ToString());
            }

        }

        // ẩn hiện menu data
        private void btnData_Click(object sender, EventArgs e)
        {
          //  flowLayoutPanel6.Visible = !flowLayoutPanel6.Visible; //hiển thị menu
            BackupViewerForm f = new BackupViewerForm();

            f.Show();
        }

        //chặn selection cell khi click vào datagridview
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private async void btnMenu_Click(object sender, EventArgs e) { }
        private void pnResult_Paint(object sender, PaintEventArgs e) { }

        //đặt lại biến cục bộ hiển thị tổng số hàng đã quét, ok, ng, none về 0 và update lại label
        private void button1_Click_1(object sender, EventArgs e)
        {
            //Clear data
            total = 0;
            ok = 0;
            ng = 0;
            none = 0;
            labelOk.Text = "0";
            labelOkPer.Text = "0%";


            labelNg.Text = "0";
            labelNgPer.Text = "0%";

            labelNone.Text = "0";
            labelNonePer.Text = "0%";

            labelTotal.Text = "0";
        }
        public void cleanData()
        {
            //Clear data
            total = 0;
            ok = 0;
            ng = 0;
            none = 0;
            labelOk.Text = "0";
            labelOkPer.Text = "0%";


            labelNg.Text = "0";
            labelNgPer.Text = "0%";

            labelNone.Text = "0";
            labelNonePer.Text = "0%";

            labelTotal.Text = "0";
        }


        public void clearData()
        {
            total = 0;
            ok = 0;
            ng = 0;
            none = 0;
            labelOk.Text = "0";
            labelOkPer.Text = "0%";
        }


            //add log vào richtextbox
            void AppendLog(RichTextBox rtb, string text)
        {
            text = text + "\n";
            if (rtb.InvokeRequired)
            {
                rtb.Invoke(new Action(() => AppendLog(rtb, text)));
                return;
            }

            // Check xem đang ở cuối không
            bool isAtBottom = rtb.SelectionStart == rtb.TextLength;

            rtb.AppendText(text + Environment.NewLine);

            // Nếu đang ở cuối thì mới auto scroll
            if (isAtBottom)
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.ScrollToCaret();
            }
            if (rtb.Lines.Length > 1000)
            {
                rtb.Clear();
            }
        }
        //test ghi plc d100
        private async void WritePlc_Click(object sender, EventArgs e)
        {
            string address = "D100";
            short value = 123;

            // Gọi hàm WriteWord bạn đã viết
            bool isSuccess = await Task.Run(() => plc.WriteWord(address, value));

            if (isSuccess)
            {
                AppendLog(richTextBox2, $"✅ Ghi thành công {value} vào {address}  với {value}");
            }
            else
            {
                AppendLog(richTextBox2, $"❌ Ghi thất bại {value} vào {address}  với {value}");
            }
            writeBit();
            WriteFloat();
        }
        //test đọc plc d100
        private async void ReadPlc_Click(object sender, EventArgs e)
        {
            string address = "D100";

            // Gọi hàm ReadWord bạn đã viết
            short result = await Task.Run(() => plc.ReadWord(address));
            AppendLog(richTextBox2, $"📥 Đọc thành công {result} từ {address}  với {result}");
            // Hiển thị kết quả ra Label hoặc TextBox
            readBit();
            ReadFloat();
        }
        public async void readBit()
        {
            string address = "M100";
            bool value = true;

            // Thư viện sẽ tự động ghi vào D100 và D101
            bool success = await Task.Run(() => plc.ReadBit(address));

            AppendLog(richTextBox2, $"📥 Đọc thành công {success} từ {address}  với {value}");
        }

        public async void writeBit()
        {
            string address = "M100";
            bool value = true;

            // Thư viện sẽ tự động ghi vào D100 và D101
            bool success = await Task.Run(() => plc.WriteBit(address, value));

            AppendLog(richTextBox2, $"📥 Ghi thành công {success} từ {address}  với {value}");
        }
        // --- GHI SỐ THỰC (Float/Real) ---
        private async void WriteFloat()
        {
            string address = "D300";


            // Thư viện sẽ tự động ghi vào D100 và D101
         //   bool success = await Task.Run(() => plc.WriteFloat(address, value));

         //   AppendLog(richTextBox2, $"📥 Ghi float thành công {success} từ {address}  với {value}");
        }

        private async void ReadFloat()
        {
            string address = "D300";
 

            // Thư viện sẽ tự động ghi vào D100 và D101
           // bool success = await Task.Run(() => plc.WriteFloat(address, value));

        //    AppendLog(richTextBox2, $"📥 Đọc float thành công {success} từ {address} với {value}");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _cts.Cancel();

                timer1?.Stop();
                timer1?.Dispose();

                vision?.Disconnect();

                robot?.Disconnect();

               // plc?.Disconnect();

                if (_flow != null)
                {
                    _flow.OnWorkEndStatusCallBack -= OnVisionDone;
                }

                if (VmSolution.Instance != null)
                {
                    VmSolution.Instance.Dispose();
                }

                plcServer.ServerClose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            base.OnFormClosing(e);
        }

        private async void ConnectSocketRb_Click(object sender, EventArgs e)
        {
            //string ip = "192.168.10.80";
            //int port = 8005;
           // socketConnect(ipRobot, portSocketRobot);
        }

        private async void sendSocket_Click(object sender, EventArgs e)
        {
        // await   socketService.Send("00000\r\n");
        }
        //bật tắt đèn - camera
        bool light = true;
        private void OnOffLight_Click(object sender, EventArgs e)
        {
            if (light) {
                plc.WriteBit("M1000", true);
                light = false;
            }
            else {
                plc.WriteBit("M1000", false);
                light = true; 
            }

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            lblHour.Text = now.ToString("HH:mm:ss");
            lblDayofY.Text = now.ToString("dd/MM/yyyy");
            lblDayofW.Text = now.ToString("dddd",
              new System.Globalization.CultureInfo("en-US"));
        }

        private void TimeReal()
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            timer1.Start();
        }
        private async void sendserversk_Click(object sender, EventArgs e)
        {
            byte a = 1;
        //  await  serverService.Send("1\r\n");

        }

        private async void serverc_Click(object sender, EventArgs e)
        {
            AppendLog(richTextBox2, "open server");

             serverService.Start(8005);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void CloseApp_Click(object sender, EventArgs e)
        {

            Application.Exit();
            vision.CancelToken();

        }

        private void token_Click(object sender, EventArgs e)
        {
            vision.CancelToken();
        }

        private void ExtendApp_Click(object sender, EventArgs e)
        {;

            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                SetupGridStyle();
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                SetupGridStyle();
            }
        }
        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private void lblMachineName_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lblHour_Click_1(object sender, EventArgs e)
        {

        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)                                                
        {

        }

        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void vmRenderControl1_Load(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void OnOffPickUp_Click(object sender, EventArgs e)
        {
            // đảo trạng thái
            OnOffpick = !OnOffpick;

            // ghi PLC
            plc.WriteBit("M1001", OnOffpick);

            // cập nhật UI
            UpdatePickupButton();
        }
        private void UpdatePickupButton()
        {
            if (OnOffpick)
            {
                // TRUE = OFF
                OnOffPickUp.BackColor = Color.Red;
                OnOffPickUp.Text = "StatusPickup: OFF";
            }
            else
            {
                // FALSE = ON
                OnOffPickUp.BackColor = Color.LimeGreen;
                OnOffPickUp.Text = "StatusPickup: ON";
            }
        }
        BackupViewerForm f;
        private void GetBackUpDb_Click(object sender, EventArgs e)
        {
            BackupViewerForm f =
                new BackupViewerForm();

            f.ShowDialog();

        }
        public int Cycle=0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            Cycle++;
            CycleTime.Text = Cycle.ToString();
        }
        ModelForm formModel = new ModelForm();
        private void Model_Click(object sender, EventArgs e)
        {
            formModel.ShowDialog();
        }
    }
}