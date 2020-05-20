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
using System.IO;
namespace OpticalFlowMethods
{
    public partial class Test_Form : Form
    {

        FlowArray eval = new FlowArray();
      
        FlowArray groundtruth = new FlowArray();
        SimpleImage inputImage_1 = new SimpleImage();
        FileHandler fileHandler= new FileHandler();
        CLProcessor clprocessor = new CLProcessor();
        FlowArray length = new FlowArray();
        
        public Test_Form()
        {
            InitializeComponent();
            label2.Visible = false;
            label4.Visible = false;
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            string Path;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                Path = ofd.FileName;
                eval = fileHandler.convertFloToFlowArray(Path);
                

            }


        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string Path;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                Path = ofd.FileName;
                groundtruth = fileHandler.convertFloToFlowArray(Path);

               
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
           
           
         

            float[]length1 = clprocessor.calcFlowLength(groundtruth);
            float[] length2 = clprocessor.calcFlowLength(eval);
           

            float[] error = new float[eval.Height * eval.Width];
            float[] angularerror = new float[eval.Height * eval.Width];
            error = clprocessor.calcFlowDist(eval, groundtruth);
            angularerror = clprocessor.calcAngularError(eval, groundtruth);
            label2.Text = Convert.ToString(error.Average());
            label4.Text = Convert.ToString(angularerror.Average()*(180/Math.PI));

            label2.Visible = true;
            label4.Visible = true;



        }
    }
}
