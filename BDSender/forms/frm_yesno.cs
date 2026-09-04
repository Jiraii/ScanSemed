using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDSender.forms
{
    public partial class frm_yesno : Form
    {
        public bool result;
        public frm_yesno(string text)
        {
            InitializeComponent();
            lblText.Text = text;
        }
        public void set_emergency()
        {
            lblText.ForeColor = Color.Red;
        }

        private void btnCencel_Click(object sender, EventArgs e)
        {
            result = false;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            result = true;
            this.Close();
        }
    }
}
