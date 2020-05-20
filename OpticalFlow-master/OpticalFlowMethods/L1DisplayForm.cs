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
    public partial class L1DisplayForm : Form
    {
        private L1_Form main = null;
        public L1DisplayForm(Form call)
        {
            main = call as L1_Form;
            InitializeComponent();
            textBox1.Text = this.main.flowinterval;
            textBox2.Text = this.main.threshold;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.main.flowinterval = textBox1.Text;
            this.main.threshold = textBox2.Text;
            this.Close();

        }
    }
}
