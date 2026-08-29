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
    public partial class frmNumpad : Form
    {
        TextBox txtReturn;
        public frmNumpad(string txtHeader,ref TextBox txtBoxInsert)
        {
            InitializeComponent();
            lblHeader.Text = txtHeader;
            txtReturn = txtBoxInsert;
            txtNumber.Text = txtBoxInsert.Text;
        }

        private void btn0_Click(object sender, EventArgs e)
        {

            txtNumber.Text += "0";
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "1";
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "2";
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "3";
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "4";
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "5";
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "6";
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "7";
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "8";
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            txtNumber.Text += "9";
        }

        private void btnBackspace_Click(object sender, EventArgs e)
        {
            if (txtNumber.Text.Length >= 1)
            {
               txtNumber.Text = txtNumber.Text.Remove(txtNumber.Text.Length-1);
            }            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            ///txtNumber.Text += Keys.Enter;
            txtReturn.Text = txtNumber.Text;
            this.Close();
        }
    }
}
