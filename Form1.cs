using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
//using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;

//using Dynamsoft.Barcode;
using System.Diagnostics;
using ZXing.QrCode;
using ZXing;
using TouchlessLib;
//using DirectX.Capture;
//using DirectShowLib;

namespace cvtest
{
    public partial class Form1 : Form
    {
        Emgu.CV.Capture webCam;
        private BarcodeReader _barcodeReader;
        private HaarCascade haarCascade;
        bool pause = false;
        private TouchlessMgr _touch;
        int wid = 640;//capture width
        int hei = 480;//capture height
        Queue<double> frate = new Queue<double>();
        int Sindex;
        //bool rloadyet = false; 

        Image<Ycc, UInt16> YcrCbFrame = null;
        Image<Bgr, Byte> BgrFrame = null;

        public Form1()
        {
            InitializeComponent();
            _touch = new TouchlessMgr();
            //comboBox1.SelectedIndex = 0;
            //this.Width = 900;
            //this.Height = 1800;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var c in _touch.Cameras)
                comboBox1.Items.Add(c);
            comboBox1.SelectedIndex = 0;//default
            //取得第一支網路攝影機
            webCam = new Emgu.CV.Capture(comboBox1.SelectedIndex);
            Sindex = comboBox1.SelectedIndex;
            //抓取解析度
            LoadFrameSize(ref webCam);
            Showcurrentframesize(ref webCam);
            //設定網路攝影機影像寬高為640x480
            Setsize(wid, hei);
            webCam.FlipHorizontal = false;
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
            _barcodeReader = new BarcodeReader();
            haarCascade = new HaarCascade(@"haarcascade_frontalface_alt_tree.xml");

            comboBox3.Items.Add("MJPG");
            comboBox3.Items.Add("YUYV");
            comboBox3.SelectedIndex = 1;
            //建立系統閒置處理程序
            Application.Idle += Application_Idle;
        }


        private void Application_Idle(Object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            if (pause == false)
            {
                pictureBox1.Image = webCam.QueryFrame().ToBitmap();
                //Image<Ycc, Byte> YcrCbFrame = webCam.QueryFrame().Convert<Ycc, Byte>();
                Image<Bgr, Byte> Frame = webCam.QueryFrame();
                if (radioButton1.Checked == true)
                {
                    //pictureBox1.Image = BgrFrame.ToBitmap();
                    ReadBarcode(Frame.ToBitmap());
                }
                else if (radioButton2.Checked == true)
                {
                    FaceDetect();
                }
            }
            sw.Stop();
            label3.Text = "FPS: " + Math.Round((1000 / sw.Elapsed.TotalMilliseconds), 1).ToString();
        }

        private void ReadBarcode(Bitmap bitmap)
        {
            ZXing.Result result = _barcodeReader.Decode(bitmap);
            if (result != null)
            {
                string rs = null;
                rs += result.Text;

                Form2 f = new Form2(rs);
                pause = true;//暫停擷取影像

                while (f.ShowDialog() != DialogResult.OK)
                {
                    //do nothing
                    //等待使用者關閉Form2
                }
                pause = false;
            }
        }

        private void FaceDetect()
        {
            Image<Bgr, Byte> BgrFrame = webCam.QueryFrame();
            Image<Bgr, Byte> SmallFrame = webCam.QuerySmallFrame();
            //Image<Ycc, Byte> YcrCbFrame = BgrFrame.Convert<Ycc, Byte>();

            if (BgrFrame != null)
            {
                Image<Gray, Byte> grayFrame = SmallFrame.Convert<Gray, Byte>();
                var detectedFaces = grayFrame.DetectHaarCascade(haarCascade)[0];
                foreach (var face in detectedFaces)
                {
                    Rectangle ori_rect = new Rectangle(face.rect.X * 2, face.rect.Y * 2, face.rect.Width * 2, face.rect.Height * 2);
                    BgrFrame.Draw(ori_rect, new Bgr(0, 255, 255), 3);
                }
            }
            pictureBox1.Image = BgrFrame.ToBitmap();
        }



