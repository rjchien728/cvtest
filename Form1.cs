using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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
//using TouchlessLib;
//using DirectX.Capture;
using Camera_NET;
using DirectShowLib;

namespace cvtest
{
    public partial class Form1 : Form
    {

        Timer _timer;

        //Emgu.CV.Capture webCam;

        CameraChoice _cameraChoice;
        System.Runtime.InteropServices.ComTypes.IMoniker _moniker;
        CameraControl _cameraControl;
        ResolutionList _resolutions;
        private BarcodeReader _barcodeReader;
        private HaarCascade haarCascade;
        bool pause = false;
        //private TouchlessMgr _touch;
        int wid = 640;//capture width
        int hei = 480;//capture height
        int fps;
        double totalsecond = 0;
        Queue<double> frate = new Queue<double>();//算平均
        int Sindex;
        //bool rloadyet = false; 

        Image<Ycc, Byte> _YcrCbFrame = null;
        Image<Bgr, Byte> _BgrFrame = null;
        VideoWriter videowriter1;
        string videoname;
        string tempdic = @"C:\ProgramData\webcam\";
        bool isrecording = false;
        Stopwatch sw;

        public Form1()
        {
            InitializeComponent();
            _cameraChoice = new CameraChoice();
            _cameraControl = new CameraControl();
        }

        private void resetCamera(CameraChoice cchoice, ref CameraControl ccontrol)
        {
            //cchoice = new CameraChoice();
            //ccontrol = null;
            //ccontrol = new CameraControl();
            //_cameraControl = ccontrol;
            _cameraControl.SetCamera(_moniker, null);
            //readResolution();
        }

        private void readResolution()
        {
            _cameraChoice.UpdateDeviceList();
            if (_cameraChoice.Devices.Count < 1)
            {
                MessageBox.Show("No webcam detected. Please check and restart the application.");
                Close();
                Environment.Exit(Environment.ExitCode);
            }

            foreach (var c in _cameraChoice.Devices)
                comboBox1.Items.Add(c.Name);
            comboBox1.SelectedIndex = 0;//default
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string defaultpath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            textBox1.Text = defaultpath;
            textBox2.Text = defaultpath;

            readResolution();
            //取得第一支網路攝影機
            Sindex = comboBox1.SelectedIndex;
            //抓取解析度
            //LoadFrameSize(ref webCam);
            _moniker = _cameraChoice.Devices[Sindex].Mon;//for getting resolution
            getResolution(_moniker);
            _cameraControl.SetCamera(_moniker, null);
            comboBox2.SelectedIndex = 0;
            label1.Text = "解析度: <" + comboBox2.SelectedItem + ">";
            _barcodeReader = new BarcodeReader();
            haarCascade = new HaarCascade(@"haarcascade_frontalface_alt_tree.xml");


            comboBox3.Items.Add("MJPG");
            comboBox3.Items.Add("YUYV");
            comboBox3.SelectedIndex = 1;
            //建立系統閒置處理程序
            //Application.Idle += Application_Idle;

            startCapture();
        }

