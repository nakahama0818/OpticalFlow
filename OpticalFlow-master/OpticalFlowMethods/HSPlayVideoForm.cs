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
    public partial class HSPlayVideoForm : Form
    {
        private HS_Form main = null;
        public HSPlayVideoForm(Form call)
        {
            main = call as HS_Form;
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.main.saveVideo = true;
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.main.saveVideo = false;
            this.Close();
        }
    }
}
