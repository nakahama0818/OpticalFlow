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
    public partial class LGParameters_Form : Form
    {
        private LG_Form main = null;
        int prevValue;
        public LGParameters_Form(Form call)
        {
            main = call as LG_Form;
            InitializeComponent();
            numericUpDown2.Value = this.main.max_iteration;
            numericUpDown1.Value = this.main.kernelSize;
            textBox1.Text = this.main.alpha;
            textBox2.Text = this.main.sigma;
          
    
            prevValue = (int)numericUpDown1.Value;

           
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.main.max_iteration = (int)numericUpDown2.Value;
            this.main.kernelSize = (int)numericUpDown1.Value;
            this.main.alpha = textBox1.Text;
            this.main.sigma= textBox2.Text;
          
          
                this.Close();
            


        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if((prevValue==3)&&(numericUpDown1.Value==4))
            {
                numericUpDown1.Value = 5;

            }

            if ((prevValue == 5) && (numericUpDown1.Value == 6))
            {
                numericUpDown1.Value = 7;
            }

            if ((prevValue == 7) && (numericUpDown1.Value == 6))
            {
                numericUpDown1.Value = 5;
            }

            if ((prevValue == 5) && (numericUpDown1.Value == 4))
            {
                numericUpDown1.Value = 3;
            }

            prevValue = (int)numericUpDown1.Value;
        }



    }
}
