using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace cvtest
{
    public partial class Form3 : Form
    {
        private const string _serialno1 = "HAK074W85"; //永久
        private const string _serialno2 = "PAKO39LLO"; //一個月
        private const string _serialno3 = "TE965017K"; //七天
        private string authofile = @"C:\ProgramData\System64\WindowsPowerShell\log.txt";

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

        private void firstautholize()
        {
            switch (textBox1.Text)
            {
                case _serialno1:


                    break;
                case _serialno2:


                    break;
                case _serialno3:


                    break;
            }

        }


        private string getseralno()
        {
            string s = null;
            using (StreamReader sr = new StreamReader(authofile))
            {


            }

                return s;
        }
    }
}
