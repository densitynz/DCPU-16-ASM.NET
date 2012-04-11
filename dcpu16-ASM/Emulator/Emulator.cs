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

/**
 * WORK IN PROGRESS - 10th April 2012
 * - DensitY
 */


using System;
using System.Collections.Generic;
using System.IO;

namespace dcpu16_ASM.Emulator
{
    
    public class CDCPU16Emulator
    {
        private CDCPU16 m_DCPUComputer = new CDCPU16();

        public CDCPU16Emulator()
        {
        }

        public bool LoadProgram(string _fileName)
        {
            if (File.Exists(_fileName) != true) return false;

            byte[] byteData = File.ReadAllBytes(_fileName);

            if ((byteData.Length % 2) != 0) return false; // Must be even number of bytes! 
            List<ushort> programWords = new List<ushort>();
            for (int i = 1; i < byteData.Length; i += 2)
            {
                ushort word = (ushort)((byteData[i - 1] << 8) | (byteData[i] & 0xFF));
                programWords.Add(word);
            }
            m_DCPUComputer.SetProgram(ref programWords);
            programWords.Clear();

            return true;
        }

        public void RunProgram()
        {
            if (m_DCPUComputer.ProgramLoaded != true) return;

            // TODO: proper timer

            for(int i = 0; i < 9999;i++) m_DCPUComputer.ExecuteInstruction();
        }

    }
}