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


// for debugging
using System.Windows.Forms;

namespace dcpu16_ASM.Emulator
{
    // Event handlers

    // Execution pre/post step events
    // these are called within the DCPU-16 Thread
    public delegate void OnExecutePostStepHandle(ref CDCPU16 _cpu);
    public delegate void OnExecutePreStepHandle(ref CDCPU16 _cpu);
    public delegate void OnErrorHandle(ref CDCPU16 _cpu);

    public delegate void OnPauseHandle();
    public delegate void OnStartHandle();
    public delegate void OnStopHandle();
    public delegate void OnResetHandle();
    
    
    public class CDCPU16Emulator
    {
        private CDCPU16 m_DCPUComputer = new CDCPU16();
        private Thread m_DCPUThread = null;
        private long m_BinaryLength = 0;
        public long BinaryLength { get { return m_BinaryLength; } }        

        public event OnExecutePostStepHandle OnExecutePostStepEvent = null;
        public event OnErrorHandle OnErrorEvent = null; // TODO

        public event OnPauseHandle OnPauseEvent = null; // TODO
        public event OnStartHandle OnStartEvent = null;
        public event OnStopHandle OnStopEvent = null;
        public event OnResetHandle OnResetEvent = null; // TODO

        private Stopwatch m_StopwatchTimer = new Stopwatch();

        private int m_TargetKHZ = 100; 

        public int TargetFreqKHZ
        {
            get { return m_TargetKHZ; }
            set { m_TargetKHZ = value; }
        }

        public CDCPU16Emulator()
        {
        }

        public bool LoadProgram(string _fileName, 
                    out string _errorMessage)
        {
            _errorMessage = "";

            if (File.Exists(_fileName) != true) return false;

            try
            {
                byte[] byteData = File.ReadAllBytes(_fileName);

                if ((byteData.Length % 2) != 0) return false; // Must be even number of bytes! 
                List<ushort> programWords = new List<ushort>();
                for (int i = 1; i < byteData.Length; i += 2)
                {
                    ushort word = (ushort)((byteData[i - 1] << 8) | (byteData[i] & 0xFF));
                    programWords.Add(word);
                }
                m_BinaryLength = programWords.Count;
                m_DCPUComputer.SetProgram(ref programWords);
                m_DCPUComputer.ResetCPURegisters();
                if(OnExecutePostStepEvent!=null)OnExecutePostStepEvent(ref m_DCPUComputer); 
                programWords.Clear();
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

        public void Pause()
        {
            if (m_DCPUThread == null) return;            
        }

        public void Stop()
        {
            if (m_DCPUThread == null) return;

            m_DCPUThread.Abort();
            m_DCPUThread.Join();

            if (OnStopEvent != null) OnStopEvent();
        }

        public void Reset()
        {
            if (m_DCPUThread == null) return;

            m_DCPUComputer.ResetCPURegisters();            
        }

        public void Start()
        {
            if (m_DCPUComputer.ProgramLoaded != true) return;

            if (m_DCPUThread != null)
            {
                m_DCPUThread.Interrupt();
                m_DCPUThread.Join();

            }           
            m_DCPUThread = new Thread(new ThreadStart(RunProgram));
            m_DCPUThread.IsBackground = true;
            m_DCPUThread.Priority = ThreadPriority.BelowNormal;
            m_DCPUThread.Start();

            if (OnStartEvent != null) OnStopEvent();
        }

        public void RunProgram() 
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

                    m_DCPUComputer.ExecuteInstruction(); // TODO: pull error string

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
                            Thread.Sleep(new TimeSpan((int)(totalTicks - elapsedTicks)));                            
                        }
                    }

                    if (OnExecutePostStepEvent != null)
                        OnExecutePostStepEvent(ref m_DCPUComputer);

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