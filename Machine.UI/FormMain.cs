
using DocumentFormat.OpenXml.Wordprocessing;
using FrontendUI.Design.Controls;
using Machine.UI.model;
using Machine.UI.services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
        ExportExcelService excelService = new ExportExcelService();
        CameraService cam = new CameraService();
        bool isRunning = false;
        TcpClientService vision = new TcpClientService();
        ModbusService robot = new ModbusService();
        int positions = 0;
        //string ipVision = "192.168.0.211";
        string ipVision = "127.0.0.1";
        string ipRobot = "127.0.0.1";
        TrayModel currentTray;
        TrayProcessor processor;
        ushort registerData = 5;

        VisionDataService visionDb;
        TrayRunService trayRunService;
        int _previousIndex = -1;
        bool _isInitializing = true;

        int portVision = 8001;
        int portRobot = 502;
        int currentTrayId;
        int total = 0;
        int ok = 0;
        int ng = 0;
        int none = 0;


        List<Model1> trays;

        public FormMain()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;

            string conn = configDB.configDB.ConnectionString;

            visionDb = new VisionDataService(conn);
            trayRunService = new TrayRunService(conn);
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            string path = Path.Combine(Application.StartupPath, "configDB", "models.json");

            string json = File.ReadAllText(path);

            trays = JsonSerializer.Deserialize<List<Model1>>(json);
            robot.OnIndexReceived = (index) =>
            {
                this.Invoke(new Action(() =>
                {
                    richTextBox2.AppendText($" Index: {index}\n");
                }));
            };
            vision.OnRawData = (msg) =>
            {
                this.Invoke(new Action(() =>
                {
                    // richTextBox1.AppendText(msg + "\n");
                    robot.WriteStringToRobot(registerData, msg); //send data to robot
                    var results = VisionParser.Parse(msg); // 👈 parse ở đây

                    string appendTextRs = $"[{DateTime.Now:HH: mm: ss}]";
                    foreach (var item in results)
                    {
                        appendTextRs += item + "\t";
                    }
                    richTextBox3.AppendText(appendTextRs + "\n");

                    var cells = processor.ProcessBatch(results);

                    InsertData(cells);

                    foreach (var cell in cells)
                    {
                        if (!string.IsNullOrEmpty(cell.Result))
                        {
                            UpdateTray(cell.Row, cell.Col, cell.Result);
                        }

                        if (processor.IsFull)
                        {
                            Console.WriteLine("Tray FULL");
                        }
                    }
                }));
            };

            await Task.WhenAll(
                //vision.Connect(ipVision, portVision),
                // robot.Connect(ipRobot, portRobot)

                );
            robot.OnTrayCompleted = () =>
            {
                this.Invoke(new Action(() =>
                {
                    richTextBox2.AppendText("✅ Tray DONE\n");
                    trayRunService.UpdateEndTime(currentTrayId);
                    // 🔥 reset UI
                    ClearTray();

                    // 🔥 reset processor
                    processor.Reset();
                }));
            };

            InitComboBox();
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            LoadTray(0);
            _previousIndex = 0;
            comboBox1.SelectedIndex = 0;

            _isInitializing = false;
        }

        public void InsertData(List<Cell> cells)
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
            textBox3.Text = Math.Round((double)ng / total * 100, 2).ToString() + "%";
            textBox1.Text = Math.Round((double)none / total * 100, 2).ToString() + "%";
            textBox2.Text = (100 - Math.Round((double)ng / total * 100, 2) - Math.Round((double)none / total * 100, 2)).ToString() + "%";

            txtOK.Text = ok.ToString();
            txtNG.Text = ng.ToString();
            textBox6.Text = total.ToString();
            textBox4.Text = none.ToString();
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

            currentTray = model.ToTrayModel();

            processor = new TrayProcessor(currentTray);
            processor.Reset();
            StartTray();
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

            dataGridView1.DataSource = table;
            SetupGridStyle();
            // ===== 3. LOAD VISION (🔥 QUAN TRỌNG) =====
            if (!File.Exists(model.ProgramVision))
            {
                richTextBox2.AppendText($"📂 Path: {model.ProgramVision}\n");
                richTextBox2.AppendText("❌ File không tồn tại\n");
                return;
            }

            try
            {
                richTextBox2.AppendText($"📂 Load: {model.ProgramVision}\n");

                string dir = Path.GetDirectoryName(model.ProgramVision);
                Directory.SetCurrentDirectory(dir);
                try
                {
                    if (VmSolution.Instance != null)
                    {
                      //  VmSolution.Instance.Dispose();
                    }

                    System.Threading.Thread.Sleep(200);
                    VmSolution.Load(model.ProgramVision, "", false);
                    richTextBox2.AppendText("Load OK\n");
                }
                catch (Exception e)
                {
                    richTextBox2.AppendText("❌ Load fail\n" + e.ToString() + "\n");
                }

                if (VmSolution.Instance == null)
                {
                    richTextBox2.AppendText("❌ Load fail\n");
                    return;
                }

                var flow = VmSolution.Instance["Flow1"] as VmProcedure;

                if (flow == null)
                {
                    richTextBox2.AppendText("❌ Không có Flow1\n");
                    return;
                }

                vmRenderControl1.ModuleSource = flow;

                flow.OnWorkEndStatusCallBack -= OnVisionDone;
                flow.OnWorkEndStatusCallBack += OnVisionDone;
            //    flow.Run();

                richTextBox2.AppendText($"✅ Vision OK: {model.Name}\n");
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("❌ " + ex.ToString() + "\n");
            }
        }
        private void OnVisionDone(object sender, EventArgs e)
        {
            var proc = sender as VmProcedure;

            this.Invoke(new Action(() =>
            {
                richTextBox2.AppendText("🔥 CALLBACK\n");
            }));
            this.Invoke(new Action(() =>
            {
                richTextBox2.AppendText($"ModuResult = {(proc.ModuResult == null ? "NULL" : "OK")}\n");
            }));
            if (proc?.ModuResult == null)
            {
                this.Invoke(new Action(() =>
                {
                    richTextBox2.AppendText("❌ ModuResult NULL\n");
                }));
                return;
            }

            // 🔥 lấy list output
            var outputs = proc.ModuResult.GetAllOutputNameInfo();

            this.Invoke(new Action(() =>
            {
                richTextBox2.AppendText($"Output count = {outputs?.Count}\n");
            }));
            foreach (var item in outputs)
            {
                string name = item.Name;
                string type = item.TypeName.ToString();

                this.Invoke(new Action(() =>
                {
                    richTextBox2.AppendText($"Name: {name} | Type: {type}\n");
                }));
            }
        }

        // ================== STYLE ==================
        private void SetupGridStyle()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;

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
        // ================== UPDATE ==================
        private void UpdateTray(int row, int col, string result)
        {
            if (row < 0 || col < 0) return;
            if (row >= dataGridView1.Rows.Count) return;
            if (col >= dataGridView1.Columns.Count) return;

            string text;

            if (result == "EMPTY")
                text = $"{row},{col}\nEMPTY";
            else
                text = $"{row},{col}\n{result}";

            dataGridView1.Rows[row].Cells[col].Value = text;
            dataGridView1.InvalidateCell(col, row);
        }
        private void ClearTray()
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
        void StartTray()
        {
            if (comboBox1.SelectedIndex < 0)
            {
              //  MessageBox.Show("Chưa chọn tray!");
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
        // ================== COLOR ==================
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
            {     //cam.Start(pictureBox1);
            }
        }

        private void btnDevice_Click(object sender, EventArgs e)
        {
            if (flPnDevice.Height == 70)
                flPnDevice.Height = 280;
            else
                flPnDevice.Height = 70;
        }

        private void btnImage_Click(object sender, EventArgs e)
        {
            if (flPnImage.Height == 70)
                flPnImage.Height = 210;
            else
                flPnImage.Height = 70;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCamera_Click(object sender, EventArgs e) { }

        private void btnSetting_Click(object sender, EventArgs e) { }

        private void tUpdate_Tick(object sender, EventArgs e)
        {
            lblHour.Text = DateTime.Now.ToString("hh:mm");
            lblDay.Text = DateTime.Now.DayOfWeek.ToString();
            lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }



        private void button7_Click(object sender, EventArgs e)
        {
            int val = int.Parse("1");

            robot.WriteIndex(val);

            richTextBox2.AppendText($"📤 Send Index: {val}\n");
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            richTextBox2.AppendText("🔌 Connecting...\n");
            var ok = robot.Connect(ipRobot, portRobot);

            if (!await ok)
            {
                richTextBox2.AppendText("Không kết nối được Modbus");
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            robot.Disconnect();

            richTextBox2.AppendText("🔌 Disconnected\n");
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {
            //showdata vision
            // richTextBox1.AppendText(results + "\n");
        }

        private async void button10_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBox2.AppendText("🔌 Connecting Vision...\n");

                await vision.Connect(ipVision, portVision);

                richTextBox2.AppendText("✅ Vision Connected\n");
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("❌ Vision Connect Error: " + ex.Message + "\n");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //disconnect vision
            vision.Disconnect();
            richTextBox2.AppendText("🔌 Vision Disconnected\n");

        }

        private void button12_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files|*.xlsx";
                sfd.FileName = "tray" + DateTime.Now.ToString("yyyyMMdd HHmmss");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    excelService.Export(dataGridView1, sfd.FileName);
                    richTextBox2.AppendText("✅ Export thành công!");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Clear data
            total = 0;
            ok = 0;
            ng = 0;
            none = 0;
            txtOK.Text = "0%";
            txtNG.Text = "0%";
            textBox6.Text = "0";
            textBox1.Text = "0%";
            textBox2.Text = "0%";
            textBox3.Text = "0%";
            textBox4.Text = "0%";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var flow = VmSolution.Instance["Flow1"] as VmProcedure;
            try { 
                flow?.Run();
                    }
            catch(Exception ex)
            {
                richTextBox2.AppendText("ERR run vision " + ex.ToString() +"\n");
            }

        }
        private void txtOK_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNG_TextChanged(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void vmRenderControl1_Load(object sender, EventArgs e)
        {

        }
        private void tHiden_Tick(object sender, EventArgs e) { }

        private void btnReset_Click(object sender, EventArgs e) { }

        private void btnData_Click(object sender, EventArgs e) { }

        private void ImageDisplayInit() { }

        private void groupBox3_Enter(object sender, EventArgs e) { }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private async void btnMenu_Click(object sender, EventArgs e) { }
        private void pnResult_Paint(object sender, PaintEventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}