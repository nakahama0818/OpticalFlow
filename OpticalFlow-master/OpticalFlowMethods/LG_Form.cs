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
    public partial class LG_Form : Form
    {
        public string alpha = "5";
        public string sigma = "0";
        public string flowinterval = "3.0";
        public string threshold = "0.5";
        public int max_iteration = 1;
        public int kernelSize = 3;
        public string eps = "0.01";
        public int pyramidLevel = 5;
        
        CLProcessor _CLProcessor = new CLProcessor();
        FileHandler _ImageFileHandler = new FileHandler();
        SimpleImage inputImage_1 = new SimpleImage();
        SimpleImage inputImage_2 = new SimpleImage();
        LGFlow LGflow = new LGFlow();
        FlowArray opticalFlowNext = new FlowArray();
        FlowArray finalFlow = new FlowArray();
        FlowArray medFlow = new FlowArray();
        FlowArray tempFlow = new FlowArray();
        FlowArray flowError = new FlowArray();

        public enum windowMode { IMAGES, VIDEO };
        enum Pyramid { PyramidOff, PyramidOn };
        Pyramid pyramid;
        windowMode mode;
        public bool pyramidON;
        public bool saveVideo;
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
        string videofilename;
        FlowArray opticalFlow = new FlowArray();
        public LG_Form()
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
                LGflow.iteration = 1;
                pyramid = Pyramid.PyramidOff;
            }


            label2.Text = Convert.ToString(LGflow.alpha);
            label3.Text = Convert.ToString(LGflow.sigma);
            label15.Text = Convert.ToString(LGflow.iteration);
            label5.Text = Convert.ToString(LGflow.flowInterval);
            label7.Text = Convert.ToString(LGflow.threshold);
            label13.Text = Convert.ToString(LGflow.kernelSize);
            label10.Text = String.Format("Number of pyramid levels: {0}", Convert.ToString(LGflow.pyramidLevel));
            label20.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(LGflow.threshold));
            label19.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(LGflow.threshold + LGflow.flowInterval));
            label18.Text = String.Format("{0} [pixel/frame transition]", Convert.ToString(LGflow.threshold + 2*LGflow.flowInterval));
            label17.Text ="Maximum value";
            label9.Visible = false;
            label10.Visible = false;
          
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
                LGopticalFlowforPictures();
            }

            if (pyramid == Pyramid.PyramidOn)
            {
                LGopticalFlowwithPyramid();
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
                LGPlayVideoForm setup = new LGPlayVideoForm(this);
                DialogResult dialogresult = setup.ShowDialog();
                setup.Dispose();

                if (saveVideo == false)
                {
                    calcLG_video();
                }

                else
                {
                    playVideowithSaving();
                }
            }
        }
        void LGopticalFlowforPictures()
        {

            int i = 0;
            sw.Start();
            opticalFlow = LGflow.calcOpticalFlow(inputImage_1.Bitmap, inputImage_2.Bitmap, out i);
            sw.Stop();
            label9.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label9.Visible = true;
            float[] length = _CLProcessor.calcFlowLength(opticalFlow);
            label17.Text = String.Format("Max value: {0} [pixel/frame transition]", Convert.ToString(length.Max()));
            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, LGflow.flowInterval, LGflow.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();
         

 
            saveToolStripMenuItem.Enabled = true;
        }

        void LGopticalFlowwithPyramid()
        {
            sw.Start();
            opticalFlow = LGflow.LG_pyramidical_opticalFlow(inputImage_1, inputImage_2);
            sw.Stop();
            label9.Text = String.Format("Run time: {0} [s]", Convert.ToString(sw.Elapsed.TotalSeconds));
            label9.Visible = true;
            float[] length = _CLProcessor.calcFlowLength(opticalFlow);
            label17.Text = String.Format("Max value: {0} [pixel/frame transition]", Convert.ToString(length.Max()));

            OutputImage = _CLProcessor.decorateFlowColor(inputImage_1.Bitmap, opticalFlow, LGflow.flowInterval, LGflow.threshold);
            pictureBox3.Image = OutputImage.Bitmap;
            sw.Reset();
            saveToolStripMenuItem.Enabled = true;

        }

        private void calcLG_video()
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

                    opticalFlow = LGflow.calcOpticalFlow(input1bitmap, input2bitmap, out int k);





                    OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, LGflow.flowInterval, LGflow.threshold);
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
            LGParameters_Form setup = new LGParameters_Form(this);
            DialogResult dialogresult = setup.ShowDialog();

            LGflow.iteration = max_iteration;

           
            LGflow.kernelSize = kernelSize;

            LGflow.alpha = float.Parse(alpha,
            System.Globalization.CultureInfo.InvariantCulture);

            LGflow.sigma = float.Parse(sigma,
           System.Globalization.CultureInfo.InvariantCulture);

          



            refreshWindow();

            setup.Dispose();
        }

        private void PyramidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LGPyramidForm setup = new LGPyramidForm(this);
            DialogResult dialogresult = setup.ShowDialog();

            LGflow.pyramidLevel = pyramidLevel;


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
            LGDisplayForm setup = new LGDisplayForm(this);
            DialogResult dialogresult = setup.ShowDialog();


            LGflow.flowInterval = float.Parse(flowinterval,
            System.Globalization.CultureInfo.InvariantCulture);

            LGflow.threshold = float.Parse(threshold,
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
                        opticalFlow = LGflow.calcOpticalFlow(input1bitmap, input2bitmap, out int k);


                    }


                    OutputVideo = _CLProcessor.decorateFlowColor(input1bitmap, opticalFlow, LGflow.flowInterval, LGflow.threshold);
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

        private void AsFloToolStripMenuItem_Click(object sender, EventArgs e)
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
