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

// Font related saving/loading and ASM output generation code.  

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DCPU16_ASM.Emulator;

namespace DCPU16_ASM.Tools
{
    /// <summary>
    /// Font constants! 
    /// </summary>
    public static class FontConstants
    {
        public static int FontWidth = 4;
        public static int FontHeight = 8;

        public static int MaxFontEntries = ((int)(dcpuMemoryLayout.VIDEO_CHARSET_END - dcpuMemoryLayout.VIDEO_CHARSET_START) / 2) + 1;
    }

    /// <summary>
    /// Our Font set Object for internal usage/saving
    /// </summary>
    public class CFontCharSet
    {
        /// <summary>
        /// F
        /// </summary>
        private readonly char[] m_FileID = new char[4] 
        {
            'D','F','N','T'
        };
        private readonly int m_FileVersion = 1;

        /// <summary>
        /// Filename
        /// </summary>
        private string m_fileName = "";
        /// <summary>
        /// Our 128 font characters (each take 2 words to describe all 8x4 pixels)
        /// </summary>
        private ushort[] m_fontCharacters;


        public ushort[] FontCharacters { get { return m_fontCharacters; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CFontCharSet()
        {
            m_fontCharacters = new ushort[FontConstants.MaxFontEntries*2];
        }

        /// <summary>
        /// Completely wipes the font clear
        /// </summary>
        public void Clear()
        {
            m_fileName = "";

            Array.Clear(m_fontCharacters, 0, m_fontCharacters.Length);
        }

        /// <summary>
        /// Write character map header file
        /// </summary>
        /// <param name="_mem"></param>
        private void WriteHeader(ref MemoryStream _mem)
        {
            if (_mem == null) return;

            for (int i = 0; i < m_FileID.Length; i++)
            {
                _mem.WriteByte((byte)m_FileID[i]);
            }

            _mem.WriteByte((byte)((m_FileVersion >> 24) & 0xFF));
            _mem.WriteByte((byte)((m_FileVersion >> 16) & 0xFF));
            _mem.WriteByte((byte)((m_FileVersion >> 8) & 0xFF));
            _mem.WriteByte((byte)((m_FileVersion >> 0) & 0xFF));
        }

        /// <summary>
        /// Reads our special character map header, if anyting doesn't match
        /// we'll throw an IO Exception
        /// </summary>
        /// <param name="_mem"></param>
        private void ReadHeader(ref MemoryStream _mem)
        {
            if (_mem == null) return;

            for(int i = 0; i < 4; i++)
                _mem.ReadByte(); // skip header text. 
            
            int id = 0;
            int t1 = _mem.ReadByte(); int t2 = _mem.ReadByte(); int t3 = _mem.ReadByte(); int t4 = _mem.ReadByte();

            id = (t1 << 24) + (t2 << 16) + (t3 << 8) + t4;

            if(id != m_FileVersion) throw new IOException("File version doesn't match!");

        }


        /// <summary>
        /// Load our Font character set from disk
        /// </summary>
        /// <param name="_filename"></param>
        public bool Load(string _filename)
        {
            try
            {
                MemoryStream mem = new MemoryStream(File.ReadAllBytes(_filename));

                ReadHeader(ref mem);

                for (int i = 0; i < m_fontCharacters.Length; i++)
                {
                    byte high = (byte)mem.ReadByte();
                    byte low = (byte)mem.ReadByte();
                    m_fontCharacters[i] = (ushort)((high << 8) | (low & 0xFF));
                }

            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ee)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save our character set to disk
        /// </summary>
        /// <param name="_filename"></param>
        public bool Save(string _filename)
        {
            try
            {
                MemoryStream mem = new MemoryStream();

                WriteHeader(ref mem);

                foreach (ushort word in m_fontCharacters)
                {
                    mem.WriteByte((byte)((word >> 8) & 0xFF));
                    mem.WriteByte((byte)(word & 0xFF));                        
                }

                File.WriteAllBytes(_filename, mem.ToArray());
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            m_fileName = _filename;

            return true;
        }

        /// <summary>
        /// Dump font/character map to assembly file.
        /// </summary>
        /// <param name="_filename"></param>
        /// <returns></returns>
        public bool DumpToAssembly(string _filename)
        {
            try
            {
                StringBuilder dumpString = new StringBuilder();
                dumpString.AppendLine(string.Format("; Font built with {0}",Globals.ProgramName));
                dumpString.AppendLine();

                ushort addressOffset = (ushort)dcpuMemoryLayout.VIDEO_CHARSET_START;
                for (int i = 0; i < m_fontCharacters.Length; i++)
                {
                    dumpString.AppendLine(string.Format("SET [0x{0:X4}], 0x{1:X4}",addressOffset,m_fontCharacters[i]));
                    addressOffset++;
                }

                // embed some ASM code so people can compile and run this right away. 
                // this'll let them quickly check if their character sets have been exported
                // correctly.
                dumpString.AppendLine();
                dumpString.AppendLine("; You can remove this stuff Below, it is only for testing your font.");
                dumpString.AppendLine();
                dumpString.AppendLine("SET i, 0");
                dumpString.AppendLine(":loop");
                dumpString.AppendLine("SET z, 0xF000"); // Set our text color to white, so we can see! 
                dumpString.AppendLine("BOR z, i");
                dumpString.AppendLine("SET [0x8000+i], z");
                dumpString.AppendLine("ADD i, 0x1");
                dumpString.AppendLine("IFN i, 0x80");
                dumpString.AppendLine("  SET PC, loop");
                dumpString.AppendLine(":crash SET PC, crash");

                File.WriteAllText(_filename, dumpString.ToString());
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save our character set to disk
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if (m_fileName.Trim() == String.Empty) return false;

            return Save(m_fileName);
        }

        /// <summary>
        /// Import font from a CURRENTLY RUNNING dcpu!
        /// I figure this'll be really handy for some people.
        /// </summary>
        /// <param name="_dcpu">dpu-16 reference</param>
        public void ImportFromDCPU(ref cpuDoubleBuffer _dcpu)
        {
            if (_dcpu == null) return;

            lock (_dcpu)
            {
                Array.Copy(_dcpu.Memory.RAM, (int)dcpuMemoryLayout.VIDEO_CHARSET_START, m_fontCharacters, 0, 
                    (int)(dcpuMemoryLayout.VIDEO_CHARSET_END - dcpuMemoryLayout.VIDEO_CHARSET_START));
            }
            
        }
    }
    
}
