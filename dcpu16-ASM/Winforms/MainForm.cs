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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using dcpu16_ASM.Emulator;

namespace dcpu16_ASM.Winforms
{
    public partial class MainForm : Form
    {
        // Keep a local copy of the cpu state. 
        class cpuDoubleBuffer
        {
            public cpuMemory       Memory;
            public cpuRegisters    Registers;
            public long            CycleCount;
        }

        private CDCPU16Emulator m_DPUEmulator = new CDCPU16Emulator();

        private cpuDoubleBuffer m_CpuDoublebuffer = new cpuDoubleBuffer();  // TODO: support multiple cpu data.
        private bool m_MakeCpuDoublebuffer = true;

        private long m_lastCycleCount = 0;
        private long m_cycleCount = 0;

        public MainForm()
        {
            InitializeComponent();

            m_DPUEmulator.OnExecutePostStepEvent += new OnExecutePostStepHandle(OnCpuExecutePostStep);
            m_DPUEmulator.OnErrorEvent += new OnErrorHandle(OnCpuError);
            
            // initialize buffer
            m_CpuDoublebuffer.Memory.RAM = new ushort[0xFFFF]; // TODO: Define memory size so we don't have to change this is 20 places
            m_CpuDoublebuffer.Registers.GP = new ushort[8];
            m_CpuDoublebuffer.CycleCount = 0;
        }

        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
                BinaryMemoryDump();
            }
        }

        // Load up Assembler dialog. 
        private void asemblerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssemblerForm asmDialog = new AssemblerForm();
            asmDialog.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.Start();
            CycleTimer.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.Stop();
            CycleTimer.Enabled = false;
            m_cycleCount = m_lastCycleCount = 0;
        }

        //----------------------------------------------------------
        // On Events
        //----------------------------------------------------------
        // TODO: test to ensure thread safety! 

        protected void OnCpuExecutePreStep(ref CDCPU16 _cpu)
        {
            if (_cpu == null) return;

        }

        protected void OnCpuExecutePostStep(ref CDCPU16 _cpu)
        {
            if (_cpu == null) return;

            if (m_MakeCpuDoublebuffer != false)
            {
                lock (m_CpuDoublebuffer)
                {
                    // make a copy of ram. 
                    Array.Copy(_cpu.Memory.RAM, m_CpuDoublebuffer.Memory.RAM, 0xFFFF);
                    Array.Copy(_cpu.Registers.GP, m_CpuDoublebuffer.Registers.GP, 8);

                    m_CpuDoublebuffer.Registers.O = _cpu.Registers.O;
                    m_CpuDoublebuffer.Registers.PC = _cpu.Registers.PC;
                    m_CpuDoublebuffer.Registers.SP = _cpu.Registers.SP;
                    m_CpuDoublebuffer.CycleCount = _cpu.CycleCount;

                    m_MakeCpuDoublebuffer = false;
                }
            }          
        }

        protected void OnCpuError(ref CDCPU16 _cpu)
        {
            if (_cpu == null) return;

        }

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

        // Update UI timer
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
            }

        }

        // Cycle Timer
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

        private void button3_Click(object sender, EventArgs e)
        {
            m_DPUEmulator.Reset();
            m_lastCycleCount = m_cycleCount = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SetCpuFreqLabel.Text = string.Format("{0} Khz",trackBar1.Value);

            m_DPUEmulator.TargetFreqKHZ = trackBar1.Value;
        }

    }
}
