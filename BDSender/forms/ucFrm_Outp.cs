using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BDSender.cls;

namespace BDSender.forms
{
    public partial class ucFrm_Outp : UserControl
    {
        System.Drawing.Color colHeader;
        System.Drawing.Color colFloor;
        System.Drawing.Color colBody;
        System.Drawing.Color colActive;

        public ucFrm_Outp()
        {
            InitializeComponent();
        }

        private void ucFrm_Outp_Load(object sender, EventArgs e)
        {
            ApplyModernTheme();
        }

        public void ApplyModernTheme()
        {
            try {
                this.BackColor = System.Drawing.ColorTranslator.FromHtml("#F8FAFC");
                tblHeader.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                
                txtName.Font = new Font("Segoe UI", 20, FontStyle.Bold);
                txtName.ForeColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");
                
                txtHN.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                txtHN.ForeColor = System.Drawing.ColorTranslator.FromHtml("#16A34A");
                txtHN.BackColor = System.Drawing.ColorTranslator.FromHtml("#DCFCE7");
                
                txtVN.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                txtVN.ForeColor = System.Drawing.ColorTranslator.FromHtml("#0284C7");
                txtVN.BackColor = System.Drawing.ColorTranslator.FromHtml("#E0F2FE");

                btnOutp.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                btnOutp.ForeColor = Color.White;
                btnOutp.BackColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");
                btnOutp.FlatStyle = FlatStyle.Flat;
                btnOutp.FlatAppearance.BorderSize = 0;
                btnOutp.BackgroundImage = null; // Remove old weird images
                
                dgvDetail.BackgroundColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
                dgvDetail.BorderStyle = BorderStyle.None;
                dgvDetail.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvDetail.GridColor = System.Drawing.ColorTranslator.FromHtml("#E2E8F0");
                dgvDetail.EnableHeadersVisualStyles = false;
                dgvDetail.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvDetail.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");
                dgvDetail.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvDetail.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                dgvDetail.ColumnHeadersHeight = 45;
                dgvDetail.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                dgvDetail.DefaultCellStyle.SelectionBackColor = System.Drawing.ColorTranslator.FromHtml("#F1F5F9");
                dgvDetail.DefaultCellStyle.SelectionForeColor = System.Drawing.ColorTranslator.FromHtml("#0F766E");
                dgvDetail.RowTemplate.Height = 45;
                dgvDetail.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#F8FAFC");
                dgvDetail.RowHeadersVisible = false;
            } catch {}
        }

        public void setOutp(string txtOutp)
        {
            btnOutp.Text = txtOutp;
        }

        public void updateDetail(List<orderDetail> detail)
        {
            try {
                if (detail.Count > 0)
                {
                    //txtNo.Text = detail[0].no;
                    txtVN.Text = detail[0].qn;
                    txtName.Text = detail[0].name;
                    txtHN.Text = detail[0].hn;
                    //txtGender.Text = detail[0].gender;
                    //txtAge.Text = detail[0].age;
                    dgvDetail.DataSource = detail;
                    btnOutp.BackgroundImage = Properties.Resources.LED_ON_180x180;
                    //this.BackColor = colActive;
                }
                else
                {
                    //txtNo.Text = "";
                    txtVN.Text = "";
                    txtName.Text = "";
                    txtHN.Text = "";
                    //txtGender.Text = "";
                    //txtAge.Text = "";
                    //dgvDetail.Rows.Clear();
                    //dgvDetail.Refresh();
                    dgvDetail.DataSource = detail;
                    btnOutp.BackgroundImage = Properties.Resources.LED_OFF_180x180;
                    //this.BackColor = colBody;
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
