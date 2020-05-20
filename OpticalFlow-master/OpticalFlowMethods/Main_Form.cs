using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpticalFlowMethods
{
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
         
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            HS_Form hornSchunck = new HS_Form();
            DialogResult dialogresult = hornSchunck.ShowDialog();
            hornSchunck.Dispose();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            L1_Form L1opticalflow = new L1_Form();
            DialogResult dialogresult = L1opticalflow.ShowDialog();
            L1opticalflow.Dispose();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            LG_Form localglobalflow = new LG_Form();
            DialogResult dialogresult = localglobalflow.ShowDialog();
            localglobalflow.Dispose();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Test_Form test = new Test_Form();
            DialogResult dialogresult = test.ShowDialog();
            test.Dispose();
        }
    }
}
