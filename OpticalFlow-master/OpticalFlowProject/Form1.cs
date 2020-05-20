//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using FlowProcessorAPI;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace OpticalFlowProject
{
    public partial class OpticalFlowForm : Form
    {
        #region private fields
        private CLProcessor _CLProcessor;
        private FileHandler _ImageFileHandler;
        private OpenFileDialog _OpenFileDialog;

        private Bitmap[] _BitmapArray;

        private KernelStrings[] _Kernels = { new KernelStrings("GradX.cl", "gradX") };

        private SimpleImage _LeftFlowImage;
        private SimpleImage _RightFlowImage;
        private SimpleImage _DIsplayedImage;
        private SimpleImage _ModifiedImage;
        private FlowArray _SingleFlow;

        private int _CurrentLeftPyramidIndex = 0;
        private const int __StandardPyramidLevel = 4;

        private Func<SimpleImage, SimpleImage , int, FlowArray > _CurrentLKImplementation;
        // Slide type
        private enum slideShowType {NoSlideshow, NormalSlideshow, OpticalFlowSlideShow, SingleOpticalFlowPair};
        private slideShowType _SlideType = slideShowType.OpticalFlowSlideShow;

        // webcam
        VideoCapture _Capture;
        Mat _CameraFrame;
        Bitmap _BeforeCameraImage;
        Bitmap _AfterCameraImage;
        private Thread _CameraThread;
        bool isCameraRunning = false;

        #endregion

        #region constructor
        public OpticalFlowForm()
        {
            InitializeComponent();
            _CLProcessor = new CLProcessor(_Kernels);
            _OpenFileDialog = new OpenFileDialog();
            _ImageFileHandler = new FileHandler();
            _CurrentLKImplementation = (SimpleImage left, SimpleImage right, int level) => _CLProcessor.CalculateAdvancedPyramidalLK(left, right, level);

            string carsImagePath1  = "C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\training\\image_2\\000006_10.png";
            string carsImagePath2  = "C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\training\\image_2\\000006_11.png";

            Bitmap leftInput = _ImageFileHandler.GetImageFromFile(carsImagePath1);
            Bitmap rightInput = _ImageFileHandler.GetImageFromFile(carsImagePath2);

            try
            {
                calcCurrentOpticalFlow(leftInput, rightInput, __StandardPyramidLevel);
                _SlideType = slideShowType.SingleOpticalFlowPair;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                if (MessageBox.Show(e.Message, "Process couldn't complete", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    this.Close();
                    return;
                }
                
                Console.WriteLine("Exception end");
            }

            DisplayImage();
        }
        #endregion

        #region handlerHelpers
        private bool openImageFromFile(out Bitmap resultImage)
        {
            bool result = false;
            resultImage = null;

            if (_OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imageFileName = _OpenFileDialog.FileName;
                    if (!string.IsNullOrEmpty(imageFileName))
                    {                     
                        resultImage = this._ImageFileHandler.GetImageFromFile(imageFileName);
                        result = true;
                    }
                    else
                    {
                        throw (new Exception("Couldn't load file"));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                                    $"Details:\n\n{ex.StackTrace}");
                }
            }

            return result;
        }

        private bool openCLSourceFromFile(out string path)
        {
            path = null;
            bool result = false;
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "(*.cl)|*.cl";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                path = openFile.FileName;
                result = true;
            }
            return result;
        }

        private bool DoSlideShowLoad()
        {
            if(this._BitmapArray != null && this._BitmapArray.Length > 0)
            {
                this._BitmapArray = null;
            }
            bool success = false;

            if (_OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imageFileName = _OpenFileDialog.FileName;

                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        _ImageFileHandler.GetImageSequenceFromFolder(out this._BitmapArray, imageFileName, "");
                        while (this._BitmapArray.Length == 0) ;
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                                    $"Details:\n\n{ex.StackTrace}");
                }
            }

            return success;
        }

        private void flowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _ImageFileHandler.WriteFlowArrayToFloFile(FlowArray.ExpandForEvaluation(_SingleFlow, _LeftFlowImage.ImageWidth, _LeftFlowImage.ImageHeight), saveFileDialog.FileName);
            }
        }

        private void ShowPreviousSlide()
        {
            if (_BitmapArray != null && _BitmapArray.Length > 0 && 0 <= (_CurrentLeftPyramidIndex - 1))
            {
                _CurrentLeftPyramidIndex--;

                if (_SlideType == slideShowType.NormalSlideshow)
                {
                    this.DemonstratorPB.Image = _BitmapArray[_CurrentLeftPyramidIndex];
                }
                else if (_SlideType == slideShowType.OpticalFlowSlideShow)
                {
                    calcCurrentOpticalFlow( _BitmapArray[_CurrentLeftPyramidIndex], _BitmapArray[_CurrentLeftPyramidIndex + 1] , (int)PyramidLevelUpDown.Value);

                    DisplayImage();
                }
            }
            else if(_LeftFlowImage != null && _RightFlowImage != null && _ModifiedImage != null)
            {

            }
        }

        private void DisplayImage()
        {
            if (EarlierRadioButton.Checked)
            {
                _DIsplayedImage = _LeftFlowImage;
            }
            else if (ModifiedImageRadioButton.Checked)
            {
                _DIsplayedImage = _ModifiedImage;
            }
            else
            {
                _DIsplayedImage = _RightFlowImage;
            }

            SimpleImage result = _DIsplayedImage;

            if (DecorateCheckBox.Checked)
            {
                result = _CLProcessor.Decorateflow(result, _SingleFlow);
            }
            DemonstratorPB.Image = result.Bitmap;
        }

        private void ShowNextSlide()
        {
            if (_BitmapArray != null && _BitmapArray.Length > 0 && _BitmapArray.Length > (_CurrentLeftPyramidIndex + 2))
            {
                _CurrentLeftPyramidIndex++;

                if (_SlideType == slideShowType.NormalSlideshow)
                {
                    this.DemonstratorPB.Image = _BitmapArray[_CurrentLeftPyramidIndex];
                }
                else if (_SlideType == slideShowType.OpticalFlowSlideShow)
                {
                    calcCurrentOpticalFlow(_BitmapArray[_CurrentLeftPyramidIndex], _BitmapArray[_CurrentLeftPyramidIndex + 1], (int)PyramidLevelUpDown.Value);

                    DisplayImage();
                }
            }
        }

        #endregion

        #region flow calculation
        private void calcCurrentOpticalFlow(Bitmap leftInput, Bitmap rightInput, int pyramidLevel)
        {
            CalcOpticalFlowBase(leftInput, rightInput, (SimpleImage left, SimpleImage right, int level) => _CurrentLKImplementation(left, right, level), pyramidLevel);
        }

        private void calcOPticalFlow(Bitmap leftInput, Bitmap rightInput, int pyramidLevel)
        {
            CalcOpticalFlowBase(leftInput, rightInput, (SimpleImage left, SimpleImage right, int level) => _CLProcessor.CalculatePyramidalLK(left, right, level), pyramidLevel);
        }

        private void CalcAdvanceOpticalFlow(Bitmap leftInput, Bitmap rightInput, int pyramidLevel)
        {
            CalcOpticalFlowBase(leftInput, rightInput, (SimpleImage a, SimpleImage b, int level) => _CLProcessor.CalculateAdvancedPyramidalLK(a, b, level), pyramidLevel);
        }

        private void CalcOpticalFlowBase(Bitmap leftInput, Bitmap rightInput, Func<SimpleImage, SimpleImage, int, FlowArray> flowCalcFunc, int pyramidLevel = 4)
        {
            SimpleImage left = new SimpleImage(leftInput);
            SimpleImage right = new SimpleImage(rightInput);


            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            _SingleFlow = flowCalcFunc(left, right, pyramidLevel);
            watch.Stop();

            this.CalculationTimeText.Text = watch.Elapsed.Milliseconds.ToString();

            SimpleImage demonstrator = _CLProcessor.ModifyImageWithFlow(left, null, _SingleFlow);

            _LeftFlowImage = left;
            _RightFlowImage = right;
            _DIsplayedImage = _LeftFlowImage;
            _ModifiedImage = demonstrator;
        }
        #endregion

        #region eventhandlers
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void slideShowToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (DoSlideShowLoad())
            {    
                this._SlideType = slideShowType.NormalSlideshow;
                this.ShowNextSlide();
            }

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '6')
            {
                this.ShowNextSlide();
            }
        }

        private void flowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (DoSlideShowLoad())
            //{
            //    if (_BitmapArray.Length > 1)
            //    {
            //        _SlideType = slideShowType.OpticalFlowSlideShow;
            //        calcCurrentOpticalFlow(_BitmapArray[0], _BitmapArray[1], (int)PyramidLevelUpDown.Value);
            //        DisplayImage();
            //    }
            //}

            Bitmap[] leftArray;
            Bitmap[] rightArray;
            _ImageFileHandler.GetImageSequenceFromFolder(out leftArray, "C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\testing\\image_2\\", "_10");
            _ImageFileHandler.GetImageSequenceFromFolder(out rightArray, "C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\testing\\image_2\\", "_11");
            Bitmap[] resultArray = new Bitmap[leftArray.Length];

            for (int i = 0; i < 200; i++)
            {
                calcCurrentOpticalFlow(leftArray[i], rightArray[i], 5);
                //_ImageFileHandler.WriteFlowArrayToFile(FlowArray.ExpandForEvaluation(_SingleFlow, _LeftFlowImage.BitmapWidth, _LeftFlowImage.BitmapHeight), "C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\devkit\\matlab\\flows\\");
                resultArray[i] = _ModifiedImage.Bitmap;
                CalculationTimeText.Text = i.ToString();
                resultArray[i].Save("C:\\Users\\Ricsi\\Desktop\\BME\\2019_20_1\\FlowDatasets\\KITTI\\training\\results\\" + i.ToString() + ".png");
            }
        }

        private void flowPairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _BitmapArray = null;
            Bitmap left;
            Bitmap right;
            if (openImageFromFile(out left))
            {
                if (openImageFromFile(out right))
                {
                    _CurrentLeftPyramidIndex = 0;
                    calcCurrentOpticalFlow(left, right, (int)PyramidLevelUpDown.Value);
                    EarlierRadioButton.Checked = true;
                    DisplayImage();
                }
            }
        }

        private void imageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Bitmap temp;
            if (this.openImageFromFile(out temp))
            {
                this.DemonstratorPB.Image = temp;
            }
        }

        private void ShowPreviousImageButton_Click_1(object sender, EventArgs e)
        {
            ShowPreviousSlide();
        }

        private void ShowNextImageButton_Click_1(object sender, EventArgs e)
        {
            ShowNextSlide();
        }

        private void EarlierRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _DIsplayedImage = _LeftFlowImage;
            DisplayImage();
        }
        private void LaterRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _DIsplayedImage = _RightFlowImage;
            DisplayImage();
        }
        private void ModifiedImageRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _DIsplayedImage = _ModifiedImage;
            DisplayImage();
        }
        private void DecorateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DisplayImage();
        }
        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string path = folderDialog.SelectedPath;
                _LeftFlowImage.Bitmap.Save(path + "\\Demo0.png");
                _ModifiedImage.Bitmap.Save(path + "\\Demo1.png");
                _RightFlowImage.Bitmap.Save(path + "\\Demo2.png");
                _CLProcessor.Decorateflow(_LeftFlowImage, _SingleFlow).Bitmap.Save(path + "\\DemoDecorated.png");
            }
        }
        private void PyramidLevelUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void webCamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isCameraRunning)
            {
                CaptureCamrea();
                isCameraRunning = true;
            }
            else
            {
                _Capture.Release();
                isCameraRunning = false;
            }
        }
        #endregion

        #region WebCam
        private void CaptureCamrea()
        {
            _CameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
            _CameraThread.Start();
        }

        private void CaptureCameraCallback()
        {

            _Capture = new VideoCapture(0);
            _Capture.Open(0);

            if (_Capture.IsOpened())
            {
                while (isCameraRunning)
                {
                    _CameraFrame = new Mat();
                    if (_Capture.Read(_CameraFrame))
                    {
                        try
                        {
                            _BeforeCameraImage = _AfterCameraImage;
                            _AfterCameraImage = BitmapConverter.ToBitmap(_CameraFrame);

                            if (_BeforeCameraImage != null)
                            {
                                calcCurrentOpticalFlow(_BeforeCameraImage, _AfterCameraImage, (int)PyramidLevelUpDown.Value);
                                //DemonstratorPB.Image.Dispose();
                                DisplayImage();
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Thrown by CaptureCameraCallback", e);
                        }
                    }
                }
            }
        }
        #endregion

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
