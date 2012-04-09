//--------------------------------------------------------------------------
// DCPU-16 ASM.NET
// Tim "DensitY" Hancock (densitynz@orcon.net.nz)
// 2012 
//--------------------------------------------------------------------------
// program.cs
//--------------------------------------------------------------------------

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
