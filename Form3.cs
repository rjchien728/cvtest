using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cvtest
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "kevin666")
            {
                MessageBox.Show("密碼錯誤，請重新輸入");
            }
            else
            {
                Form1 cvform = new Form1();
                this.Visible = false;
                cvform.ShowDialog(this);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = Properties.Settings.Default.passwordsetting;
            if (textBox1.Text == "kevin666")
            {
                Form1 cvform = new Form1();
                cvform.ShowDialog(this);
                this.Visible = false;
            }

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.passwordsetting = this.textBox1.Text;
            Properties.Settings.Default.Save();
        }
    }
}
