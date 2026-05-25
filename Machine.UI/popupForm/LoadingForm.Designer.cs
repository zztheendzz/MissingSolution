namespace Machine.UI.popupForm
{
    partial class LoadingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.ProgressBar progressBar1;

        private System.Windows.Forms.Button btnCancel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing &&
                (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle =
                new System.Windows.Forms.Label();

            this.progressBar1 =
                new System.Windows.Forms.ProgressBar();

            this.btnCancel =
                new System.Windows.Forms.Button();

            this.SuspendLayout();

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;

            this.lblTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    11F,
                    System.Drawing.FontStyle.Bold);

            this.lblTitle.Location =
                new System.Drawing.Point(30, 20);

            this.lblTitle.Name = "lblTitle";

            this.lblTitle.Size =
                new System.Drawing.Size(170, 20);

            this.lblTitle.TabIndex = 0;

            this.lblTitle.Text =
                "Exporting excel...";

            // 
            // progressBar1
            // 
            this.progressBar1.Location =
                new System.Drawing.Point(34, 50);

            this.progressBar1.MarqueeAnimationSpeed = 30;

            this.progressBar1.Name =
                "progressBar1";

            this.progressBar1.Size =
                new System.Drawing.Size(320, 22);

            this.progressBar1.Style =
                System.Windows.Forms.ProgressBarStyle.Marquee;

            this.progressBar1.TabIndex = 1;

            // 
            // btnCancel
            // 
            this.btnCancel.Location =
                new System.Drawing.Point(145, 85);

            this.btnCancel.Name =
                "btnCancel";

            this.btnCancel.Size =
                new System.Drawing.Size(100, 30);

            this.btnCancel.TabIndex = 2;

            this.btnCancel.Text = "Cancel";

            this.btnCancel.UseVisualStyleBackColor =
                true;

            // 
            // LoadingForm
            // 
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(7F, 15F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.BackColor =
                System.Drawing.Color.White;

            this.ClientSize =
                new System.Drawing.Size(390, 135);

            this.Controls.Add(this.btnCancel);

            this.Controls.Add(this.progressBar1);

            this.Controls.Add(this.lblTitle);

            this.FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;

            this.Name = "LoadingForm";

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text = "Please wait...";

            this.TopMost = true;

            this.ControlBox = false;

            this.ResumeLayout(false);

            this.PerformLayout();
        }

        #endregion
    }
}