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
    public partial class HSParameters_Form : Form
    {
        private HS_Form main = null;
        public HSParameters_Form(Form call)
        {
            main = call as HS_Form;
            InitializeComponent();
            numericUpDown1.Value = this.main.max_iteration;
       
            textBox1.Text = this.main.alpha;
            textBox2.Text = this.main.eps;
        
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.main.max_iteration = (int)numericUpDown1.Value;
      
            this.main.alpha = textBox1.Text;
            this.main.eps = textBox2.Text;
      
            this.Close();
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
