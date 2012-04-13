namespace dcpu16_ASM.Winforms
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadCompiledBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asemblerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutDCPU16ASMNETToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SetCpuFreqLabel = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.CpuFreqLabel = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.cycleCounttextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RegAtextBox = new System.Windows.Forms.TextBox();
            this.RegBtextBox = new System.Windows.Forms.TextBox();
            this.RegJtextBox = new System.Windows.Forms.TextBox();
            this.RegCtextBox = new System.Windows.Forms.TextBox();
            this.RegItextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.RegZtextBox = new System.Windows.Forms.TextBox();
            this.RegXtextBox = new System.Windows.Forms.TextBox();
            this.RegYtextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.RegOtextBox = new System.Windows.Forms.TextBox();
            this.RegSPtextBox = new System.Windows.Forms.TextBox();
            this.RegPCtextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BinaryDumptextBox = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.CycleTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.asemblerToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(856, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadCompiledBinaryToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadCompiledBinaryToolStripMenuItem
            // 
            this.loadCompiledBinaryToolStripMenuItem.Name = "loadCompiledBinaryToolStripMenuItem";
            this.loadCompiledBinaryToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.loadCompiledBinaryToolStripMenuItem.Text = "&Load Compiled Binary";
            this.loadCompiledBinaryToolStripMenuItem.Click += new System.EventHandler(this.loadCompiledBinaryToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // asemblerToolStripMenuItem
            // 
            this.asemblerToolStripMenuItem.Name = "asemblerToolStripMenuItem";
            this.asemblerToolStripMenuItem.Size = new System.Drawing.Size(74, 20);
            this.asemblerToolStripMenuItem.Text = "&Assembler";
            this.asemblerToolStripMenuItem.Click += new System.EventHandler(this.asemblerToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutDCPU16ASMNETToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutDCPU16ASMNETToolStripMenuItem
            // 
            this.aboutDCPU16ASMNETToolStripMenuItem.Name = "aboutDCPU16ASMNETToolStripMenuItem";
            this.aboutDCPU16ASMNETToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.aboutDCPU16ASMNETToolStripMenuItem.Text = "A&bout DCPU-16 ASM.NET";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "DCPU Binary File (.bin)|*.bin|DCPU Binary File (.obj)|*.obj|All files|*.*";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SetCpuFreqLabel);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.trackBar1);
            this.groupBox1.Controls.Add(this.CpuFreqLabel);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.cycleCounttextBox);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.RegOtextBox);
            this.groupBox1.Controls.Add(this.RegSPtextBox);
            this.groupBox1.Controls.Add(this.RegPCtextBox);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(856, 170);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DCPU-16 Registers and Cycle Counter";
            // 
            // SetCpuFreqLabel
            // 
            this.SetCpuFreqLabel.AutoSize = true;
            this.SetCpuFreqLabel.Location = new System.Drawing.Point(742, 20);
            this.SetCpuFreqLabel.Name = "SetCpuFreqLabel";
            this.SetCpuFreqLabel.Size = new System.Drawing.Size(46, 13);
            this.SetCpuFreqLabel.TabIndex = 40;
            this.SetCpuFreqLabel.Text = "100 Khz";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(661, 20);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(84, 13);
            this.label15.TabIndex = 39;
            this.label15.Text = "Set Target Freq:";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(650, 43);
            this.trackBar1.Maximum = 1200;
            this.trackBar1.Minimum = 25;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(178, 45);
            this.trackBar1.TabIndex = 38;
            this.trackBar1.Value = 100;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // CpuFreqLabel
            // 
            this.CpuFreqLabel.AutoSize = true;
            this.CpuFreqLabel.Location = new System.Drawing.Point(725, 106);
            this.CpuFreqLabel.Name = "CpuFreqLabel";
            this.CpuFreqLabel.Size = new System.Drawing.Size(34, 13);
            this.CpuFreqLabel.TabIndex = 37;
            this.CpuFreqLabel.Text = "0 Khz";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(661, 106);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 13);
            this.label14.TabIndex = 36;
            this.label14.Text = "Cpu Freq:";
            // 
            // cycleCounttextBox
            // 
            this.cycleCounttextBox.Location = new System.Drawing.Point(728, 127);
            this.cycleCounttextBox.Name = "cycleCounttextBox";
            this.cycleCounttextBox.ReadOnly = true;
            this.cycleCounttextBox.Size = new System.Drawing.Size(100, 20);
            this.cycleCounttextBox.TabIndex = 35;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(647, 130);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 13);
            this.label13.TabIndex = 34;
            this.label13.Text = "Cycle Count:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.RegAtextBox);
            this.groupBox4.Controls.Add(this.RegBtextBox);
            this.groupBox4.Controls.Add(this.RegJtextBox);
            this.groupBox4.Controls.Add(this.RegCtextBox);
            this.groupBox4.Controls.Add(this.RegItextBox);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.RegZtextBox);
            this.groupBox4.Controls.Add(this.RegXtextBox);
            this.groupBox4.Controls.Add(this.RegYtextBox);
            this.groupBox4.Location = new System.Drawing.Point(12, 32);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(414, 100);
            this.groupBox4.TabIndex = 33;
            this.groupBox4.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "A: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "B: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "C:";
            // 
            // RegAtextBox
            // 
            this.RegAtextBox.Location = new System.Drawing.Point(32, 18);
            this.RegAtextBox.Name = "RegAtextBox";
            this.RegAtextBox.ReadOnly = true;
            this.RegAtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegAtextBox.TabIndex = 11;
            // 
            // RegBtextBox
            // 
            this.RegBtextBox.Location = new System.Drawing.Point(32, 43);
            this.RegBtextBox.Name = "RegBtextBox";
            this.RegBtextBox.ReadOnly = true;
            this.RegBtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegBtextBox.TabIndex = 12;
            // 
            // RegJtextBox
            // 
            this.RegJtextBox.Location = new System.Drawing.Point(296, 67);
            this.RegJtextBox.Name = "RegJtextBox";
            this.RegJtextBox.ReadOnly = true;
            this.RegJtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegJtextBox.TabIndex = 25;
            // 
            // RegCtextBox
            // 
            this.RegCtextBox.Location = new System.Drawing.Point(32, 67);
            this.RegCtextBox.Name = "RegCtextBox";
            this.RegCtextBox.ReadOnly = true;
            this.RegCtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegCtextBox.TabIndex = 13;
            // 
            // RegItextBox
            // 
            this.RegItextBox.Location = new System.Drawing.Point(296, 18);
            this.RegItextBox.Name = "RegItextBox";
            this.RegItextBox.ReadOnly = true;
            this.RegItextBox.Size = new System.Drawing.Size(100, 20);
            this.RegItextBox.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(138, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "X: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(270, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(15, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "J:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(138, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Y: ";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(270, 21);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(16, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "I: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(138, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Z:";
            // 
            // RegZtextBox
            // 
            this.RegZtextBox.Location = new System.Drawing.Point(164, 67);
            this.RegZtextBox.Name = "RegZtextBox";
            this.RegZtextBox.ReadOnly = true;
            this.RegZtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegZtextBox.TabIndex = 19;
            // 
            // RegXtextBox
            // 
            this.RegXtextBox.Location = new System.Drawing.Point(164, 18);
            this.RegXtextBox.Name = "RegXtextBox";
            this.RegXtextBox.ReadOnly = true;
            this.RegXtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegXtextBox.TabIndex = 17;
            // 
            // RegYtextBox
            // 
            this.RegYtextBox.Location = new System.Drawing.Point(164, 43);
            this.RegYtextBox.Name = "RegYtextBox";
            this.RegYtextBox.ReadOnly = true;
            this.RegYtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegYtextBox.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(133, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "General Purpose Registers";
            // 
            // RegOtextBox
            // 
            this.RegOtextBox.Location = new System.Drawing.Point(527, 96);
            this.RegOtextBox.Name = "RegOtextBox";
            this.RegOtextBox.ReadOnly = true;
            this.RegOtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegOtextBox.TabIndex = 31;
            // 
            // RegSPtextBox
            // 
            this.RegSPtextBox.Location = new System.Drawing.Point(527, 72);
            this.RegSPtextBox.Name = "RegSPtextBox";
            this.RegSPtextBox.ReadOnly = true;
            this.RegSPtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegSPtextBox.TabIndex = 30;
            // 
            // RegPCtextBox
            // 
            this.RegPCtextBox.Location = new System.Drawing.Point(527, 47);
            this.RegPCtextBox.Name = "RegPCtextBox";
            this.RegPCtextBox.ReadOnly = true;
            this.RegPCtextBox.Size = new System.Drawing.Size(100, 20);
            this.RegPCtextBox.TabIndex = 29;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(469, 99);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(52, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "Overflow:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(447, 75);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Stack Pointer:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(432, 50);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Program Counter:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(0, 471);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(856, 81);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Name me good";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(206, 38);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "Reset";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(125, 38);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(44, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run Program";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BinaryDumptextBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox3.Location = new System.Drawing.Point(527, 194);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(329, 277);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "DCPU-16 Binary Dump";
            // 
            // BinaryDumptextBox
            // 
            this.BinaryDumptextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BinaryDumptextBox.Location = new System.Drawing.Point(3, 16);
            this.BinaryDumptextBox.MaxLength = 2000000;
            this.BinaryDumptextBox.Multiline = true;
            this.BinaryDumptextBox.Name = "BinaryDumptextBox";
            this.BinaryDumptextBox.ReadOnly = true;
            this.BinaryDumptextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.BinaryDumptextBox.Size = new System.Drawing.Size(323, 258);
            this.BinaryDumptextBox.TabIndex = 0;
            this.BinaryDumptextBox.WordWrap = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 16;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CycleTimer
            // 
            this.CycleTimer.Interval = 1000;
            this.CycleTimer.Tick += new System.EventHandler(this.CycleTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 552);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DCPU-16 ASM.NET Emulator";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCompiledBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asemblerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutDCPU16ASMNETToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox RegCtextBox;
        private System.Windows.Forms.TextBox RegBtextBox;
        private System.Windows.Forms.TextBox RegAtextBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox RegJtextBox;
        private System.Windows.Forms.TextBox RegItextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox RegZtextBox;
        private System.Windows.Forms.TextBox RegXtextBox;
        private System.Windows.Forms.TextBox RegYtextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox RegOtextBox;
        private System.Windows.Forms.TextBox RegSPtextBox;
        private System.Windows.Forms.TextBox RegPCtextBox;
        private System.Windows.Forms.TextBox BinaryDumptextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox cycleCounttextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label CpuFreqLabel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Timer CycleTimer;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label SetCpuFreqLabel;
        private System.Windows.Forms.Label label15;
    }
}