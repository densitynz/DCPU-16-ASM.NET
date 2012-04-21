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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using System.Drawing.Imaging;
using System.Drawing;

using DCPU16_ASM.Tools;

namespace DCPU16_ASM.Emulator
{
    // Event handlers

    // Execution pre/post step events
    // these are called within the DCPU-16 Thread

    /// <summary>
    /// On Post step Delegate struct
    /// </summary>
    /// <param name="_cpu">Reference to existing DCPU-16 object</param>
    public delegate void OnExecutePostStepHandle(ref CDCPU16 _cpu);
    /// <summary>
    /// On Error Delegate struct
    /// </summary>
    /// <param name="_cpu">Reference to existing DCPU-16 object</param>
    public delegate void OnErrorHandle(ref CDCPU16 _cpu);

    /// <summary>
    /// On Pause Delegate struct
    /// </summary>
    public delegate void OnPauseHandle();
    /// <summary>
    /// On Start Delegate struct
    /// </summary>
    public delegate void OnStartHandle();
    /// <summary>
    /// On Stop Delegate struct
    /// </summary>
    public delegate void OnStopHandle();
    /// <summary>
    /// On Reset Delegate struct
    /// </summary>
    public delegate void OnResetHandle();
    
    //-----------------------------------------------------------------------------------
    
    /// <summary>
    /// 
    /// </summary>
    public class CDCPU16Emulator
    {
        /// <summary>
        /// DCPU-16 Computer
        /// </summary>
        private CDCPU16 m_DCPUComputer = new CDCPU16();
        /// <summary>
        /// DCPU Thread 
        /// </summary>
        private Thread m_DCPUThread = null;

        /// <summary>
        /// Binary file length.
        /// </summary>
        private long m_BinaryLength = 0;
        public long BinaryLength { get { return m_BinaryLength; } }
        private bool m_Running = false;
        public bool Running { get { return m_Running; } }

        public event OnExecutePostStepHandle OnExecutePostStepEvent = null;
        public event OnErrorHandle OnErrorEvent = null; // TODO

        public event OnPauseHandle OnPauseEvent = null; // TODO
        public event OnStartHandle OnStartEvent = null;
        public event OnStopHandle OnStopEvent = null;
        public event OnResetHandle OnResetEvent = null; // TODO

        /// <summary>
        /// Processor Timer 
        /// </summary>
        private Stopwatch m_StopwatchTimer = new Stopwatch();

        private int m_keyCounter = 0;

        /// <summary>
        /// Target DCPU-16 Frequence (in khz)
        /// </summary>
        private int m_TargetKHZ = 125; 

        public int TargetFreqKHZ
        {
            get { return m_TargetKHZ; }
            set { m_TargetKHZ = value; }
        }

        private bool m_UseKeyboardRingBuffer = false;

        public bool UseKeyboardRingBuffer
        {
            get { return m_UseKeyboardRingBuffer; }
            set { m_UseKeyboardRingBuffer = value; }
        }

        /// <summary>
        /// Local copy of Load program, used to clear everything out on reset or restart
        /// </summary>
        List<ushort> m_programWords = new List<ushort>();
        
        /// <summary>
        /// Constructor
        /// </summary>
        public CDCPU16Emulator()
        {
        }

