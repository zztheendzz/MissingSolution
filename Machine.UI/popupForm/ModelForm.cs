using Machine.UI.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Machine.UI.popupForm
{
    public partial class ModelForm : Form
    {
        // event reload combobox FormMain
        public event Action ResetListModel;

        private readonly string jsonPath;

        // lưu model đang chọn
        private string currentName = "";

        public ModelForm()
        {
            InitializeComponent();

            jsonPath = Path.Combine(
                Application.StartupPath,
                "configDB",
                "models.json");
        }

        private void ModelForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        // ================= LOAD =================

        private void LoadData()
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    File.WriteAllText(jsonPath, "[]");
                }

                string json = File.ReadAllText(jsonPath);

                List<Model1> trays =
                    JsonSerializer.Deserialize<List<Model1>>(json)
                    ?? new List<Model1>();

                dgvModel.DataSource = null;
                dgvModel.DataSource = trays;

                dgvModel.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                dgvModel.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================= ADD =================

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string json = File.ReadAllText(jsonPath);

                List<Model1> trays =
                    JsonSerializer.Deserialize<List<Model1>>(json)
                    ?? new List<Model1>();

                string name = txtName.Text.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Input model name");
                    return;
                }

                bool exists =
                    trays.Any(x => x.Name == name);

                if (exists)
                {
                    MessageBox.Show("Model already exists");
                    return;
                }

                Model1 model = new Model1()
                {
                    Name = name,
                    Row = (int)numRow.Value,
                    Col = (int)numCol.Value,
                    Index = trays.Count + 1,
                    VisionCount = (int)numVision.Value,
                    ProgramVision = txtProgram.Text.Trim()
                };

                trays.Add(model);

                SaveJson(trays);

                // reload combobox
                ResetListModel?.Invoke();

                LoadData();

                ClearInput();

                MessageBox.Show("Add success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================= UPDATE =================

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string json = File.ReadAllText(jsonPath);

                List<Model1> trays =
                    JsonSerializer.Deserialize<List<Model1>>(json)
                    ?? new List<Model1>();

                // tìm model cũ
                var model =
                    trays.FirstOrDefault(x =>
                        x.Name == currentName);

                if (model == null)
                {
                    MessageBox.Show("Model not found");
                    return;
                }

                string newName = txtName.Text.Trim();

                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Input model name");
                    return;
                }

                // check trùng tên
                bool exists = trays.Any(x =>
                    x.Name == newName &&
                    x.Name != currentName);

                if (exists)
                {
                    MessageBox.Show("Name already exists");
                    return;
                }

                // update
                model.Name = newName;
                model.Row = (int)numRow.Value;
                model.Col = (int)numCol.Value;
                model.VisionCount = (int)numVision.Value;
                model.ProgramVision = txtProgram.Text.Trim();

                SaveJson(trays);

                currentName = newName;

                // reload combobox
                ResetListModel?.Invoke();

                LoadData();

                MessageBox.Show("Update success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================= DELETE =================

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentName))
                {
                    MessageBox.Show("Select model");
                    return;
                }

                DialogResult rs = MessageBox.Show(
                    $"Delete model {currentName} ?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (rs != DialogResult.Yes)
                    return;

                string json = File.ReadAllText(jsonPath);

                List<Model1> trays =
                    JsonSerializer.Deserialize<List<Model1>>(json)
                    ?? new List<Model1>();

                var model =
                    trays.FirstOrDefault(x =>
                        x.Name == currentName);

                if (model == null)
                    return;

                trays.Remove(model);

                // update lại index
                for (int i = 0; i < trays.Count; i++)
                {
                    trays[i].Index = i + 1;
                }

                SaveJson(trays);

                // reload combobox
                ResetListModel?.Invoke();

                LoadData();

                ClearInput();

                MessageBox.Show("Delete success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================= SELECT ROW =================

        private void dgvModel_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0)
                    return;

                DataGridViewRow row =
                    dgvModel.Rows[e.RowIndex];

                currentName =
                    row.Cells["Name"].Value.ToString();

                txtName.Text =
                    row.Cells["Name"].Value.ToString();

                numRow.Value =
                    Convert.ToDecimal(
                        row.Cells["Row"].Value);

                numCol.Value =
                    Convert.ToDecimal(
                        row.Cells["Col"].Value);

                numVision.Value =
                    Convert.ToDecimal(
                        row.Cells["VisionCount"].Value);

                txtProgram.Text =
                    row.Cells["ProgramVision"].Value.ToString();
            }
            catch
            {

            }
        }

        // ================= SAVE JSON =================

        private void SaveJson(List<Model1> trays)
        {
            string newJson =
                JsonSerializer.Serialize(
                    trays,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(jsonPath, newJson);
        }

        // ================= CLEAR INPUT =================

        private void ClearInput()
        {
            txtName.Clear();

            txtProgram.Clear();

            numRow.Value = 0;
            numCol.Value = 0;
            numVision.Value = 0;

            currentName = "";

            dgvModel.ClearSelection();
        }

        // ================= CLOSE =================

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }
    }
}