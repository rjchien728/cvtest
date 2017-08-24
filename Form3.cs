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
        private string authodic = @"C:\ProgramData\System64\WindowsPowerShell";
        private List<string> usedserialno;

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool pass = true;

            if (!File.Exists(authofile))
                autholize(textBox1.Text);
            else
            {
                foreach (var no in usedserialno)
                {
                    if (textBox1.Text == no)
                    {
                        MessageBox.Show("已過期的序號");
                        pass = false;
                    }
                }
                if (pass)
                {
                    autholize(textBox1.Text);
                }
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            if (File.Exists(authofile))
            {
                //autholize
                string duedate = checkautho(authofile);
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(duedate)) > 0)//逾期
                {
                    MessageBox.Show("你的認證已逾期");
                }
                else
                {
                    openMain();
                }
            }
        }


        private void openMain()
        {
            Form1 cvform = new Form1();//進入主程式
            cvform.ShowDialog(this);
            this.Visible = false;

            //MessageBox.Show("OK");
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Properties.Settings.Default.passwordsetting = this.textBox1.Text;
            //Properties.Settings.Default.Save();
        }


        private void autholize(string serialno)
        {
            switch (serialno)
            {
                case _serialno1:
                    writeauthofile(serialno, DateTime.Now.AddDays(9999).ToShortDateString());
                    openMain();
                    break;
                case _serialno2:
                    writeauthofile(serialno, DateTime.Now.AddDays(30).ToShortDateString());
                    openMain();
                    break;
                case _serialno3:
                    writeauthofile(serialno, DateTime.Now.AddDays(7).ToShortDateString());
                    openMain();
                    break;
                default:
                    MessageBox.Show("認證序號有誤，請重新確認");
                    break;
            }
        }


        private string checkautho(string file)
        {
            usedserialno = new List<string>();
            string no = null;
            string date = null;
            StreamReader sr = new StreamReader(authofile);
            string line = null;
            int lineno = 1;
            while ((line = sr.ReadLine()) != null)
            {
                if (lineno % 2 == 1)
                {
                    no = line;
                    usedserialno.Add(no);
                    //MessageBox.Show("No=" + no);

                }
                else
                {
                    date = line;
                    //MessageBox.Show("Date=" + date);
                }
                lineno++;
            }
            sr.Close();
            return date;
        }

        private void writeauthofile(string s, string duedate)
        {
            //FileStream fs;
            //if (fexist == false)
            //    fs = new FileStream(authofile, FileMode.CreateNew);
            //else
            //    fs = new FileStream(authofile, FileMode.Open);
            if (!Directory.Exists(authodic))
            {
                System.IO.Directory.CreateDirectory(authodic);
            }
            StreamWriter sw = new StreamWriter(authofile, true);
            sw.WriteLine(s);
            sw.WriteLine(duedate);
            sw.Flush();
            sw.Close();
            //fs.Close();
        }


    }
}
