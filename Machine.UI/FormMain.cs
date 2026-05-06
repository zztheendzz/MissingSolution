
using Machine.UI.model;
using Machine.UI.popupForm;
using Machine.UI.services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using VM.Core;


namespace Machine.UI
{
    public partial class FormMain : Form
    {
        ExportExcelService excelService;
        CameraService cam = new CameraService();
        bool isRunning = false;
        TcpClientService vision = new TcpClientService();
        ModbusService robot = new ModbusService();
        int positions = 0;
        SummaryResultsService summaryResultsService;
        //chuỗi gửi từ camera -> app dạng : 1,1,1,1,1

        //string ipVision = "192.168.0.211";
        string ipVision = "127.0.0.1";
        string ipRobot = "127.0.0.1";
        TrayModel currentTray;
        TrayProcessor processor;
        ushort registerData = 5;

        VisionDataService visionDb;
        TrayRunService trayRunService;

        bool _isInitializing = true;

        int portVision = 8001;
        int portRobot = 502;

        int currentTrayId; // xác định xem tray nào đang chạy để insert vào db
        int _previousIndex = -1; // biến xác định ng dùng có đổi tray mới k - cacche tray

        int total = 0; // biến cục bộ hiển thị tổng số hàng đã quét
        int ok = 0;// biến cục bộ hiển thị tổng số hàng ok
        int ng = 0;// biến cục bộ hiển thị tổng số hàng ng
        int none = 0;// biến cục bộ hiển thị tổng số hàng none

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

            // sự kiên dc gọi khi nhận dc data từ vision, parse ra kết quả, gửi cho robot, update UI, insert vào db
            vision.OnRawData = (msg) =>
            {
                this.Invoke(new Action(() =>
                {

                    var results = VisionParser.Parse(msg); //nhận dữ liệu từ vision
                    robot.WriteStringToRobot(registerData, msg);//gửi cho robot

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
                    ////////////////////////////////////////////////////
                }));
            };

            await Task.WhenAll(
      // robot.Connect(ipRobot, portRobot),
      //vision.Connect(ipVision, portVision)
                      );

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
            LoadTray(0);// load tray đầu tiên 0 - tray đầu tiên của combobox
            _previousIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox1.BringToFront();
            comboBox1.Dock = DockStyle.None;
            _isInitializing = false;
        }
        public void LoadComboBox1()
        {
            comboBox1.Items.Clear();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < trays.Count; i++)
            {
                comboBox1.Items.Add($"Tray {trays[i].Name}");
            }

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
                        Result = cell.Result == "OK" ? 1 : 0,
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
                visionDb.InsertBatch(list);
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
                comboBox1.Items.Add($"Tray {trays[i].Name}");
            }

            comboBox1.SelectedIndex = 1;
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
                return;
            }

            // ✅ user đồng ý → update index cũ
            _previousIndex = comboBox1.SelectedIndex;

            // 🔥 reset processor
            processor?.Reset();

            // 🔥 clear UI
            ClearTray();

            // 🔥 load tray mới
            LoadTray(comboBox1.SelectedIndex);
        }

        // ================== LOAD TRAY ==================
        private void LoadTray(int index)
        {
            if (index < 0 || index >= trays.Count) return;

            var model = trays[index];

            currentTray = model.ToTrayModel();//tray hiện tại

            processor = new TrayProcessor(currentTray);
            processor.Reset();//

            StartTray();  //lưu tray chuẩn bị chạy vào db

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
                try
                {
                    if (VmSolution.Instance != null)
                    {
                        //app crash???
                        //   VmSolution.Instance.Dispose();
                    }

                    VmSolution.Load(model.ProgramVision, "", false);//load chương trình vision
                    AppendLog(richTextBox2, "Load ProgramVision OK");
                }
                catch (Exception e)
                {
                    AppendLog(richTextBox2, "❌ Load fail\n" + e.ToString());
                }

                if (VmSolution.Instance == null)
                {
                    AppendLog(richTextBox2, "❌ Load fail");
                    return;
                }

                _flow = VmSolution.Instance["Flow1"] as VmProcedure;

                if (_flow == null)
                {
                    //  richTextBox2.AppendText("❌ Không có Flow1\n");
                    AppendLog(richTextBox2, "❌ Không có Flow1");
                    return;
                }

                vmRenderControl1.ModuleSource = _flow;

                _flow.OnWorkEndStatusCallBack -= OnVisionDone;
                _flow.OnWorkEndStatusCallBack += OnVisionDone;
                // _flow.Run();


                AppendLog(richTextBox2, $"✅ Vision OK: {model.Name}");

                //check kết nối vision, nếu không kết nối được thì log ra richtextbox2
                _ = Task.Run(async () =>
                {
                    bool visionOk = await ConnectVisionSafe();

                    if (!visionOk)
                    {
                        this.Invoke(new Action(() =>
                        {
                            AppendLog(richTextBox2, "❌ Cannot connect to Vision.!");
                        }));
                    }
                }
        );
            }
            catch (Exception ex)
            {
                AppendLog(richTextBox2, "❌ " + ex.ToString());
            }

            //connect robot, nếu không kết nối được thì log ra richtextbox2
            _ = Task.Run(async () =>
            {
                AppendLog(richTextBox2, "🤖 Connecting Robot...");

                var ok = await robot.Connect(ipRobot, portRobot);

                this.Invoke(new Action(() =>
                {
                    if (ok)
                        AppendLog(richTextBox2, "✅ Robot Connected");
                    else
                        AppendLog(richTextBox2, "❌ Robot Connect Fail");
                }));
            });
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
        private void FormMain_Resize(object sender, EventArgs e)
        {
            SetupGridStyle();
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

            dataGridView1.Rows[row].Cells[col].Value = text;
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
            if (comboBox1.SelectedIndex < 0)
            {
                AppendLog(richTextBox2, "chưa chọn tray");
                return;
            }
            var model = trays[comboBox1.SelectedIndex];

            currentTrayId = trayRunService.Create(new TrayRun
            {
                TrayName = model.Name,
                Row = model.Row,
                Col = model.Col,
                StartTime = DateTime.Now
            });
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



        //kết nối robot
        private async void button8_Click(object sender, EventArgs e)
        {
            AppendLog(richTextBox2, "🔌 Connecting...");
            var ok = robot.Connect(ipRobot, portRobot);
            AppendLog(richTextBox2, "🔌 Connected");
            if (!await ok)
            {
                AppendLog(richTextBox2, "❌ Không kết nối được Modbus");
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
            flowLayoutPanel6.Visible = !flowLayoutPanel6.Visible;
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


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                // 🔌 stop vision
                vision?.Disconnect();

                // 🤖 stop robot
                robot?.Disconnect();

                // 📷 camera
                // cam?.Dispose();

                // 🔥 dispose VM (QUAN TRỌNG NHẤT)
                if (VmSolution.Instance != null)
                {
                    VmSolution.Instance.Dispose();
                }

                // 🔥 remove event
                if (_flow != null)
                {
                    _flow.OnWorkEndStatusCallBack -= OnVisionDone;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error closing: " + ex.Message);
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
            timer1 = new Timer();
            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            timer1.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void CloseApp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ExtendApp_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }
        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnControl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblHour_Click(object sender, EventArgs e)
        {

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

    }
}