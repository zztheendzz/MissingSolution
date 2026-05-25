using System;
using System.Drawing;
using System.Windows.Forms;

namespace Machine.UI.popupForm
{
    partial class ModelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private DataGridView dgvModel;

        private Label lbName;
        private Label lbRow;
        private Label lbCol;
        private Label lbVision;
        private Label lbProgram;

        private TextBox txtName;
        private TextBox txtProgram;

        private NumericUpDown numRow;
        private NumericUpDown numCol;
        private NumericUpDown numVision;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClose;

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel infoLayout;
        private FlowLayoutPanel buttonLayout;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle headerStyle =
                new DataGridViewCellStyle();

            DataGridViewCellStyle cellStyle =
                new DataGridViewCellStyle();

            this.dgvModel = new DataGridView();

            this.lbName = new Label();
            this.lbRow = new Label();
            this.lbCol = new Label();
            this.lbVision = new Label();
            this.lbProgram = new Label();

            this.txtName = new TextBox();
            this.txtProgram = new TextBox();

            this.numRow = new NumericUpDown();
            this.numCol = new NumericUpDown();
            this.numVision = new NumericUpDown();

            this.btnAdd = new Button();
            this.btnUpdate = new Button();
            this.btnDelete = new Button();
            this.btnClose = new Button();