        /// <summary>
        /// Loads DCPU-16 binary Program from disk into DCPU's ram.
        /// </summary>
        /// <param name="_fileName">name of file to load</param>
        /// <param name="_errorMessage">String storing error messages, empty if no errors found</param>
        /// <returns>true on success, false in failure</returns>
        public bool LoadProgram(string _fileName, 
                    out string _errorMessage)
        {
            _errorMessage = "";

            if (File.Exists(_fileName) != true) return false;

            try
            {
                byte[] byteData = File.ReadAllBytes(_fileName);

                if ((byteData.Length % 2) != 0) return false; // Must be even number of bytes! 
                m_programWords.Clear();
                for (int i = 1; i < byteData.Length; i += 2)
                {
                    ushort word = (ushort)((byteData[i - 1] << 8) + (byteData[i] & 0xFF));
                    m_programWords.Add(word);
                }
                m_BinaryLength = m_programWords.Count;
                m_DCPUComputer.ResetCPURegisters();
                m_DCPUComputer.SetProgram(ref m_programWords);                
                if(OnExecutePostStepEvent!=null)OnExecutePostStepEvent(ref m_DCPUComputer); 
            }
            catch (IOException ioE) // TODO: proper logging! 
            {
                _errorMessage = string.Format("IO Exception: {0}\r\nStack-Trace: {1}", ioE.Message, ioE.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                _errorMessage = string.Format("Exception: {0}\r\nStack-Trace: {1}", e.Message, e.StackTrace);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Load font into DCPU-16's Video memory
        /// 
        /// Based on Notch's Specs
        /// </summary>
        /// <param name="_fontBytes"></param>
        public void LoadFontIntoVideoMemory(Bitmap _fontBuffer)
        {
            BitmapData fontData = _fontBuffer.LockBits(new Rectangle(0, 0, _fontBuffer.Width, _fontBuffer.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                IntPtr fontScan = fontData.Scan0;

                for (int c = 0; c < _fontBuffer.Width; c++)
                {
                    int charOffset = (int)dcpuMemoryLayout.VIDEO_CHARSET_START + c * 2;
                    int xOffset = (c % 32) * FontConstants.FontWidth;
                    int yOffset = (c / 32) * FontConstants.FontHeight;

                    m_DCPUComputer.Memory.RAM[charOffset] = '\0';
                    m_DCPUComputer.Memory.RAM[charOffset + 1] = '\0';

                    for (int x = 0; x < FontConstants.FontWidth; x++)
                    {
                        int bitPixel = 0;
                        for (int y = 0; y < FontConstants.FontHeight; y++)
                        {
                            int fontscanOffset = ((xOffset + x) + (yOffset + y) * 128);
                            int fontPixel = *((int*)(void*)fontScan + fontscanOffset);
                            if ((fontPixel & 0xFF) > 128) bitPixel |= 1 << y;       
                        }

                        m_DCPUComputer.Memory.RAM[charOffset + x  / 2] |= (ushort)(bitPixel << (x + 1 & 1) * FontConstants.FontHeight);
                    }

                }
            }
            _fontBuffer.UnlockBits(fontData);

        }

        /// <summary>
        /// Pause DCPU-16 Thread
        /// </summary>
        public void Pause()
        {
            if (m_DCPUThread == null) return;            
        }

        /// <summary>
        /// Stop DCPU-16 Thread
        /// </summary>
        public void Stop()
        {
            m_Running = false;
            if (m_DCPUThread == null) return;

            m_DCPUThread.Abort();
            m_DCPUThread.Join();

            if (OnStopEvent != null) OnStopEvent();
            m_keyCounter = 0;
            m_DCPUComputer.SetProgram(ref m_programWords);  
        }

        /// <summary>
        /// Reset the DCPU-16's Registers
        /// </summary>
        public void Reset()
        {
            if (m_DCPUThread == null) return;

            m_DCPUComputer.ResetCPURegisters();
            m_keyCounter = 0;
            m_DCPUComputer.SetProgram(ref m_programWords);
        }

        /// <summary>
        /// Start DCPU-16 Thread
        /// </summary>
        public void Start()
        {
            if (m_DCPUComputer.ProgramLoaded != true) return;

            if (m_Running != false) return;

            if (m_DCPUThread != null)
            {
                m_DCPUThread.Interrupt();
                m_DCPUThread.Join();

            }            
            m_DCPUThread = new Thread(new ThreadStart(RunProgram));
            m_DCPUThread.IsBackground = true;
            m_DCPUThread.Priority = ThreadPriority.BelowNormal;
            m_DCPUThread.Start();

            m_keyCounter = 0;

            m_Running = true;
            if (OnStartEvent != null) OnStopEvent();
        }

        /// <summary>
        /// Key press
        /// </summary>
        /// <param name="_keyPress"></param>
        public void ProcessKeyPress(ushort _keyPress)
        {
            if (m_DCPUComputer.ProgramLoaded != true) return;
            if (m_DCPUThread == null) return;

            if (m_UseKeyboardRingBuffer != false)
            {
                if (m_DCPUComputer.Memory.RAM[(int)dcpuMemoryLayout.KEYBOARD_START + m_keyCounter] == 0x0)
                {
                    m_DCPUComputer.Memory.RAM[(int)dcpuMemoryLayout.KEYBOARD_START + m_keyCounter] = (ushort)_keyPress;                    
                    m_DCPUComputer.Memory.RAM[(int)dcpuMemoryLayout.KEYBOARD_INDEX] = (ushort)m_keyCounter; // Store counter reference @ 0x9100 so dcpu programs can find it.
                }
                m_keyCounter++;

                if (m_keyCounter > 0xF) m_keyCounter = 0;
            }
            else
            {
                for (int i = 0; i < 0x10; i++)                
                {
                    if (m_DCPUComputer.Memory.RAM[(int)dcpuMemoryLayout.KEYBOARD_START + i] == 0x0)
                    {
                        m_DCPUComputer.Memory.RAM[(int)dcpuMemoryLayout.KEYBOARD_START + i] = (ushort)_keyPress;
                    }
                }
            }

        }


        /// <summary>
        /// DCPU-16 thread's Main. 
        /// 
        /// Executes DCPU-16 Instructions. 
        /// </summary>
        protected void RunProgram() 
        {
            try
            {
                // Adopted timing code from lodle's NotchCPU
                // check out his project @ https://github.com/lodle/NotchCpu
                m_DCPUComputer.ResetCPURegisters();
                long lastCycles = 0;
                while (true)
                {
                    long ticksPerInstruction = TimeSpan.TicksPerSecond / (m_TargetKHZ * 1000);
                    m_StopwatchTimer.Reset();
                    m_StopwatchTimer.Start();

                    lock (m_DCPUComputer)
                    {
                        m_DCPUComputer.ExecuteInstruction(); // TODO: pull error string
                    }

                    long elapsedTicks = m_StopwatchTimer.Elapsed.Ticks;
                    long totalTicks = ticksPerInstruction * (m_DCPUComputer.CycleCount - lastCycles);

                    if (elapsedTicks < totalTicks)
                    { 
                        while (true)
                        {
                            elapsedTicks = m_StopwatchTimer.Elapsed.Ticks;
                            if (m_StopwatchTimer.IsRunning != true ||
                                elapsedTicks > totalTicks) break;

                            /**
                                * Using spinwait vs Sleep doesn't make a lick of differences. While other emulators seem to be
                                * fixing this via Batch processing of instructions (so wait times are longer). I think that'll
                                * come back to bite if future 'virtual hardware' requires precise timings (And we of course will
                                * have to emulate that else people will write incorrect code). 
                                */
                            Thread.SpinWait((int)(totalTicks - elapsedTicks));
                        }
                    }

                    lock (m_DCPUComputer)
                    {
                        if (OnExecutePostStepEvent != null)
                            OnExecutePostStepEvent(ref m_DCPUComputer);
                    }
                    m_StopwatchTimer.Stop();
                    lastCycles = m_DCPUComputer.CycleCount;
                }
            }
            catch (ThreadInterruptedException interruptE)
            {
            }
            catch (Exception e)
            {
                // REPORT
            }
            finally
            {
            }         
        }

    }
}