        private void startCapture()
        {
            _timer = new Timer();
            _timer.Interval = 30; //30毫秒更新一次
            _timer.Tick += new EventHandler(TimerEventProcessor);
            _timer.Start();
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            sw = Stopwatch.StartNew();
            sw.Start();
            Image<Bgr, Byte> Frame = null;
            if (pause == false)
            {
                Frame = new Image<Bgr, Byte>(_cameraControl.SnapshotOutputImage());
                if (isrecording)//錄影
                {
                    pictureBox1.Image = Frame.ToBitmap();
                    videowriter1.WriteFrame<Bgr, Byte>(Frame);
                }
                else
                {
                    if (radioButton1.Checked == true)
                    {
                        pictureBox1.Image = Frame.ToBitmap();
                        ReadBarcode(Frame.ToBitmap());
                    }
                    else if (radioButton2.Checked == true)
                    {
                        FaceDetect(Frame);
                    }
                    else
                    {
                        pictureBox1.Image = Frame.ToBitmap();
                    }
                }
            }
            sw.Stop();
            double f = 1000 / sw.Elapsed.TotalMilliseconds;
            try
            {
                fps = Convert.ToInt32(f);
                if (frate.Count >= 5)
                {
                    label3.Text = "FPS: " + Math.Round(totalsecond / frate.Count, 1).ToString();
                    totalsecond = 0;
                    frate.Clear();
                }
                frate.Enqueue(f);
                totalsecond += f;
            }
            catch { fps = 0; }

            if (Frame != null)
                Frame.Dispose();
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

        private void FaceDetect(Image<Bgr, Byte> BgrFrame)
        {
            int targetW = 320;
            int targetH = _height / (_width / targetW);
            double shrinkRatio = _width / targetW;
            Image<Bgr, Byte> SmallFrame = new Image<Bgr, byte>(ScaleImage(BgrFrame.ToBitmap(), targetW, targetH));
            if (BgrFrame != null)
            {
                Image<Gray, Byte> grayFrame = SmallFrame.Convert<Gray, Byte>();
                var detectedFaces = grayFrame.DetectHaarCascade(haarCascade)[0];
                foreach (var face in detectedFaces)
                {
                    Rectangle ori_rect = new Rectangle((int)(face.rect.X * shrinkRatio), (int)(face.rect.Y * shrinkRatio), (int)(face.rect.Width * shrinkRatio), (int)(face.rect.Height * shrinkRatio));
                    BgrFrame.Draw(ori_rect, new Bgr(0, 255, 255), 3);
                }
            }
            pictureBox1.Image = BgrFrame.ToBitmap();
        }

        private void getResolution(System.Runtime.InteropServices.ComTypes.IMoniker moniker)
        {
            if (_resolutions != null)
                _resolutions.Clear();
            _resolutions = Camera_NET.Camera.GetResolutionList(moniker);
            comboBox2.Items.Clear();
            foreach (var r in _resolutions)
            {
                comboBox2.Items.Add(r);
            }
            comboBox2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)//選裝置
        {
            if (comboBox1.SelectedIndex == Sindex) return;
            pause = true;
            _moniker = _cameraChoice.Devices[comboBox1.SelectedIndex].Mon;
            Sindex = comboBox1.SelectedIndex;
            getResolution(_moniker);
            resetCamera(_cameraChoice, ref _cameraControl);
            pause = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //webCam.FlipHorizontal = !webCam.FlipHorizontal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        //拍照
        private void button3_Click(object sender, EventArgs e)//拍照
        {
            _YcrCbFrame = new Image<Ycc, byte>(_cameraControl.SnapshotSourceImage());//webCam.QueryFrame().Convert<Ycc, Byte>();
            _BgrFrame = new Image<Bgr, byte>(_cameraControl.SnapshotSourceImage());//webCam.QueryFrame();
            pause = true;
            button5.Visible = true;
            button6.Visible = true;
            button3.Visible = false;
        }

        //儲存
        private void button5_Click(object sender, EventArgs e)
        {
            string savefilename = null;
            //SaveFileDialog sf = new SaveFileDialog();
            //sf.Title = "儲存影像";
            //sf.Filter = "JPGE檔案(*.JPG)|*.jpg|Portable Network Graphic檔案(*.PNG)|*.PNG";
            //if (sf.ShowDialog() == DialogResult.OK)
            //{
            savefilename = textBox1.Text + @"\" + DateTime.Now.ToString("yyyyMMddHmmss") + @".JPG";
            if (comboBox3.SelectedIndex == 0) //ycc
            {
                _YcrCbFrame.Save(savefilename);
            }
            else
            {
                _BgrFrame.Save(savefilename);
            }
            //    MessageBox.Show("儲存成功");
            //}
            //else MessageBox.Show("儲存失敗");
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
            button4.Enabled = false;
            label7.Visible = false;
            string content = comboBox2.SelectedItem.ToString();
            wid = int.Parse(content.Substring(0, content.IndexOf("x")));
            hei = int.Parse(content.Substring(content.IndexOf("x") + 1, content.Length - content.IndexOf("x") - 1));
            //MessageBox.Show(wid + "x" + hei);
            Setsize();
        }

        private void Setsize()
        {
            try
            {
                _timer.Stop();
                resetCamera(_cameraChoice, ref _cameraControl);
                System.Threading.Thread.Sleep((int)1000);

                //pause = true;
                Camera_NET.Resolution r = _resolutions[comboBox2.SelectedIndex];
                _cameraControl.SetCamera(_moniker, r);
                pictureBox1.Image = _cameraControl.SnapshotOutputImage();//只取一張圖，調整Form大小
                this.Width = pictureBox1.Width + 100;
                this.Height = pictureBox1.Height + tabControl1.Height + panel1.Height + panel2.Height + 150;
                label1.Text = "解析度: <" + comboBox2.SelectedItem + ">";
                //pause = false;
                startCapture();
            }
            catch
            {
                pictureBox1.Image = null;
                label7.Location = new Point(pictureBox1.Location.X + ((pictureBox1.Width - label7.Width) / 2), pictureBox1.Location.Y + ((pictureBox1.Height - label7.Height) / 2));
                label7.Visible = true;
            }
            finally
            {
                button4.Enabled = true;
            }
        }

        //錄影
        private void button7_Click(object sender, EventArgs e)
        {
            if (isrecording == false)
            {
                if (!System.IO.Directory.Exists(tempdic))
                    System.IO.Directory.CreateDirectory(tempdic);
                videoname = string.Format("{0}{1}{2}", tempdic, DateTime.Now.ToString("yyyyMMddHmmss"), ".avi");
                videowriter1 = new VideoWriter(videoname, 0, fps, wid, hei, true);
                isrecording = true;
                button7.Text = "停止錄影";
                ReverseEnable();
            }
            else
            {
                isrecording = false;
                videowriter1.Dispose();//刪除videowriter物件
                button7.Text = "開始錄影";
                ReverseEnable();
                //存檔
                DialogResult dialogResult = MessageBox.Show("是否保存影片?", "FACE_CODE", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    System.IO.File.Copy(videoname, textBox2.Text + @"\" + DateTime.Now.ToString("yyyyMMddHmmss") + @".avi", true);
                    System.IO.File.Delete(videoname);

                }
                else if (dialogResult == DialogResult.No)
                {
                    System.IO.File.Delete(videoname);
                }
                //SaveFileDialog sf = new SaveFileDialog();
                //sf.Title = "儲存影片";
                //sf.Filter = "AVI檔案(*.AVI)|*.avi";
                //if (sf.ShowDialog() == DialogResult.OK)
                //{
                //    System.IO.File.Copy(videoname, sf.FileName, true);
                //    MessageBox.Show("儲存成功");
                //    System.IO.File.Delete(videoname);
                //}
                //else
                //{
                //    MessageBox.Show("儲存失敗");
                //    System.IO.File.Delete(videoname);
                //}


            }
        }

        private void ReverseEnable()
        {
            button1.Enabled = !button2.Enabled;
            button2.Enabled = !button2.Enabled;
            button3.Enabled = !button2.Enabled;
            button4.Enabled = !button2.Enabled;
            button5.Enabled = !button2.Enabled;
            button6.Enabled = !button2.Enabled;
            tabControl1.Enabled = !tabControl1.Enabled;
        }

        private static Bitmap ScaleImage(Bitmap pBmp, int pWidth, int pHeight)
        {
            try
            {
                Bitmap tmpBmp = new Bitmap(pWidth, pHeight);
                Graphics tmpG = Graphics.FromImage(tmpBmp);

                //tmpG.InterpolationMode = InterpolationMode.HighQualityBicubic;

                tmpG.DrawImage(pBmp,
                    new Rectangle(0, 0, pWidth, pHeight),
                    new Rectangle(0, 0, pBmp.Width, pBmp.Height),
                    GraphicsUnit.Pixel);
                tmpG.Dispose();
                return tmpBmp;
            }
            catch
            {
                return null;
            }
        }

        private int _width
        {
            get
            {
                string r = comboBox2.SelectedItem.ToString();
                return int.Parse(r.Substring(0, r.IndexOf("x")));
            }
        }
        private int _height
        {
            get
            {
                string r = comboBox2.SelectedItem.ToString();
                return int.Parse(r.Substring(r.IndexOf("x") + 1));
            }
        }

    }
}
