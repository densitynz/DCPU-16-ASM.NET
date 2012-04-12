﻿/*
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

namespace DCPU16_ASM.Assembler
{
    using System;
    using System.IO;

    public class Generator
    {
        public string MessageOuput { get; private set; }

        public string Generate(ushort[]machineCode, string filename)
        {
            if (filename.Trim() == string.Empty)
            {
                filename = "Default.bin";
            }
            else
            {
                filename = filename.Split('.')[0] + ".bin";
            }

            try
            {
                var outfile = new MemoryStream();
                foreach (var word in machineCode)
                {
                    var b = (byte)(word >> 8);
                    var a = (byte)(word & 0xFF);

                    outfile.WriteByte(b);
                    outfile.WriteByte(a);
                }

                File.WriteAllBytes(filename, outfile.ToArray());
            }
            catch (Exception e)
            {
                this.AddMessageLine(string.Format("{0}", e.Message));
                return string.Empty;
            }

            this.AddMessageLine();
            this.AddMessageLine(string.Format("Saved to '{0}", filename));

            return filename;
        }

        private void AddMessageLine()
        {
            this.MessageOuput += "\r\n";
        }

        private void AddMessageLine(string input)
        {
            this.MessageOuput += string.Format("{0}\r\n", input);
        }
    }
}