            this.mainLayout = new TableLayoutPanel();
            this.infoLayout = new TableLayoutPanel();
            this.buttonLayout = new FlowLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.dgvModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVision)).BeginInit();

            this.mainLayout.SuspendLayout();
            this.infoLayout.SuspendLayout();
            this.buttonLayout.SuspendLayout();

            this.SuspendLayout();

            // =====================================================
            // dgvModel
            // =====================================================

            this.dgvModel.AllowUserToAddRows = false;
            this.dgvModel.AllowUserToDeleteRows = false;
            this.dgvModel.AllowUserToResizeRows = false;

            this.dgvModel.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            this.dgvModel.BackgroundColor = Color.White;

            this.dgvModel.BorderStyle = BorderStyle.None;

            this.dgvModel.Dock = DockStyle.Fill;

            this.dgvModel.EnableHeadersVisualStyles = false;

            this.dgvModel.GridColor = Color.Gainsboro;

            this.dgvModel.Location = new Point(10, 10);

            this.dgvModel.Margin = new Padding(10);

            this.dgvModel.MultiSelect = false;

            this.dgvModel.Name = "dgvModel";

            this.dgvModel.ReadOnly = true;

            this.dgvModel.RowHeadersVisible = false;

            this.dgvModel.RowTemplate.Height = 42;

            this.dgvModel.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            headerStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            headerStyle.BackColor =
                Color.FromArgb(235, 235, 235);

            headerStyle.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);

            headerStyle.ForeColor = Color.Black;

            headerStyle.WrapMode =
                DataGridViewTriState.True;

            this.dgvModel.ColumnHeadersDefaultCellStyle =
                headerStyle;

            this.dgvModel.ColumnHeadersHeight = 45;

            cellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            cellStyle.BackColor = Color.White;

            cellStyle.Font =
                new Font("Segoe UI", 10.5F);

            cellStyle.ForeColor = Color.Black;

            cellStyle.SelectionBackColor =
                Color.FromArgb(0, 120, 215);

            cellStyle.SelectionForeColor =
                Color.White;

            cellStyle.WrapMode =
                DataGridViewTriState.False;

            this.dgvModel.DefaultCellStyle =
                cellStyle;

            this.dgvModel.CellClick +=
                new DataGridViewCellEventHandler(
                    this.dgvModel_CellClick);

            // =====================================================
            // LABEL NAME
            // =====================================================

            this.lbName.Text = "Model Name:";
            this.lbName.Dock = DockStyle.Fill;
            this.lbName.TextAlign =
                ContentAlignment.MiddleRight;
            this.lbName.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lbName.Margin = new Padding(5);

            // =====================================================
            // LABEL ROW
            // =====================================================

            this.lbRow.Text = "Row:";
            this.lbRow.Dock = DockStyle.Fill;
            this.lbRow.TextAlign =
                ContentAlignment.MiddleRight;
            this.lbRow.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lbRow.Margin = new Padding(5);

            // =====================================================
            // LABEL COL
            // =====================================================

            this.lbCol.Text = "Column:";
            this.lbCol.Dock = DockStyle.Fill;
            this.lbCol.TextAlign =
                ContentAlignment.MiddleRight;
            this.lbCol.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lbCol.Margin = new Padding(5);

            // =====================================================
            // LABEL VISION
            // =====================================================

            this.lbVision.Text = "Vision Count:";
            this.lbVision.Dock = DockStyle.Fill;
            this.lbVision.TextAlign =
                ContentAlignment.MiddleRight;
            this.lbVision.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lbVision.Margin = new Padding(5);

            // =====================================================
            // LABEL PROGRAM
            // =====================================================

            this.lbProgram.Text = "Program:";
            this.lbProgram.Dock = DockStyle.Fill;
            this.lbProgram.TextAlign =
                ContentAlignment.MiddleRight;
            this.lbProgram.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lbProgram.Margin = new Padding(5);

            // =====================================================
            // TEXTBOX NAME
            // =====================================================

            this.txtName.Dock = DockStyle.Fill;

            this.txtName.Font =
                new Font("Segoe UI", 11F);

            this.txtName.Margin =
                new Padding(5, 12, 10, 12);

            // =====================================================
            // TEXTBOX PROGRAM
            // =====================================================

            this.txtProgram.Dock = DockStyle.Fill;

            this.txtProgram.Font =
                new Font("Segoe UI", 11F);

            this.txtProgram.Margin =
                new Padding(5, 12, 10, 12);

            // =====================================================
            // NUM ROW
            // =====================================================

            this.numRow.Dock = DockStyle.Fill;

            this.numRow.Font =
                new Font("Segoe UI", 11F);

            this.numRow.Margin =
                new Padding(5, 12, 10, 12);

            this.numRow.Maximum = 1000;

            this.numRow.TextAlign =
                HorizontalAlignment.Center;

            // =====================================================
            // NUM COL
            // =====================================================

            this.numCol.Dock = DockStyle.Fill;

            this.numCol.Font =
                new Font("Segoe UI", 11F);

            this.numCol.Margin =
                new Padding(5, 12, 10, 12);

            this.numCol.Maximum = 1000;

            this.numCol.TextAlign =
                HorizontalAlignment.Center;

            // =====================================================
            // NUM VISION
            // =====================================================

            this.numVision.Dock = DockStyle.Fill;

            this.numVision.Font =
                new Font("Segoe UI", 11F);

            this.numVision.Margin =
                new Padding(5, 12, 10, 12);

            this.numVision.Maximum = 1000;

            this.numVision.TextAlign =
                HorizontalAlignment.Center;

            // =====================================================
            // BUTTON ADD
            // =====================================================

            this.btnAdd.Text = "Add";
            this.btnAdd.Size = new Size(120, 42);
            this.btnAdd.Margin = new Padding(8);
            this.btnAdd.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Cursor = Cursors.Hand;

            this.btnAdd.Click +=
                new EventHandler(this.btnAdd_Click);

            // =====================================================
            // BUTTON UPDATE
            // =====================================================

            this.btnUpdate.Text = "Update";
            this.btnUpdate.Size = new Size(120, 42);
            this.btnUpdate.Margin = new Padding(8);
            this.btnUpdate.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnUpdate.FlatStyle = FlatStyle.Flat;
            this.btnUpdate.Cursor = Cursors.Hand;

            this.btnUpdate.Click +=
                new EventHandler(this.btnUpdate_Click);

            // =====================================================
            // BUTTON DELETE
            // =====================================================

            this.btnDelete.Text = "Delete";
            this.btnDelete.Size = new Size(120, 42);
            this.btnDelete.Margin = new Padding(8);
            this.btnDelete.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Cursor = Cursors.Hand;

            this.btnDelete.Click +=
                new EventHandler(this.btnDelete_Click);

            // =====================================================
            // BUTTON CLOSE
            // =====================================================

            this.btnClose.Text = "Close";
            this.btnClose.Size = new Size(120, 42);
            this.btnClose.Margin = new Padding(8);
            this.btnClose.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Cursor = Cursors.Hand;

            // =====================================================
            // BUTTON LAYOUT
            // =====================================================

            this.buttonLayout.Dock = DockStyle.Fill;

            this.buttonLayout.FlowDirection =
                FlowDirection.RightToLeft;

            this.buttonLayout.Padding =
                new Padding(0, 10, 0, 0);

            this.buttonLayout.Controls.Add(this.btnClose);
            this.buttonLayout.Controls.Add(this.btnDelete);
            this.buttonLayout.Controls.Add(this.btnUpdate);
            this.buttonLayout.Controls.Add(this.btnAdd);

            // =====================================================
            // INFO LAYOUT
            // =====================================================

            this.infoLayout.ColumnCount = 4;

            this.infoLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 15F));

            this.infoLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35F));

            this.infoLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 15F));

            this.infoLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35F));

            this.infoLayout.RowCount = 4;

            this.infoLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65F));

            this.infoLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65F));

            this.infoLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65F));

            this.infoLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            this.infoLayout.Dock = DockStyle.Fill;

            this.infoLayout.Padding =
                new Padding(15, 10, 15, 10);

            this.infoLayout.Controls.Add(
                this.lbName, 0, 0);

            this.infoLayout.Controls.Add(
                this.txtName, 1, 0);

            this.infoLayout.Controls.Add(
                this.lbVision, 2, 0);

            this.infoLayout.Controls.Add(
                this.numVision, 3, 0);

            this.infoLayout.Controls.Add(
                this.lbRow, 0, 1);

            this.infoLayout.Controls.Add(
                this.numRow, 1, 1);

            this.infoLayout.Controls.Add(
                this.lbProgram, 2, 1);

            this.infoLayout.Controls.Add(
                this.txtProgram, 3, 1);

            this.infoLayout.Controls.Add(
                this.lbCol, 0, 2);

            this.infoLayout.Controls.Add(
                this.numCol, 1, 2);

            this.infoLayout.Controls.Add(
                this.buttonLayout, 0, 3);

            this.infoLayout.SetColumnSpan(
                this.buttonLayout, 4);

            // =====================================================
            // MAIN LAYOUT
            // =====================================================

            this.mainLayout.ColumnCount = 1;

            this.mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            this.mainLayout.RowCount = 2;

            this.mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 72F));

            this.mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 28F));

            this.mainLayout.Dock = DockStyle.Fill;

            this.mainLayout.Controls.Add(
                this.dgvModel, 0, 0);

            this.mainLayout.Controls.Add(
                this.infoLayout, 0, 1);

            // =====================================================
            // FORM
            // =====================================================

            this.AutoScaleDimensions =
                new SizeF(8F, 16F);

            this.AutoScaleMode =
                AutoScaleMode.Font;

            this.BackColor =
                Color.WhiteSmoke;

            this.ClientSize =
                new Size(1500, 850);

            this.Controls.Add(this.mainLayout);

            this.MinimumSize =
                new Size(1200, 700);

            this.Name = "ModelForm";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.Text = "Model Manager";

            this.Load +=
                new EventHandler(this.ModelForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvModel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVision)).EndInit();

            this.mainLayout.ResumeLayout(false);

            this.infoLayout.ResumeLayout(false);
            this.infoLayout.PerformLayout();

            this.buttonLayout.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        #endregion
    }
}