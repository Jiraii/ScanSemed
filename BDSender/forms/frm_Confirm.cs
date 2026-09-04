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
    public partial class frm_Confirm : Form
    {
        public bool result;
        public frm_Confirm(string text)
        {
            InitializeComponent();
            lblText.Text = text;
            ApplyModernTheme();
        }

        private void ApplyModernTheme()
        {
            try {
                this.BackColor = System.Drawing.ColorTranslator.FromHtml("#F8FAFC");
                lblText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                lblText.ForeColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");

                // Style buttons if they are accessible (assuming btnOK and btnCencel)
                btnOK.BackColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");
                btnOK.ForeColor = Color.White;
                btnOK.FlatStyle = FlatStyle.Flat;
                btnOK.Font = new Font("Segoe UI", 12, FontStyle.Bold);

                btnCencel.BackColor = System.Drawing.ColorTranslator.FromHtml("#94A3B8");
                btnCencel.ForeColor = Color.White;
                btnCencel.FlatStyle = FlatStyle.Flat;
                btnCencel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            } catch {}
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
