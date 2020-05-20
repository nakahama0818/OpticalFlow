using FlowProcessorAPI;
using System;
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
    public partial class L1_Form : Form
    {
        public string tau = "0.25";
        public string theta = "0.3";
        public string lambda = "0.15";
        public string flowinterval = "3.0";
        public string threshold = "0.5";
        public int max_iteration = 1;
        public int warps = 1;
       // public string eps = "0.01";
        public int pyramidLevel = 2;
        string videofilename;
        CLProcessor _CLProcessor = new CLProcessor();
        FileHandler _ImageFileHandler = new FileHandler();
        SimpleImage inputImage_1 = new SimpleImage();
        SimpleImage inputImage_2 = new SimpleImage();
        L1Flow L1flow = new L1Flow();
        FlowArray opticalFlowNext = new FlowArray();
 
    
        FlowArray flowError = new FlowArray();
        FlowArray opticalFlow = new FlowArray();
        public enum windowMode { IMAGES, VIDEO };
        enum Pyramid { PyramidOff, PyramidOn };
        Pyramid pyramid;
        windowMode mode;
        public bool pyramidON;
        public bool saveVideo = false;
        public float[] listEps;
        public ArrayList array = new ArrayList();
        Bitmap input1bitmap;
        Bitmap input2bitmap;
        VideoCapture capture;
        Mat image = new Mat();
        SimpleImage OutputImage = new SimpleImage();
        SimpleImage OutputVideo = new SimpleImage();
        bool videoSelected = false;
        string video;
        private Stopwatch sw = new Stopwatch();
        float runTime;

        public L1_Form()
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
                L1flow.maxiter = 1;
                pyramid = Pyramid.PyramidOff;
            }


            label2.Text = Convert.ToString(L1flow.lambda);
            label4.Text = Convert.ToString(L1flow.theta);
            label17.Text = Convert.ToString(L1flow.tau);
            label15.Text = Convert.ToString(L1flow.maxiter);
            label8.Text = Convert.ToString(L1flow.flowInterval);
              label6.Text = Convert.ToString(L1flow.threshold);
           // label14.Text = Convert.ToString(L1flow.epsilon);
            label19.Text = String.Format(Convert.ToString(L1flow.pyramidLevel));
            label10.Visible = false;

            label14.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(L1flow.threshold));
            label13.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(L1flow.threshold + L1flow.flowInterval));
            label12.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(L1flow.threshold + 2*L1flow.flowInterval));
            label11.Text = "Maximum value";
            // label11.Visible = false;
            // label12.Visible = false;
            if (pyramid == Pyramid.PyramidOff)
            {
                label9.Visible = false;
                label19.Visible = false;
            }

            else
            {
                label9.Visible = true;
                label19.Visible = true;
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
                L1opticalFlowforPictures();
            }

            if (pyramid == Pyramid.PyramidOn)
            {
                L1opticalFlowwithPyramid();
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
                L1PlayVideoForm setup = new L1PlayVideoForm(this);
                DialogResult dialogresult = setup.ShowDialog();
                setup.Dispose();

                if (saveVideo == false)
                {
                    calcL1_video();
                }


                else
                {
                    playVideowithSaving();
                }

            }
        
        }

        void L1opticalFlowforPictures()
        {

            int i = 0;

            opticalFlow.Height = inputImage_1.ImageHeight;
            opticalFlow.Width = inputImage_1.ImageWidth;
            opticalFlow.Array = new float[2][];
            opticalFlow.Array[0] = new float[opticalFlow.Width * opticalFlow.Height];
            opticalFlow.Array[1] = new float[opticalFlow.Width * opticalFlow.Height];

            sw.Start();
            opticalFlow = L1flow.TV_L1_opticalFlow(inputImage_1.Bitmap, inputImage_2.Bitmap, out i);
            sw.Stop();
            label10.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label10.Visible = true;
           float[] length = _CLProcessor.calcFlowLength(opticalFlow);
            label11.Text = String.Format("Max value: {0} [pixel/frame transition]", Convert.ToString(length.Max()));
            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, L1flow.flowInterval, L1flow.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();

           // label12.Text = String.Format(Convert.ToString(i));
            //label11.Visible = true;
            //label12.Visible = true;
            saveToolStripMenuItem.Enabled = true;
        }

        void L1opticalFlowwithPyramid()
        {
            sw.Start();
            opticalFlow = L1flow.TV_L1_pyramidical_opticalFlow(inputImage_1, inputImage_2);
            sw.Stop();
            label10.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label10.Visible = true;
            float[] length = _CLProcessor.calcFlowLength(opticalFlow);
           label11.Text = String.Format("Max value: {0} [pixel/frame transition]",Convert.ToString(length.Max()));

            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, L1flow.flowInterval, L1flow.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();
            saveToolStripMenuItem.Enabled = true;

        }


        private void calcL1_video()
        {

            
            if (videoSelected)
            {
                while (!(image.Empty()))
                {
                    input1bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                    sw.Start();
                    
                        opticalFlow =L1flow.TV_L1_opticalFlow(input1bitmap, input2bitmap, out int k);


            


                    OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, L1flow.flowInterval, L1flow.threshold);
                    pictureBox4.Image.Dispose();
                    pictureBox5.Image.Dispose();

                    pictureBox4.Image = OutputVideo.Bitmap;
                    pictureBox5.Image = input1bitmap;

                    Application.DoEvents();
                    pictureBox4.Refresh();
                    pictureBox5.Refresh();
                    sw.Stop();
                    runTime = 1 / (float)sw.Elapsed.TotalSeconds;
                    label10.Text = String.Format("Frame rate: {0} [fps]", Convert.ToString(runTime));
                    label10.Visible = true;
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
            L1Parameters_Form setup = new L1Parameters_Form(this);
            DialogResult dialogresult = setup.ShowDialog();

            L1flow.maxiter = max_iteration;

            L1flow.warps = warps;

            L1flow.lambda = float.Parse(lambda,
            System.Globalization.CultureInfo.InvariantCulture);

            L1flow.theta = float.Parse(theta,
           System.Globalization.CultureInfo.InvariantCulture);

            L1flow.tau = float.Parse(tau,
           System.Globalization.CultureInfo.InvariantCulture);

            

           // L1flow.epsilon = float.Parse(eps,
           //System.Globalization.CultureInfo.InvariantCulture);

            refreshWindow();

            setup.Dispose();


        }

        private void PyramidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            L1PyramidForm setup = new L1PyramidForm(this);
            DialogResult dialogresult = setup.ShowDialog();

            L1flow.pyramidLevel = pyramidLevel;


            if (pyramidON)
            {
                pyramid = Pyramid.PyramidOn;

            }

            else
            {
                pyramid = Pyramid.PyramidOff;

            }

            refreshWindow();

            setup.Dispose();
        }

        private void DisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            L1DisplayForm setup = new L1DisplayForm(this);
            DialogResult dialogresult = setup.ShowDialog();


            L1flow.flowInterval = float.Parse(flowinterval,
            System.Globalization.CultureInfo.InvariantCulture);

            L1flow.threshold = float.Parse(threshold,
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
                save.ImWrite(String.Format("{0}.JPG", imagePath));

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
                    opticalFlow = L1flow.TV_L1_opticalFlow(input1bitmap, input2bitmap, out int k);


                    }


                OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, L1flow.flowInterval, L1flow.threshold);
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

    private void L1_Form_Load(object sender, EventArgs e)
        {

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
