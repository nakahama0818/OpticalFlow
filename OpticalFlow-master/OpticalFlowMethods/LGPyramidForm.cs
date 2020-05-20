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
    public partial class LGPyramidForm : Form
    {
        private LG_Form main = null;
        public LGPyramidForm(Form call)
        {
            main = call as LG_Form;
            InitializeComponent();
            checkBox1.Checked = this.main.pyramidON;
            numericUpDown1.Visible = checkBox1.Checked;
            label1.Visible = checkBox1.Checked;
            numericUpDown1.Value = this.main.pyramidLevel;
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            this.main.pyramidLevel = (int)numericUpDown1.Value;
            this.main.pyramidON = checkBox1.Checked;
            this.Close();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Visible = checkBox1.Checked;
            label1.Visible = checkBox1.Checked;
        }
    }
}
