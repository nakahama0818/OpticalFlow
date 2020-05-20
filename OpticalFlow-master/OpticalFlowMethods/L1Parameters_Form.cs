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
    public partial class L1Parameters_Form : Form
    {
        private L1_Form main = null;
        public L1Parameters_Form(Form call)
        {
            main = call as L1_Form;
            InitializeComponent();
            numericUpDown1.Value = this.main.max_iteration;
            numericUpDown2.Value = this.main.warps;
            textBox1.Text = this.main.lambda;
            textBox3.Text = this.main.theta;
            textBox4.Text = this.main.tau;
            //textBox2.Text = this.main.eps;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.main.max_iteration = (int)numericUpDown1.Value;
            this.main.warps = (int)numericUpDown2.Value;
            this.main.lambda = textBox1.Text;
            this.main.theta = textBox3.Text;
            this.main.tau = textBox4.Text;
           // this.main.eps = textBox2.Text;
            this.Close();

        }
    }
}
