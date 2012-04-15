/**
 * DCPU-16 ASM.NET
 * Copyright (c) 2012 Tim "DensitY" Hancock (densitynz@orcon.net.nz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DCPU16_ASM.Emulator;

namespace DCPU16_ASM.Winforms
{

    public partial class MainForm : Form
    {
        // TODO: these need to be shifted to a better location.
        private const int ScreenTextWidth = 32;
        private const int ScreenTextHeight = 16;
        private const int FontWidth = 4;
        private const int FontHeight = 8;        

        private const int ScreenPixelWidth = ScreenTextWidth * FontWidth;
        private const int ScreenPixelHeight = ScreenTextHeight * FontHeight;

        /// <summary>
        /// DCPU local buffer. 
        /// 
        /// A copy of dcpu data currently running in the dcpu thread. updated 
        /// every 16ms if needed. 
        /// </summary>
        class cpuDoubleBuffer
        {
            /// <summary>
            /// DCPU memory
            /// </summary>
            public cpuMemory       Memory;
            /// <summary>
            /// DCPU Registers
            /// </summary>
            public cpuRegisters    Registers;
            /// <summary>
            /// Cyclce counter
            /// </summary>
            public long            CycleCount;

            /// <summary>
            /// Keyboard cylinder index
            /// </summary>
            public int             KeyIndex = 0;
        }

        /// <summary>
        /// DCPU-16 Emulator
        /// </summary>
        private CDCPU16Emulator m_DPUEmulator = new CDCPU16Emulator();

        /// <summary>
        /// DCPU-16 Local buffer
        /// </summary>
        private cpuDoubleBuffer m_CpuDoublebuffer = new cpuDoubleBuffer();  // TODO: support multiple cpu data.
        /// <summary>
        /// DCPU-16 buffer copy flag, set to true if we want our local buffer updated.
        /// </summary>
        private bool m_MakeCpuDoublebuffer = true;

        /// <summary>
        /// Cycle count on last cpu "frame" 
        /// </summary>
        private long m_lastCycleCount = 0;

        /// <summary>
        /// Cycle count on current cpu "frame"
        /// </summary>
        private long m_cycleCount = 0;

        /// <summary>
        /// Text mode Frame buffer
        /// </summary>
        private Bitmap m_FrameBuffer;
        /// <summary>
        /// Font texture buffer
        /// </summary>
        private Bitmap m_FontBuffer;

        /// <summary>
        /// Our text color map
        /// EGA color goodness
        /// </summary>
        private uint[] m_TextColorMap = 
        {
             0x101010, 
             0x1000AA, 
             0x10AA10, 
             0x10AAAA, 
             0xAA1010, 
             0xAA10AA, 
             0xAA5010,
             0xAAAAAA, 
             0x808080, 
             0x1010ff, 
             0x10FF10, 
             0x10FFFF, 
             0xFF1010, 
             0xFF10FF,
             0xFFFF10, 
             0xFFFFFF         
        };

        /// <summary>
        /// Constructor 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            m_DPUEmulator.OnExecutePostStepEvent += new OnExecutePostStepHandle(OnCpuExecutePostStep);
            m_DPUEmulator.OnErrorEvent += new OnErrorHandle(OnCpuError);
            
            // initialize buffer
            m_CpuDoublebuffer.Memory.RAM = new ushort[0x10000]; // TODO: Define memory size so we don't have to change this is 20 places
            m_CpuDoublebuffer.Registers.GP = new ushort[8];
            m_CpuDoublebuffer.CycleCount = 0;

            m_FrameBuffer = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
            LoadFontFromImagelist();

            checkBox1.AutoCheck = false;
            checkBox1.Click += new EventHandler(checkBox1_Click);
        }

        /// <summary>
        /// Load Font (Which is stored in FontimageList.Images[0]
        /// Into a simple int array buffer, as it'll be easier to use. 
        /// </summary>
        private void LoadFontFromImagelist()
        {
            m_FontBuffer = new Bitmap(FontimageList.Images[0]);
        }

        /// <summary>
        /// nasty clear framebuffer via destorying it <_<
        /// </summary>
        private void ResetFrameBuffer()
        {
            if (m_FrameBuffer != null)
            {
                m_FrameBuffer.Dispose();
                m_FrameBuffer = null;
            }
            m_FrameBuffer = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
        }
        /// <summary>
        /// Main Menu
        /// 
        /// File->exit menu Close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Main Menu
        /// file->Load Compiled Binary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadCompiledBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // open compiled binary
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string errorMessage = string.Empty;
                if (m_DPUEmulator.LoadProgram(openFileDialog1.FileName, out errorMessage) != true)
                {
                    MessageBox.Show(errorMessage);
                    return;
                }
                m_cycleCount = m_lastCycleCount = 0;
                m_CpuDoublebuffer.KeyIndex = 0;
                BinaryMemoryDump();
                ResetFrameBuffer();
            }
        }

        /// <summary>
        /// Main Menu
        /// Assembler option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void asemblerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssemblerForm asmDialog = new AssemblerForm();
            asmDialog.ShowDialog(this);
        }

        /// <summary>
        /// Start DCPU-16 Emulation button On click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.UseKeyboardRingBuffer = checkBox1.Checked;
            m_DPUEmulator.Start();
            CycleTimer.Enabled = true;
            m_CpuDoublebuffer.KeyIndex = 0;
        }

        /// <summary>
        /// Stop DCPU-16 Emulation button  On click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.UseKeyboardRingBuffer = checkBox1.Checked;
            m_DPUEmulator.Stop();
            CycleTimer.Enabled = false;
            m_cycleCount = m_lastCycleCount = 0;
        }

        //----------------------------------------------------------
        // On Events
        //----------------------------------------------------------
        // TODO: test to ensure thread safety! 

        /// <summary>
        /// OnCpuExecutePostStep
        /// 
        /// Called after execution of DCPU-16 instruction. 
        /// </summary>
        /// <param name="_cpu">Reference to existing DCPU-16 object</param>
        protected void OnCpuExecutePostStep(ref CDCPU16 _cpu)
        {
            if (_cpu == null) return;

            if (m_MakeCpuDoublebuffer != false)
            {
                lock (m_CpuDoublebuffer)
                {
                    
                    // make a copy of ram. 
                    Array.Copy(_cpu.Memory.RAM, m_CpuDoublebuffer.Memory.RAM, 0x10000);
                    Array.Copy(_cpu.Registers.GP, m_CpuDoublebuffer.Registers.GP, 8);

                    m_CpuDoublebuffer.Registers.O = _cpu.Registers.O;
                    m_CpuDoublebuffer.Registers.PC = _cpu.Registers.PC;
                    m_CpuDoublebuffer.Registers.SP = _cpu.Registers.SP;
                    m_CpuDoublebuffer.CycleCount = _cpu.CycleCount;

                    m_MakeCpuDoublebuffer = false;
                }
            }          
        }

        /// <summary>
        /// OnCpuError
        /// 
        /// Called on DCPU-16 Error
        /// </summary>
        /// <param name="_cpu">Reference to existing DCPU-16 object</param>
        protected void OnCpuError(ref CDCPU16 _cpu)
        {
            if (_cpu == null) return;

        }

        /// <summary>
        /// TEMP function
        /// Dumps Binary data to BinaryDumptextBox
        /// </summary>
        private void BinaryMemoryDump() 
        {
            if (m_DPUEmulator.BinaryLength < 1) return;

            StringBuilder bufferString = new StringBuilder();
            lock (m_CpuDoublebuffer)
            {                
                BinaryDumptextBox.Text = string.Empty;
                

                for(int i = 0; i <m_DPUEmulator.BinaryLength;i++)
                {
                    ushort notchByte = m_CpuDoublebuffer.Memory.RAM[i];                    
                    if(((i+1) % 6) == 0)
                    {
                        bufferString.AppendLine(string.Format("{0:X4}", notchByte));
                    }
                    else
                    {
                        bufferString.Append(string.Format("{0:X4} ", notchByte));
                    }
                                
                }
            }

            BinaryDumptextBox.Text = bufferString.ToString();
        }

        /// <summary>
        /// UI Component update timer. 
        /// 
        /// updates every 16ms. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_MakeCpuDoublebuffer != false) return;
            
            lock (m_CpuDoublebuffer)
            {
                RegAtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.A]);
                RegBtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.B]);
                RegCtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.C]);

                RegXtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.X]);
                RegYtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.Y]);
                RegZtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.Z]);

                RegItextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.I]);
                RegJtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.GP[(ushort)dcpuRegisterCodes.J]);

                RegPCtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.PC);
                RegSPtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.SP);
                RegOtextBox.Text = string.Format("{0:X4}", m_CpuDoublebuffer.Registers.O);

                cycleCounttextBox.Text = m_CpuDoublebuffer.CycleCount.ToString();
                m_MakeCpuDoublebuffer = true;
                m_cycleCount = m_CpuDoublebuffer.CycleCount;            

                DrawToFrameBuffer();

                pictureBox1.Refresh();
                pictureBox2.Refresh();
            }

        }

        /// <summary>
        /// DCPU-16 Cycle timer. 
        /// 
        /// Cheap way of calculating DCPU-16's current freq.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CycleTimer_Tick(object sender, EventArgs e)
        {
            long diff = (m_cycleCount - m_lastCycleCount);

            if (diff >= 10000000) // if >= 10Mhz
            {
                diff /= 1000000;
                CpuFreqLabel.Text = string.Format("~{0} Mhz", diff);
            }
            else if (diff > 1000)
            {
                diff /= 1000;
                CpuFreqLabel.Text = string.Format("~{0} Khz", diff);
            }
            else
            {
                CpuFreqLabel.Text = string.Format("~{0} hz", diff);
            }

            m_lastCycleCount = m_cycleCount;
        }

        /// <summary>
        /// DCPU-16 Reset button on click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.UseKeyboardRingBuffer = checkBox1.Checked;
            m_DPUEmulator.Reset();
            m_lastCycleCount = m_cycleCount = 0;
        }

        /// <summary>
        /// DCPU-16 Freq Trackbar Scroll event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SetCpuFreqLabel.Text = string.Format("{0} Khz",trackBar1.Value);

            m_DPUEmulator.TargetFreqKHZ = trackBar1.Value;
        }

        /// <summary>
        /// Main Menu
        /// Help->About
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutDCPU16ASMNETToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog(this);
        }

        /// <summary>
        /// Picture box On paint event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                  
            if(m_FrameBuffer != null) g.DrawImage(m_FrameBuffer, 0, 0,pictureBox1.Width,pictureBox1.Height);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            if (m_FrameBuffer != null) g.DrawImage(m_FrameBuffer, 0, 0, pictureBox2.Width, pictureBox2.Height);

        }
        //----------------------------------------------------------
        // Text mode Drawing code. 
        //----------------------------------------------------------

        /// <summary>
        /// Draw Text buffer to our bitmap Framebuffer.
        /// </summary>
        private void DrawToFrameBuffer()
        {
            BitmapData Data = m_FrameBuffer.LockBits(new Rectangle(0, 0, ScreenPixelWidth, ScreenPixelHeight), 
                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData fontData = m_FontBuffer.LockBits(new Rectangle(0, 0, m_FontBuffer.Width, m_FontBuffer.Height),
                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);


            /**
             * Time to bring back some memories *sniff*
             * This kind of thing was more fun on the Nintendo DS. Although
             * 4x8 kerneling into 8x8 native tiles was annoying at times, the 
             * Nintendo DS's tiles were linear 1D arrays which made this really
             * nice. 
             */
            unsafe
            {
                IntPtr bmpScan = Data.Scan0;
                IntPtr fontScan = fontData.Scan0;

                // Slow font kerneling 
                for (int ty = 0; ty < ScreenTextHeight; ty++)                
                {
                    int screenOffsetY = (ty * FontHeight);

                    for (int tx = 0; tx < ScreenTextWidth; tx++)
                    {
                        int screenOffsetX = (tx * FontWidth);
                        
                        int tff = ty * ScreenTextWidth + tx;

                        ushort charData = m_CpuDoublebuffer.Memory.RAM[(int)dcpuMemoryLayout.VIDEO_TEXT_START + tff];
                        byte c = (byte)(charData & 0xFF);       // character
                        int b = (int)((charData >> 8) & 0xF);   // background Color
                        int f = (int)((charData >> 12) & 0xF);  // charcter Color       

                        // calculate pixel position of character in font. 
                        // image is 128x32, so 128/4 = 32. 
                        int fOffX = (c % 32) * FontWidth;
                        int fOffY = (c / 32) * FontHeight;

                        for (int py = 0; py < FontHeight; py++)                        
                        {
                            for (int px = 0; px < FontWidth; px++)
                            {
                                int screenOff = ((screenOffsetY + py) * (ScreenPixelWidth) + screenOffsetX + px) * 4;
                                int fontOff = ((fOffY + py) * m_FontBuffer.Width + fOffX + px) * 4;

                                uint fontPixel = (uint)(*((byte*)(void*)fontScan + fontOff + 0) +
                                                (*((byte*)(void*)fontScan + fontOff + 1) << 8) +
                                                (*((byte*)(void*)fontScan + fontOff + 2) << 16));

                                fontPixel = (b == 0 && f == 0 && c != 0) ? fontPixel :
                                    (fontPixel != 0 && c != 0) ? fontPixel = m_TextColorMap[f] : fontPixel = m_TextColorMap[b];

                                *((byte*)(void*)bmpScan + screenOff + 0) = (byte)(fontPixel & 0xFF);
                                *((byte*)(void*)bmpScan + screenOff + 1) = (byte)((fontPixel >> 8) & 0xFF);
                                *((byte*)(void*)bmpScan + screenOff + 2) = (byte)((fontPixel >> 16) & 0xFF);
                                *((byte*)(void*)bmpScan + screenOff + 3) = 0xFF;
                            }
                        }

                    }
                }
                
            }
            m_FontBuffer.UnlockBits(fontData);
            m_FrameBuffer.UnlockBits(Data);
            
        }

        /// <summary>
        /// Keypress Event.
        /// Only triggered by game window!
        /// </summary>
        /// <param name="_keyPress"></param>
        public void ProcessKeyPress(ushort _keyPress)
        {

            m_DPUEmulator.ProcessKeyPress(_keyPress);
        }

        /// <summary>
        /// Launch User window
        /// this window we can capture key inputs!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            UserWindowForm gameWindow = new UserWindowForm();
            MainForm tmp = this;
            gameWindow.FeedReferences(ref m_FrameBuffer, ref tmp);
            gameWindow.ShowDialog(this);
        }

        /// <summary>
        /// Check box change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;

            if (m_DPUEmulator.Running != true) return;

            if (MessageBox.Show("DCPU-16 Program must be restarted for this to work!\r\n\r\nDo you wish to restart the DCPU-16 Program?",
                Globals.ProgramName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                checkBox1.Checked = !checkBox1.Checked;

                return;
            }

            // reset program
            button3_Click(sender, e);
        }
    }
}