        private void LoadFrameSize(ref Emgu.CV.Capture cam)
        {
            comboBox2.Items.Clear();
            SortedSet<double> availwidth = new SortedSet<double>(); //all available capture width
            SortedSet<double> availheight = new SortedSet<double>(); // all available capture height
            /*640 x 480
              800 x 600
              1280 x 720
              1600 x 1200
              1920 x 1080
              2048 x 1536
              2592 x 1944
            */
            //設定解析度
            int[] widthhh = { 160, 320, 480, 640, 800, 1280, 1600, 1920, 2048, 2592 };
            foreach (var w in widthhh)
            {
                cam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, w);
                availwidth.Add(cam.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH));
                availheight.Add(cam.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT));
            }

            List<string> rlist = new List<string>();

            foreach (var w in availwidth)
                rlist.Add(w.ToString());
            int a = 0;
            foreach (var h in availheight)
            {
                rlist[a] += "x" + h;
                a++;
            }
            foreach (var item in rlist)
                comboBox2.Items.Add(item);
        }
        private void button1_Click(object sender, EventArgs e)//選裝置
        {
            if (comboBox1.SelectedIndex == Sindex) return;
            pause = true;
            webCam = new Emgu.CV.Capture(comboBox1.SelectedIndex);
            Sindex = comboBox1.SelectedIndex;
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, wid);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, hei);
            comboBox2.Items.Clear();
            LoadFrameSize(ref webCam);
            Showcurrentframesize(ref webCam);
            pause = false;
            //LoadFrameSize(webCam);
        }

        private void Showcurrentframesize(ref Emgu.CV.Capture cam)
        {
            string w = cam.GetCaptureProperty((Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH)).ToString() + "x" + cam.GetCaptureProperty((Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT)).ToString();
            int index = 0;
            foreach (var i in comboBox2.Items)
            {
                if (w == i.ToString())
                {
                    comboBox2.SelectedIndex = index;
                    break;
                }
                else index++;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webCam.FlipHorizontal = !webCam.FlipHorizontal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        //拍照
        private void button3_Click(object sender, EventArgs e)//拍照
        {
            YcrCbFrame = webCam.QueryFrame().Convert<Ycc, UInt16>();
            BgrFrame = webCam.QueryFrame();
            pause = true;
            button5.Visible = true;
            button6.Visible = true;
            button3.Visible = false;
        }

        //儲存
        private void button5_Click(object sender, EventArgs e)
        {
            string savefilename = null;
            SaveFileDialog sf = new SaveFileDialog();
            sf.Title = "儲存影像";
            sf.Filter = "JPGE檔案(*.JPG)|*.jpg|Portable Network Graphic檔案(*.PNG)|*.PNG";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                savefilename = sf.FileName;
                if (comboBox3.SelectedIndex == 0) //ycc
                {
                    YcrCbFrame.Save(savefilename);
                }
                else
                {
                    BgrFrame.Save(savefilename);
                }
                MessageBox.Show("儲存成功");
            }
            else MessageBox.Show("儲存失敗");
            button5.Visible = false;
            button6.Visible = false;
            button3.Visible = true;
            pause = false;
        }

        //取消
        private void button6_Click(object sender, EventArgs e)
        {
            button5.Visible = false;
            button6.Visible = false;
            button3.Visible = true;
            pause = false;
        }


        private void button4_Click(object sender, EventArgs e)//換解析度
        {
            string content = comboBox2.SelectedItem.ToString();
            wid = int.Parse(content.Substring(0, content.IndexOf("x")));
            hei = int.Parse(content.Substring(content.IndexOf("x") + 1, content.Length - content.IndexOf("x") - 1));
            //MessageBox.Show(wid + "x" + hei);
            Setsize(wid, hei);
        }

        private void Setsize(int w, int h)
        {
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, wid);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, hei);
            pictureBox1.Image = webCam.QueryFrame().ToBitmap();//只取一張圖，調整Form大小
            this.Width = pictureBox1.Width + 100;
            this.Height = pictureBox1.Height + tabControl1.Height + panel1.Height + panel2.Height + 150;
            label1.Text = "解析度: <" + webCam.Width.ToString() + "x" + webCam.Height.ToString() + ">";
            Showcurrentframesize(ref webCam);
        }

    }
}
