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
 * Based on the specs below. 
 * http://0x10c.com/doc/dcpu-16.txt
 * 
 * Will take in ASM code, and throw out a .OBJ file. 
 * 
 * NOTE: Fixed tab issues, 4:55 PM, 9 April 2012.
 * NOTE: Cleaned and sorted it a bit, 01:17 PM, 9 April 2012 UTC/GMT+2.
 */

using System;

namespace dcpu16_ASM
{
   /// <summary>
   /// Program entry point. 
   /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("DCPU-16 ASM.NET Assembler Version 0.3 Super-ALPHA");
            Console.WriteLine();

            /*
            CDCPU16Assemble dd = new CDCPU16Assemble();
            dd.Assemble("test3.txt");
            Console.ReadLine();
            return;
            */
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dcpu16-ASM <filename>");
                Console.WriteLine();
                return; 
            }

            string filename = args[0];
            CDCPU16Assemble test = new CDCPU16Assemble();
            Console.WriteLine(string.Format("Assembling ASM file '{0}'",filename));

            test.Assemble(filename);


        }
    }
}
