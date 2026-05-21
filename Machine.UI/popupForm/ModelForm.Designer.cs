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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvModel = new System.Windows.Forms.DataGridView();
            this.lbName = new System.Windows.Forms.Label();
            this.lbRow = new System.Windows.Forms.Label();
            this.lbCol = new System.Windows.Forms.Label();
            this.lbVision = new System.Windows.Forms.Label();
            this.lbProgram = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtProgram = new System.Windows.Forms.TextBox();
            this.numRow = new System.Windows.Forms.NumericUpDown();
            this.numCol = new System.Windows.Forms.NumericUpDown();
            this.numVision = new System.Windows.Forms.NumericUpDown();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.infoLayout = new System.Windows.Forms.TableLayoutPanel();
            this.buttonLayout = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVision)).BeginInit();
            this.mainLayout.SuspendLayout();
            this.infoLayout.SuspendLayout();
            this.buttonLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvModel
            // 
            this.dgvModel.AllowUserToAddRows = false;
            this.dgvModel.AllowUserToDeleteRows = false;
            this.dgvModel.AllowUserToResizeRows = false;
            this.dgvModel.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvModel.BackgroundColor = System.Drawing.Color.White;
            this.dgvModel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvModel.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvModel.ColumnHeadersHeight = 45;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvModel.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModel.EnableHeadersVisualStyles = false;
            this.dgvModel.GridColor = System.Drawing.Color.Gainsboro;
            this.dgvModel.Location = new System.Drawing.Point(10, 10);
            this.dgvModel.Margin = new System.Windows.Forms.Padding(10);
            this.dgvModel.MultiSelect = false;
            this.dgvModel.Name = "dgvModel";
            this.dgvModel.ReadOnly = true;
            this.dgvModel.RowHeadersVisible = false;
            this.dgvModel.RowHeadersWidth = 51;
            this.dgvModel.RowTemplate.Height = 42;
            this.dgvModel.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvModel.Size = new System.Drawing.Size(1480, 592);
            this.dgvModel.TabIndex = 0;
            this.dgvModel.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvModel_CellClick);
            // 
            // lbName
            // 
            this.lbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbName.Location = new System.Drawing.Point(20, 15);
            this.lbName.Margin = new System.Windows.Forms.Padding(5);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(209, 55);
            this.lbName.TabIndex = 0;
            this.lbName.Text = "Model Name:";
            this.lbName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbRow
            // 
            this.lbRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRow.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbRow.Location = new System.Drawing.Point(20, 80);
            this.lbRow.Margin = new System.Windows.Forms.Padding(5);
            this.lbRow.Name = "lbRow";
            this.lbRow.Size = new System.Drawing.Size(209, 55);
            this.lbRow.TabIndex = 4;
            this.lbRow.Text = "Row:";
            this.lbRow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbCol
            // 
            this.lbCol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCol.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbCol.Location = new System.Drawing.Point(20, 145);
            this.lbCol.Margin = new System.Windows.Forms.Padding(5);
            this.lbCol.Name = "lbCol";
            this.lbCol.Size = new System.Drawing.Size(209, 55);
            this.lbCol.TabIndex = 8;
            this.lbCol.Text = "Column:";
            this.lbCol.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbVision
            // 
            this.lbVision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbVision.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbVision.Location = new System.Drawing.Point(751, 15);
            this.lbVision.Margin = new System.Windows.Forms.Padding(5);
            this.lbVision.Name = "lbVision";
            this.lbVision.Size = new System.Drawing.Size(209, 55);
            this.lbVision.TabIndex = 2;
            this.lbVision.Text = "Vision Count:";
            this.lbVision.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbProgram
            // 
            this.lbProgram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbProgram.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbProgram.Location = new System.Drawing.Point(751, 80);
            this.lbProgram.Margin = new System.Windows.Forms.Padding(5);
            this.lbProgram.Name = "lbProgram";
            this.lbProgram.Size = new System.Drawing.Size(209, 55);
            this.lbProgram.TabIndex = 6;
            this.lbProgram.Text = "Program:";
            this.lbProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtName
            // 
            this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtName.Location = new System.Drawing.Point(239, 22);
            this.txtName.Margin = new System.Windows.Forms.Padding(5, 12, 10, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(497, 32);
            this.txtName.TabIndex = 1;
            // 
            // txtProgram
            // 
            this.txtProgram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProgram.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtProgram.Location = new System.Drawing.Point(970, 87);
            this.txtProgram.Margin = new System.Windows.Forms.Padding(5, 12, 10, 12);
            this.txtProgram.Name = "txtProgram";
            this.txtProgram.Size = new System.Drawing.Size(499, 32);
            this.txtProgram.TabIndex = 7;
            // 
            // numRow
            // 
            this.numRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numRow.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numRow.Location = new System.Drawing.Point(239, 87);
            this.numRow.Margin = new System.Windows.Forms.Padding(5, 12, 10, 12);
            this.numRow.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numRow.Name = "numRow";
            this.numRow.Size = new System.Drawing.Size(497, 32);
            this.numRow.TabIndex = 5;
            this.numRow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numCol
            // 
            this.numCol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numCol.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numCol.Location = new System.Drawing.Point(239, 152);
            this.numCol.Margin = new System.Windows.Forms.Padding(5, 12, 10, 12);
            this.numCol.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numCol.Name = "numCol";
            this.numCol.Size = new System.Drawing.Size(497, 32);
            this.numCol.TabIndex = 9;
            this.numCol.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numVision
            // 
            this.numVision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numVision.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numVision.Location = new System.Drawing.Point(970, 22);
            this.numVision.Margin = new System.Windows.Forms.Padding(5, 12, 10, 12);
            this.numVision.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numVision.Name = "numVision";
            this.numVision.Size = new System.Drawing.Size(499, 32);
            this.numVision.TabIndex = 3;
            this.numVision.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnAdd
            // 
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAdd.Location = new System.Drawing.Point(922, 18);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(8);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(120, 42);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnUpdate.Location = new System.Drawing.Point(1058, 18);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(8);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(120, 42);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDelete.Location = new System.Drawing.Point(1194, 18);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 42);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClose
            // 
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.Location = new System.Drawing.Point(1330, 18);
            this.btnClose.Margin = new System.Windows.Forms.Padding(8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 42);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.dgvModel, 0, 0);
            this.mainLayout.Controls.Add(this.infoLayout, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 2;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.mainLayout.Size = new System.Drawing.Size(1500, 850);
            this.mainLayout.TabIndex = 0;
            // 
            // infoLayout
            // 
            this.infoLayout.ColumnCount = 4;
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.infoLayout.Controls.Add(this.lbName, 0, 0);
            this.infoLayout.Controls.Add(this.txtName, 1, 0);
            this.infoLayout.Controls.Add(this.lbVision, 2, 0);
            this.infoLayout.Controls.Add(this.numVision, 3, 0);
            this.infoLayout.Controls.Add(this.lbRow, 0, 1);
            this.infoLayout.Controls.Add(this.numRow, 1, 1);
            this.infoLayout.Controls.Add(this.lbProgram, 2, 1);
            this.infoLayout.Controls.Add(this.txtProgram, 3, 1);
            this.infoLayout.Controls.Add(this.lbCol, 0, 2);
            this.infoLayout.Controls.Add(this.numCol, 1, 2);
            this.infoLayout.Controls.Add(this.buttonLayout, 0, 3);
            this.infoLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoLayout.Location = new System.Drawing.Point(3, 615);
            this.infoLayout.Name = "infoLayout";
            this.infoLayout.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.infoLayout.RowCount = 4;
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.infoLayout.Size = new System.Drawing.Size(1494, 232);
            this.infoLayout.TabIndex = 1;
            // 
            // buttonLayout
            // 
            this.infoLayout.SetColumnSpan(this.buttonLayout, 4);
            this.buttonLayout.Controls.Add(this.btnClose);
            this.buttonLayout.Controls.Add(this.btnDelete);
            this.buttonLayout.Controls.Add(this.btnUpdate);
            this.buttonLayout.Controls.Add(this.btnAdd);
            this.buttonLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonLayout.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonLayout.Location = new System.Drawing.Point(18, 208);
            this.buttonLayout.Name = "buttonLayout";
            this.buttonLayout.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.buttonLayout.Size = new System.Drawing.Size(1458, 11);
            this.buttonLayout.TabIndex = 10;
            // 
            // ModelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1500, 850);
            this.Controls.Add(this.mainLayout);
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "ModelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Model Manager";
            this.Load += new System.EventHandler(this.ModelForm_Load);
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