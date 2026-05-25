using System;
using System.Windows.Forms;

namespace Machine.UI.popupForm
{
    public partial class LoadingForm : Form
    {
        public event Action CancelRequested;

        public LoadingForm()
        {
            InitializeComponent();

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.FormBorderStyle =
                FormBorderStyle.FixedDialog;

            this.ControlBox = false;

            this.MaximizeBox = false;

            this.MinimizeBox = false;

            this.TopMost = true;

            this.Text = "Please wait...";

            progressBar1.Style =
                ProgressBarStyle.Marquee;

            progressBar1.MarqueeAnimationSpeed = 30;

            btnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(
            object sender,
            EventArgs e)
        {
            btnCancel.Enabled = false;

            lblTitle.Text =
                "Cancelling...";

            CancelRequested?.Invoke();
        }
    }
}