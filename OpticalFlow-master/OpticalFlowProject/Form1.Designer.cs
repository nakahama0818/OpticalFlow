namespace OpticalFlowProject
{
    partial class OpticalFlowForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpticalFlowForm));
            this.DemonstratorPB = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slideShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowPairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webCamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PyramidSymbolPB = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.CalculationTimeText = new System.Windows.Forms.ToolStripStatusLabel();
            this.ShowNextImageButton = new System.Windows.Forms.Button();
            this.ShowPreviousImageButton = new System.Windows.Forms.Button();
            this.DecorateCheckBox = new System.Windows.Forms.CheckBox();
            this.EarlierRadioButton = new System.Windows.Forms.RadioButton();
            this.LaterRadioButton = new System.Windows.Forms.RadioButton();
            this.ModifiedImageRadioButton = new System.Windows.Forms.RadioButton();
            this.PyramidLevelUpDown = new System.Windows.Forms.NumericUpDown();
            this.PyramidLevelsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.DemonstratorPB)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PyramidSymbolPB)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PyramidLevelUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // DemonstratorPB
            // 
            this.DemonstratorPB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DemonstratorPB.Location = new System.Drawing.Point(13, 134);
            this.DemonstratorPB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DemonstratorPB.Name = "DemonstratorPB";
            this.DemonstratorPB.Size = new System.Drawing.Size(964, 463);
            this.DemonstratorPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.DemonstratorPB.TabIndex = 0;
            this.DemonstratorPB.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.displayToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(993, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageToolStripMenuItem1});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // imageToolStripMenuItem1
            // 
            this.imageToolStripMenuItem1.Name = "imageToolStripMenuItem1";
            this.imageToolStripMenuItem1.Size = new System.Drawing.Size(126, 26);
            this.imageToolStripMenuItem1.Text = "Image";
            this.imageToolStripMenuItem1.Click += new System.EventHandler(this.imageToolStripMenuItem1_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageToolStripMenuItem,
            this.flowToolStripMenuItem1});
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.imageToolStripMenuItem.Text = "Images";
            this.imageToolStripMenuItem.Click += new System.EventHandler(this.imageToolStripMenuItem_Click);
            // 
            // flowToolStripMenuItem1
            // 
            this.flowToolStripMenuItem1.Name = "flowToolStripMenuItem1";
            this.flowToolStripMenuItem1.Size = new System.Drawing.Size(216, 26);
            this.flowToolStripMenuItem1.Text = "Flow";
            this.flowToolStripMenuItem1.Click += new System.EventHandler(this.flowToolStripMenuItem1_Click);
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slideShowToolStripMenuItem,
            this.flowToolStripMenuItem,
            this.flowPairToolStripMenuItem,
            this.webCamToolStripMenuItem});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // slideShowToolStripMenuItem
            // 
            this.slideShowToolStripMenuItem.Name = "slideShowToolStripMenuItem";
            this.slideShowToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.slideShowToolStripMenuItem.Text = "SlideShow";
            this.slideShowToolStripMenuItem.Click += new System.EventHandler(this.slideShowToolStripMenuItem_Click);
            // 
            // flowToolStripMenuItem
            // 
            this.flowToolStripMenuItem.Name = "flowToolStripMenuItem";
            this.flowToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.flowToolStripMenuItem.Text = "Flow Slideshow";
            this.flowToolStripMenuItem.Click += new System.EventHandler(this.flowToolStripMenuItem_Click);
            // 
            // flowPairToolStripMenuItem
            // 
            this.flowPairToolStripMenuItem.Name = "flowPairToolStripMenuItem";
            this.flowPairToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.flowPairToolStripMenuItem.Text = "FlowPair";
            this.flowPairToolStripMenuItem.Click += new System.EventHandler(this.flowPairToolStripMenuItem_Click);
            // 
            // webCamToolStripMenuItem
            // 
            this.webCamToolStripMenuItem.Name = "webCamToolStripMenuItem";
            this.webCamToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.webCamToolStripMenuItem.Text = "WebCam";
            this.webCamToolStripMenuItem.Click += new System.EventHandler(this.webCamToolStripMenuItem_Click);
            // 
            // PyramidSymbolPB
            // 
            this.PyramidSymbolPB.Image = ((System.Drawing.Image)(resources.GetObject("PyramidSymbolPB.Image")));
            this.PyramidSymbolPB.InitialImage = ((System.Drawing.Image)(resources.GetObject("PyramidSymbolPB.InitialImage")));
            this.PyramidSymbolPB.Location = new System.Drawing.Point(0, 30);
            this.PyramidSymbolPB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PyramidSymbolPB.Name = "PyramidSymbolPB";
            this.PyramidSymbolPB.Size = new System.Drawing.Size(77, 71);
            this.PyramidSymbolPB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PyramidSymbolPB.TabIndex = 9;
            this.PyramidSymbolPB.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CalculationTimeText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 585);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.Size = new System.Drawing.Size(993, 25);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // CalculationTimeText
            // 
            this.CalculationTimeText.Name = "CalculationTimeText";
            this.CalculationTimeText.Size = new System.Drawing.Size(124, 20);
            this.CalculationTimeText.Text = "Calculation time: ";
            // 
            // ShowNextImageButton
            // 
            this.ShowNextImageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowNextImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ShowNextImageButton.Location = new System.Drawing.Point(941, 33);
            this.ShowNextImageButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ShowNextImageButton.Name = "ShowNextImageButton";
            this.ShowNextImageButton.Size = new System.Drawing.Size(36, 44);
            this.ShowNextImageButton.TabIndex = 8;
            this.ShowNextImageButton.Text = ">";
            this.ShowNextImageButton.UseVisualStyleBackColor = true;
            this.ShowNextImageButton.Click += new System.EventHandler(this.ShowNextImageButton_Click_1);
            // 
            // ShowPreviousImageButton
            // 
            this.ShowPreviousImageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowPreviousImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ShowPreviousImageButton.Location = new System.Drawing.Point(897, 33);
            this.ShowPreviousImageButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ShowPreviousImageButton.Name = "ShowPreviousImageButton";
            this.ShowPreviousImageButton.Size = new System.Drawing.Size(36, 44);
            this.ShowPreviousImageButton.TabIndex = 7;
            this.ShowPreviousImageButton.Text = "<";
            this.ShowPreviousImageButton.UseVisualStyleBackColor = true;
            this.ShowPreviousImageButton.Click += new System.EventHandler(this.ShowPreviousImageButton_Click_1);
            // 
            // DecorateCheckBox
            // 
            this.DecorateCheckBox.AutoSize = true;
            this.DecorateCheckBox.Checked = true;
            this.DecorateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DecorateCheckBox.Location = new System.Drawing.Point(723, 58);
            this.DecorateCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DecorateCheckBox.Name = "DecorateCheckBox";
            this.DecorateCheckBox.Size = new System.Drawing.Size(88, 21);
            this.DecorateCheckBox.TabIndex = 11;
            this.DecorateCheckBox.Text = "Decorate";
            this.DecorateCheckBox.UseVisualStyleBackColor = true;
            this.DecorateCheckBox.CheckedChanged += new System.EventHandler(this.DecorateCheckBox_CheckedChanged);
            // 
            // EarlierRadioButton
            // 
            this.EarlierRadioButton.AutoSize = true;
            this.EarlierRadioButton.Checked = true;
            this.EarlierRadioButton.Location = new System.Drawing.Point(641, 30);
            this.EarlierRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.EarlierRadioButton.Name = "EarlierRadioButton";
            this.EarlierRadioButton.Size = new System.Drawing.Size(70, 21);
            this.EarlierRadioButton.TabIndex = 12;
            this.EarlierRadioButton.TabStop = true;
            this.EarlierRadioButton.Text = "Earlier";
            this.EarlierRadioButton.UseVisualStyleBackColor = true;
            this.EarlierRadioButton.CheckedChanged += new System.EventHandler(this.EarlierRadioButton_CheckedChanged);
            // 
            // LaterRadioButton
            // 
            this.LaterRadioButton.AutoSize = true;
            this.LaterRadioButton.Location = new System.Drawing.Point(817, 30);
            this.LaterRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LaterRadioButton.Name = "LaterRadioButton";
            this.LaterRadioButton.Size = new System.Drawing.Size(62, 21);
            this.LaterRadioButton.TabIndex = 14;
            this.LaterRadioButton.TabStop = true;
            this.LaterRadioButton.Text = "Later";
            this.LaterRadioButton.UseVisualStyleBackColor = true;
            this.LaterRadioButton.CheckedChanged += new System.EventHandler(this.LaterRadioButton_CheckedChanged);
            // 
            // ModifiedImageRadioButton
            // 
            this.ModifiedImageRadioButton.AutoSize = true;
            this.ModifiedImageRadioButton.Location = new System.Drawing.Point(723, 30);
            this.ModifiedImageRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ModifiedImageRadioButton.Name = "ModifiedImageRadioButton";
            this.ModifiedImageRadioButton.Size = new System.Drawing.Size(82, 21);
            this.ModifiedImageRadioButton.TabIndex = 13;
            this.ModifiedImageRadioButton.TabStop = true;
            this.ModifiedImageRadioButton.Text = "Modified";
            this.ModifiedImageRadioButton.UseVisualStyleBackColor = true;
            this.ModifiedImageRadioButton.CheckedChanged += new System.EventHandler(this.ModifiedImageRadioButton_CheckedChanged);
            // 
            // PyramidLevelUpDown
            // 
            this.PyramidLevelUpDown.Location = new System.Drawing.Point(212, 32);
            this.PyramidLevelUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PyramidLevelUpDown.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.PyramidLevelUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.PyramidLevelUpDown.Name = "PyramidLevelUpDown";
            this.PyramidLevelUpDown.Size = new System.Drawing.Size(171, 22);
            this.PyramidLevelUpDown.TabIndex = 15;
            this.PyramidLevelUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.PyramidLevelUpDown.ValueChanged += new System.EventHandler(this.PyramidLevelUpDown_ValueChanged);
            // 
            // PyramidLevelsLabel
            // 
            this.PyramidLevelsLabel.AutoSize = true;
            this.PyramidLevelsLabel.Location = new System.Drawing.Point(85, 34);
            this.PyramidLevelsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PyramidLevelsLabel.Name = "PyramidLevelsLabel";
            this.PyramidLevelsLabel.Size = new System.Drawing.Size(100, 17);
            this.PyramidLevelsLabel.TabIndex = 16;
            this.PyramidLevelsLabel.Text = "PyramidLevels";
            // 
            // OpticalFlowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(993, 610);
            this.Controls.Add(this.PyramidLevelsLabel);
            this.Controls.Add(this.PyramidLevelUpDown);
            this.Controls.Add(this.ModifiedImageRadioButton);
            this.Controls.Add(this.LaterRadioButton);
            this.Controls.Add(this.EarlierRadioButton);
            this.Controls.Add(this.DecorateCheckBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.PyramidSymbolPB);
            this.Controls.Add(this.ShowNextImageButton);
            this.Controls.Add(this.ShowPreviousImageButton);
            this.Controls.Add(this.DemonstratorPB);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "OpticalFlowForm";
            this.Text = "Optical flow";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DemonstratorPB)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PyramidSymbolPB)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PyramidLevelUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox DemonstratorPB;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem slideShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flowPairToolStripMenuItem;
        private System.Windows.Forms.PictureBox PyramidSymbolPB;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flowToolStripMenuItem1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel CalculationTimeText;
        private System.Windows.Forms.Button ShowNextImageButton;
        private System.Windows.Forms.Button ShowPreviousImageButton;
        private System.Windows.Forms.ToolStripMenuItem webCamToolStripMenuItem;
        private System.Windows.Forms.CheckBox DecorateCheckBox;
        private System.Windows.Forms.RadioButton EarlierRadioButton;
        private System.Windows.Forms.RadioButton LaterRadioButton;
        private System.Windows.Forms.RadioButton ModifiedImageRadioButton;
        private System.Windows.Forms.NumericUpDown PyramidLevelUpDown;
        private System.Windows.Forms.Label PyramidLevelsLabel;
    }
}

