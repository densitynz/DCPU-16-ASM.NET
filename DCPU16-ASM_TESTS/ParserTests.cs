using System;
using System.Text;
using NUnit.Framework;

using DCPU16_ASM.Assembler;

namespace DCPU16ASM_TESTS
{
	[TestFixture]
	public class ParserTests
	{	
		[Test]
		public void WhenParserCalledWithEmptySourceDoesNotGenerateInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{""});
			
			Assert.IsNull (instructionSet);
		}
		
		[Test]
		public void WhenParserCalledWithCommentOnlyDoesNotGenerateInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{"; Try some basic stuff"});
			
			Assert.IsNull (instructionSet);
		}
		
		[Test]
		public void WhenParserCalledWithSetRegisterWithLiteralGenertesCorrectInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{"SET A, 0x30"});
			
			string convertedInstructionSet = ConvertInstructionSetToString (instructionSet);
			Assert.AreEqual ("7c01 0030".ToUpper (), convertedInstructionSet.ToUpper ());
		}
		
		[Test]
		public void WhenParserCalledWithSetMemoryAddressWithLiteralGenertesCorrectInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{"SET [0x1000], 0x20"});
			
			string convertedInstructionSet = ConvertInstructionSetToString (instructionSet); 
			Assert.AreEqual ("7de1 1000 0020".ToUpper (), convertedInstructionSet.ToUpper ());
		}
		
		[Test]
		public void WhenParserCalledWithSetRegisterWithDecimalLiteralGenertesCorrectInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{"SET I 10"});
			
			string convertedInstructionSet = ConvertInstructionSetToString (instructionSet); 
			Assert.AreEqual ("a861".ToUpper (), convertedInstructionSet.ToUpper ());
		}
		
		[Test]
		public void WhenParserCalledWithSetRegisterWithHexLiteralGenertesCorrectInstructionSet()
		{
			Parser parser = new Parser ();
			
			ushort[] instructionSet = parser.Parse (new string[]{"SET A 0x2000"});
			
			string convertedInstructionSet = ConvertInstructionSetToString (instructionSet); 
			Assert.AreEqual ("7c01 2000".ToUpper (), convertedInstructionSet.ToUpper());
		}
		
		[Test]
		public void WhenParserCalledWithNotchSampleGenertesCorrectInstructionSet()
		{
			Parser parser = new Parser ();
			
			string source = @"
			; Try some basic stuff
            			SET A, 0x30				; 7c01 0030
                		SET [0x1000], 0x20		; 7de1 1000 0020
                		SUB A, [0x1000]			; 7803 1000
                		IFN A, 0x10				; c00d 
                   			SET PC, crash		; 7dc1 001a [*]
                      
	        ; Do a loopy thing
	        			SET I, 10				; a861
	        			SET A, 0x2000			; 7c01 2000
	        :loop		SET [0x2000+I], [A]		; 2161 2000
	        			SUB I, 1				; 8463
	        			IFN I, 0				; 806d
	        			SET PC, loop			; 7dc1 000d [*]
	        
	        ; Call a subroutine
		        		SET X, 0x4				; 9031
		        		JSR testsub				; 7c10 0018 [*]
		        		SET PC, crash			; 7dc1 001a [*]
	        
	        :testsub	SHL X, 4				; 9037
	        			SET PC, POP				; 61c1
	                        
	        ; Hang forever. X should now be 0x40 if everything went right.
	        :crash		SET PC, crash			; 7dc1 001a [*]";
			
			string[] lines = source.Split('\n');
			ushort[] instructionSet = parser.Parse(lines);
			string convertedInstructionSet = ConvertInstructionSetToString(instructionSet);
			
			string memoryDump = "";
			memoryDump += "7c01 0030 7de1 1000 0020 7803 1000 c00d ";
			memoryDump += "7dc1 001a a861 7c01 2000 2161 2000 8463 ";
        	memoryDump += "806d 7dc1 000d 9031 7c10 0018 7dc1 001a ";
        	memoryDump += "9037 61c1 7dc1 001a";
			
			Assert.AreEqual (memoryDump.ToUpper (), convertedInstructionSet.ToUpper());
		}
		
		private string ConvertInstructionSetToString (ushort[] instructionSet)
		{
			StringBuilder builder = new StringBuilder ();
			
			foreach (var code in instructionSet) 
			{
				builder.Append (string.Format ("{0:X4} ", code));
			}
			
			string output = builder.ToString ();
			output = output.Substring (0, output.Length - 1);
			return output;
		}
	}
}

