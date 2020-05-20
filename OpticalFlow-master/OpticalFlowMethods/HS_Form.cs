using FlowProcessorAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCL.Net;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Collections;

namespace OpticalFlowMethods
{
    public partial class HS_Form : Form
    {

        public string alpha = "5";
        public string flowinterval = "3.0";
        public string threshold = "0.5";
        public string w = "0.5";
        public int max_iteration = 10;
        public int warps = 1;
        public int pyramidLevel = 2;
        public int realIteration;
        public string eps = "0.01";
        CLProcessor _CLProcessor = new CLProcessor();
        FileHandler _ImageFileHandler = new FileHandler();
        SimpleImage inputImage_1;
        SimpleImage inputImage_2;
        Horn_Schunck HornSchunck = new Horn_Schunck();
        FlowArray opticalFlowNext = new FlowArray();
        FlowArray finalFlow = new FlowArray();
        FlowArray medFlow = new FlowArray();
        FlowArray tempFlow = new FlowArray();
        FlowArray flowError = new FlowArray();
        FlowArray opticalFlow = new FlowArray();
        public enum windowMode { IMAGES, VIDEO };
        enum Pyramid { PyramidOff, PyramidOn };
        Pyramid pyramid;
        windowMode mode;
        public bool pyramidON;

        public float[] listEps;
        public ArrayList array = new ArrayList();
        Bitmap input1bitmap;
        Bitmap input2bitmap;
        VideoCapture capture;
        Mat image = new Mat();
        SimpleImage OutputImage = new SimpleImage();
        SimpleImage OutputVideo = new SimpleImage();
        bool videoSelected = false;
        string videofilename;
        string video;
        public bool saveVideo = false;
        private Stopwatch sw = new Stopwatch();
        float runTime;
        public HS_Form()
        {
            mode = windowMode.IMAGES;
            pyramid = Pyramid.PyramidOff;
            InitializeComponent();
            refreshWindow();
            pyramidON = false;
        }


