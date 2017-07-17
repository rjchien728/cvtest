﻿using System;
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
using DirectX.Capture;
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
        //-----
        //Filters filters = new Filters();
        //DirectX.Capture.Capture Dcapture = null;

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
            /*
            Dcapture = new DirectX.Capture.Capture(
            filters.VideoInputDevices[0], filters.AudioInputDevices[0]);

            label2.Text = Dcapture.FrameRate.ToString();

            Dcapture.VideoCompressor = filters.VideoCompressors[0];
            Dcapture.AudioCompressor = filters.AudioCompressors[0];

            Dcapture.FrameRate = 29.997;                 // NTSC
            Dcapture.FrameSize = new Size(640, 480);   // 640x480
            Dcapture.AudioSamplingRate = 44100;          // 44.1 kHz
            Dcapture.AudioSampleSize = 16;               // 16-bit
            Dcapture.AudioChannels = 1;                  // Mono

            //Dcapture.Filename = "C:\MyVideo.avi";

            Dcapture.Start();
            //Dcapture.Stop();
            */
            foreach (var c in _touch.Cameras)
                comboBox1.Items.Add(c);
            comboBox1.SelectedIndex = 0;//default
            //取得第一支網路攝影機
            webCam = new Emgu.CV.Capture(comboBox1.SelectedIndex);
            Sindex = comboBox1.SelectedIndex;

            //LoadFrameSize(webCam);//get availible resolution
            //設定網路攝影機影像寬高為640x480
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, wid);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, hei);
            webCam.FlipHorizontal = true;
            //webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 15);
            _barcodeReader = new BarcodeReader();
            haarCascade = new HaarCascade(@"haarcascade_frontalface_alt_tree.xml");
            label1.Text = "解析度: <" + webCam.Width.ToString() + "x" + webCam.Height.ToString() + ">";
            
            pictureBox1.Image = webCam.QueryFrame().ToBitmap();
            
            this.Width = pictureBox1.Width + 100;
            this.Height = pictureBox1.Height + tabControl1.Height + panel1.Height + panel2.Height + 150;
            //建立系統閒置處理程序
            Application.Idle += Application_Idle;

            /*
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Application_Idle);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();*/

        }


        private void Application_Idle(Object sender, EventArgs e)
        {
            if (pause == false)
            {
                pictureBox1.Image = webCam.QueryFrame().ToBitmap();
                if (radioButton1.Checked == true)
                {
                    ReadBarcode(webCam.QueryFrame().ToBitmap());
                }
                else if(radioButton2.Checked == true)
                {

                }
            }
        }



        private void ReadBarcode(Bitmap bitmap)
        {
            // Read barcodes with Dynamsoft Barcode Reader
            Stopwatch sw = Stopwatch.StartNew();
            //sw.Start();

            ZXing.Result result = _barcodeReader.Decode(bitmap);

            //sw.Stop();
            //label3.Text = sw.Elapsed.TotalMilliseconds + "ms";
            if (result != null)
            {

                string rs = null;
                rs += result.Text;

                Form2 f = new Form2(rs);
                pause = true;

                while (f.ShowDialog() != DialogResult.OK)
                {
                    //do nothing
                }
                pause = false;
            }
        }

        private void facedetect(Bitmap bitmap)
        {

        }



        private void LoadFrameSize (Emgu.CV.Capture cam)
        {
            comboBox2.Items.Clear();
            SortedSet<double> availwidth = new SortedSet<double>(); //all available capture width
            SortedSet<double> availheight = new SortedSet<double>(); // all available capture height

            /*
             * 640 x 480
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

            //string ss = null;
            foreach (var d in availwidth) comboBox2.Items.Add(d);//ss += d.ToString() + "\r\n";
            comboBox2.Items.Add("-----");
            foreach (var d in availheight) comboBox2.Items.Add(d);//ss += d.ToString() + "\r\n";
            //MessageBox.Show(ss);
        }
        private void button1_Click(object sender, EventArgs e)//選裝置
        {
            if (comboBox1.SelectedIndex == Sindex) return;
            pause = true;
            webCam = new Emgu.CV.Capture(comboBox1.SelectedIndex);
            Sindex = comboBox1.SelectedIndex;
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, wid);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, hei);
            pause = false;
            //LoadFrameSize(webCam);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webCam.FlipHorizontal = !webCam.FlipHorizontal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
