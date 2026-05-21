using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Machine.UI.popupForm
{
    public partial class PopupWarning : Form
    {
        public PopupWarning()
        {
            InitializeComponent();
        }

        private Label lblMessage;
        private Button btnOK;

        public PopupWarning(string message)
        {
            this.Text = "Cảnh báo";
            this.Size = new Size(350, 180);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;

            lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Dock = DockStyle.Top;
            lblMessage.Height = 80;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Font = new Font("Arial", 12, FontStyle.Bold);

            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Width = 100;
            btnOK.Height = 35;
            btnOK.Top = 90;
            btnOK.Left = (this.ClientSize.Width - btnOK.Width) / 2;

            btnOK.Click += (s, e) =>
            {
                this.Close();
            };

            this.Controls.Add(lblMessage);
            this.Controls.Add(btnOK);
        }
    }
}