        private void refreshWindow()
        {
            if (mode == windowMode.IMAGES)
            {
                button1.Visible = true;
                button2.Visible = true;
                button3.Visible = true;
                button4.Visible = false;
                button5.Visible = false;
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox3.Visible = true;
                pictureBox4.Visible = false;
                pictureBox5.Visible = false;
            }



            if (mode == windowMode.VIDEO)
            {
                button1.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = true;
                button5.Visible = true;
                pictureBox1.Visible = false;
                pictureBox2.Visible = false;
                pictureBox3.Visible = false;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                HornSchunck.maxIteration = 1;
                pyramid = Pyramid.PyramidOff;
            }


            label2.Text = Convert.ToString(HornSchunck.alpha);
            label3.Text = Convert.ToString(HornSchunck.maxIteration);
            label5.Text = Convert.ToString(HornSchunck.flowInterval);
            label7.Text = Convert.ToString(HornSchunck.threshold);
            label13.Text = Convert.ToString(HornSchunck.eps);
            label10.Text = String.Format("Number of pyramid levels: {0}", Convert.ToString(HornSchunck.pyramidLevel));
            label9.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
            label15.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(HornSchunck.threshold));
            label16.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(HornSchunck.threshold + HornSchunck.flowInterval));
            label17.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(HornSchunck.threshold + 2*HornSchunck.flowInterval));
            label18.Text = "Maximum value";

            if (pyramid == Pyramid.PyramidOff)
            {
                label10.Visible = false;
            }

            else
            {
                label10.Visible = true;
            }

            saveToolStripMenuItem.Enabled = false;

          
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imagePath;
                imagePath = ofd.FileName;
                Bitmap inputBitmap;
                inputBitmap = _ImageFileHandler.GetImageFromFile(imagePath);

                inputImage_1 = new SimpleImage(inputBitmap);
                pictureBox1.Image = inputImage_1.Bitmap;


            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imagePath;
                imagePath = ofd.FileName;

                Bitmap inputBitmap;
                inputBitmap = _ImageFileHandler.GetImageFromFile(imagePath);
                inputImage_2 = new SimpleImage(inputBitmap);
                pictureBox2.Image = inputImage_2.Bitmap;

            }



        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (pyramid == Pyramid.PyramidOff)
            {
                HSopticalFlowforPictures();
                
                
            }


            if (pyramid == Pyramid.PyramidOn)
            {
                HSopticalFlowwithPyramid();
            }

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                video = ofd.FileName;
                capture = new VideoCapture();
                image = new Mat();
                capture.Open(video);

                capture.Read(image);
                input1bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                capture.Read(image);
                input2bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);

                videoSelected = true;

                pictureBox4.Image = input1bitmap;
                pictureBox5.Image = input1bitmap;

            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (videoSelected)
            {
                HSPlayVideoForm setup = new HSPlayVideoForm(this);
                DialogResult dialogresult = setup.ShowDialog();
                setup.Dispose();

                if (saveVideo == false)
                {
                    calcHornSchunck_video();
                }

                else
                {
                    playVideowithSaving();
                }
            }
        }

        void HSopticalFlowforPictures()
        {

            int i = 0;
           
            sw.Start();
            opticalFlow = HornSchunck.calculateOpticalFlow(inputImage_1.Bitmap, inputImage_2.Bitmap, out i, null);
            sw.Stop();
           
     
            label9.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label9.Visible = true;
           float[] length = _CLProcessor.calcFlowLength(opticalFlow);   
          label18.Text = String.Format("Max value: {0} [pixel/frame transition]",Convert.ToString(length.Max()));

            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, HornSchunck.flowInterval, HornSchunck.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();
           
            label11.Text = String.Format(Convert.ToString(i));
            label11.Visible = true;
            label12.Visible = true;
            saveToolStripMenuItem.Enabled = true;
        }



        void HSopticalFlowwithPyramid()
        {
            sw.Start();
            opticalFlow = HornSchunck.HS_pyramidical_opticalFlow(inputImage_1, inputImage_2);
            sw.Stop();
            label9.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label9.Visible = true;
           float[] length = _CLProcessor.calcFlowLength(opticalFlow);
            label18.Text = String.Format("Max value: {0} [pixel/frame transition]", Convert.ToString(length.Max()));

            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, HornSchunck.flowInterval, HornSchunck.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();
            saveToolStripMenuItem.Enabled = true;

        }

        private void calcHornSchunck_video()
        {

            FlowArray opticalFlow = new FlowArray();
            opticalFlow.Height = input1bitmap.Height;
            opticalFlow.Width = input1bitmap.Width;
            opticalFlow.Array = new float[2][];
            opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
            opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];

            if (videoSelected)
            {
                while (!(image.Empty()))
                {
                    input1bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                    sw.Start();
                    for (int i = 0; i < 1; i++)
                    {
                        opticalFlow = HornSchunck.calculateOpticalFlow(input1bitmap, input2bitmap, out int k, opticalFlow);


                    }


                    OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, HornSchunck.flowInterval, HornSchunck.threshold);
                    pictureBox4.Image.Dispose();
                    pictureBox5.Image.Dispose();

                    pictureBox4.Image = OutputVideo.Bitmap;
                    pictureBox5.Image = input1bitmap;

                    Application.DoEvents();
                    pictureBox4.Refresh();
                    pictureBox5.Refresh();
                    sw.Stop();
                    runTime = 1 / (float)sw.Elapsed.TotalSeconds;
                    label9.Text = String.Format("Frame rate: {0} [fps]", Convert.ToString(runTime));
                    label9.Visible = true;
                    sw.Reset();
                    input2bitmap = input1bitmap;
                    capture.Read(image);


                }
            }
            videoSelected = false;
            capture.Dispose();
            image.Dispose();
            pictureBox4.Image.Dispose();
            pictureBox5.Image.Dispose();



        }

        private void ImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = windowMode.IMAGES;
            refreshWindow();

            pictureBox1.Image = null;

            pictureBox2.Image = null;

            pictureBox3.Image = null;
        }

        private void VideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = windowMode.VIDEO;
            refreshWindow();
        }

        private void ParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HSParameters_Form setup = new HSParameters_Form(this);
            DialogResult dialogresult = setup.ShowDialog();

            HornSchunck.maxIteration = max_iteration;
            HornSchunck.warps = warps;
            HornSchunck.alpha = float.Parse(alpha,
            System.Globalization.CultureInfo.InvariantCulture);

            HornSchunck.eps = float.Parse(eps,
           System.Globalization.CultureInfo.InvariantCulture);
            HornSchunck.w = float.Parse(w,
        System.Globalization.CultureInfo.InvariantCulture);


            refreshWindow();

            setup.Dispose();
        }

        private void PyramidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HSPyramidForm setup = new HSPyramidForm(this);
            DialogResult dialogresult = setup.ShowDialog();

            HornSchunck.pyramidLevel = pyramidLevel;


            if (pyramidON)
            {
                pyramid = Pyramid.PyramidOn;
                HornSchunck.pyramidOn = 1;
            }

            else
            {
                pyramid = Pyramid.PyramidOff;
                HornSchunck.pyramidOn =0;

            }
            

            refreshWindow();

            setup.Dispose();
        }

        private void DisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HSDisplayForm setup = new HSDisplayForm(this);
            DialogResult dialogresult = setup.ShowDialog();

            HornSchunck.flowInterval = float.Parse(flowinterval,
            System.Globalization.CultureInfo.InvariantCulture);

            HornSchunck.threshold = float.Parse(threshold,
           System.Globalization.CultureInfo.InvariantCulture);

            refreshWindow();

            setup.Dispose();

        }

       

        private void AsJPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mat save = new Mat();
            SaveFileDialog ofd = new SaveFileDialog();
            string imagePath;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               

                imagePath = ofd.FileName;

                save = OpenCvSharp.Extensions.BitmapConverter.ToMat(OutputImage.Bitmap);
                save.ImWrite(String.Format("{0}.JPG",imagePath));

            }
        }

        private void AsPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mat save = new Mat();
            SaveFileDialog ofd = new SaveFileDialog();
            string imagePath;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                imagePath = ofd.FileName;

                save = OpenCvSharp.Extensions.BitmapConverter.ToMat(OutputImage.Bitmap);
                save.ImWrite(String.Format("{0}.PNG", imagePath));

            }

        }

        private void playVideowithSaving()
        {
            Mat save = new Mat();
             FileDialog ofd = new SaveFileDialog();     
           
            string imagePath;

            OpenCvSharp.FourCC codec = OpenCvSharp.FourCC.Default;
            OpenCvSharp.CvSize size;
            size.Height = input1bitmap.Height;
            size.Width = input1bitmap.Width;          
            

            FlowArray opticalFlow = new FlowArray();
            opticalFlow.Height = input1bitmap.Height;
            opticalFlow.Width = input1bitmap.Width;
            opticalFlow.Array = new float[2][];
            opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
            opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                imagePath = ofd.FileName;

               

                videofilename = String.Format("{0}.avi", imagePath);
            }


            VideoWriter savevid = new VideoWriter();
            savevid.Open(videofilename, codec, 24, size, true);

            if (videoSelected)
                {
                    while (!(image.Empty()))
                    {
                        input1bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                        sw.Start();
                        for (int i = 0; i < 1; i++)
                        {
                            opticalFlow = HornSchunck.calculateOpticalFlow(input1bitmap, input2bitmap, out int k, opticalFlow);


                        }
                    

                    OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, HornSchunck.flowInterval, HornSchunck.threshold);
                        save = OpenCvSharp.Extensions.BitmapConverter.ToMat(OutputVideo.Bitmap);
                        
                        savevid.Write(save);
                        pictureBox4.Image.Dispose();
                        pictureBox5.Image.Dispose();

                        pictureBox4.Image = OutputVideo.Bitmap;
                        pictureBox5.Image = input1bitmap;

                        Application.DoEvents();
                        pictureBox4.Refresh();
                        pictureBox5.Refresh();
                        sw.Stop();
                        runTime = 1 / (float)sw.Elapsed.TotalSeconds;
                        label9.Text = String.Format("Frame rate: {0} [fps]", Convert.ToString(runTime));
                        label9.Visible = true;
                        sw.Reset();
                        input2bitmap = input1bitmap;
                        capture.Read(image);
                       


                    }
                }
                videoSelected = false;
                capture.Dispose();
                image.Dispose();
            savevid.Release();
                //savevid.Dispose();
                pictureBox4.Image.Dispose();
                pictureBox5.Image.Dispose();

            }

        private void AsfloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            string Path;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                Path = ofd.FileName;

                _ImageFileHandler.WriteFlowArrayToFloFile(opticalFlow, Path);


            }

        }
    }

    
}
