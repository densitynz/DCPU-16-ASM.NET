//--------------------------------------------------------------------------
// DCPU-16 ASM.NET
// Tim "DensitY" Hancock (densitynz@orcon.net.nz)
// 2012 
//--------------------------------------------------------------------------
// program.cs
//--------------------------------------------------------------------------

/**
 * Speed coded Mess below...
 * 
 * Based on the specs below. 
 * http://0x10c.com/doc/dcpu-16.txt
 * 
 * Will take in ASM code, and throw out a .OBJ file. 
 *    
 */
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;



namespace dcpu16_ASM
{
    /// <summary>
    /// DCPU-16 Op codes, Unless extra bytes are required op codes are generally stored
    /// bbbbbbaaaaaaoooo
    /// 
    /// O = 4bits for cpu OpCode
    /// A = 6bits for Dest Param
    /// B = 6bits for Source Param 
    /// 
    /// Depending on parm, up to 2 extra words may be required (meaning max instruction length is 3 words long)
    /// </summary>
    public enum dcpuOpCode : ushort
    {
        NB_OP       = 0x0,      // Signals non basic instruction

        SET_OP      = 0x1,      // Set instruciton          -> A = B 
        ADD_OP      = 0x2,      // Add instruction          -> A = A + B
        SUB_OP      = 0x3,      // Subtract instruciton     -> A = A - B
        MUL_OP      = 0x4,      // Muliply  instruction     -> A = A * B
        DIV_OP      = 0x5,      // Divide instruction       -> A = A / B
        MOD_OP      = 0x6,      // Modulate instruction     -> A = A % B
        SHL_OP      = 0x7,      // Shift Left instruction   -> A = A << B
        SHR_OP      = 0x8,      // Shift right instruction  -> A = A >> B
        AND_OP      = 0x9,      // Boolean AND instruction  -> A = A & B
        BOR_OP      = 0xA,      // Boolean OR instruction   -> A = A | B
        XOR_OP      = 0xB,      // Boolean XOR instruction  -> A = A ^ B
        IFE_OP      = 0xC,      // Branch! if(A == B) run next instruction
        IFN_OP      = 0xD,      // Branch! if(A != B) run next instruction
        IFG_OP      = 0xE,      // Branch! if(A > B) run next instruction
        IFB_OP      = 0xF,      // Branch! if((A & B) != 0) run next instruction

        // Non basic instructions
        // Encoded as follows
        // AAAAAAoooooo0000 
        // Basically unlike basic instructions, we lose a register spot and use that for the op code.
        // the old op code is zeroed out (which signals a non basic instruction). This means 
        // any non basic instruction, even if its something like derp X, Y will use 2 words (unlike a basic instruction in that case)
        JSR_OP      = 0x10
    }

    /// <summary>
    /// DCPU-16 Register Codes
    /// 
    /// </summary>
    public enum dcpuRegisterCodes : ushort
    {
        // Basic register code, used to read value from register
        // ie SET A, X
        A = 0x00,
        B = 0x01,
        C = 0x02, 
        X = 0x03,
        Y = 0x04,
        Z = 0x05,
        I = 0x06,
        J = 0x07,

        // References memory location of register value
        // ie SET A, [X] 
        A_Mem = 0x08,
        B_Mem = 0x09,
        C_Mem = 0x0A,
        X_Mem = 0x0B,
        Y_Mem = 0x0C,
        Z_Mem = 0x0D,
        I_Mem = 0x0E,
        J_Mem = 0x0F,

        // References memory location with modifier
        // ie SET A [X+2]
        A_NextWord = 0x10,
        B_NextWord = 0x11,
        C_NextWord = 0x12,
        X_NextWord = 0x13,
        Y_NextWord = 0x14,
        Z_NextWord = 0x15,
        I_NextWord = 0x16,
        J_NextWord = 0x17,        

        POP = 0x18,     // [SP++]
        PEEK = 0x19,    // [SP]
        PUSH = 0x1A,    // [--SP]

        SP = 0x1B,      // Stack pointer
        PC = 0x1C,      // Program Counter
        O = 0x1D,       // Overflow Register

        NextWord_Literal_Mem = 0x1E,    // IE for "SET A, [0x1000]" B register will be 0x1E and we assume the next word (PC++)'s value is to reference a memory location
        NextWord_Literal_Value = 0x1F   // Same as above but its a literal value, not literal value to a memory location

        // if Literal value is < 0x1F we can encode it in 0x20-0x3F and skip the next word requirement. 
        // this is really handy for simple register initialization and incrementation, as we can encode it in as 
        // little as 1 word!
    }

    public class OpParamResult
    {
        public ushort Param;
        public bool nextWord; 
        public ushort NextWordValue;
        public string labelName;

        public bool illegal = false; 

        public OpParamResult()
        {
            Param = 0x0;
            nextWord = false;
            NextWordValue = 0x0;
            labelName = "";
            illegal = false;
        }
    }
    
    public class CDCPU16Assemble
    {
        private Dictionary<string, dcpuOpCode> m_opDictionary = new Dictionary<string, dcpuOpCode>();
        private Dictionary<string, dcpuRegisterCodes> m_regDictionary = new Dictionary<string, dcpuRegisterCodes>();

        private Dictionary<string, ushort> m_labelAddressDitionary = new Dictionary<string, ushort>();

        private Dictionary<ushort, string> m_labelReferences = new Dictionary<ushort, string>();

        private List<ushort> machineCode = new List<ushort>();

        private string m_filename = "";

        public CDCPU16Assemble()
        {
            BuildDictionaries();
        }

        private void BuildDictionaries()
        {
            m_opDictionary.Clear();
            m_regDictionary.Clear();

            // non basic instructions
            m_opDictionary.Add("jsr",   dcpuOpCode.JSR_OP);

            // basic instructions
            m_opDictionary.Add("set",   dcpuOpCode.SET_OP);
            m_opDictionary.Add("add",   dcpuOpCode.ADD_OP);
            m_opDictionary.Add("sub",   dcpuOpCode.SUB_OP);
            m_opDictionary.Add("mul",   dcpuOpCode.MUL_OP);
            m_opDictionary.Add("div",   dcpuOpCode.DIV_OP);
            m_opDictionary.Add("mod",   dcpuOpCode.MOD_OP);
            m_opDictionary.Add("shl",   dcpuOpCode.SHL_OP);
            m_opDictionary.Add("shr",   dcpuOpCode.SHR_OP);
            m_opDictionary.Add("and",   dcpuOpCode.AND_OP);
            m_opDictionary.Add("bor",   dcpuOpCode.BOR_OP);
            m_opDictionary.Add("xor",   dcpuOpCode.XOR_OP);
            m_opDictionary.Add("ife",   dcpuOpCode.IFE_OP);
            m_opDictionary.Add("ifn",   dcpuOpCode.IFN_OP);
            m_opDictionary.Add("ifg",   dcpuOpCode.IFG_OP);
            m_opDictionary.Add("ifb",   dcpuOpCode.IFB_OP);

            // Register dictionary, We'll only include the most common ones in here, others have to be constructred. 

            m_regDictionary.Add("a", dcpuRegisterCodes.A);
            m_regDictionary.Add("b", dcpuRegisterCodes.B);
            m_regDictionary.Add("c", dcpuRegisterCodes.C);
            m_regDictionary.Add("x", dcpuRegisterCodes.X);
            m_regDictionary.Add("y", dcpuRegisterCodes.Y);
            m_regDictionary.Add("z", dcpuRegisterCodes.Z);
            m_regDictionary.Add("i", dcpuRegisterCodes.I);
            m_regDictionary.Add("j", dcpuRegisterCodes.J);

            m_regDictionary.Add("[a]", dcpuRegisterCodes.A_Mem);
            m_regDictionary.Add("[b]", dcpuRegisterCodes.B_Mem);
            m_regDictionary.Add("[c]", dcpuRegisterCodes.C_Mem);
            m_regDictionary.Add("[x]", dcpuRegisterCodes.X_Mem);
            m_regDictionary.Add("[y]", dcpuRegisterCodes.Y_Mem);
            m_regDictionary.Add("[z]", dcpuRegisterCodes.Z_Mem);
            m_regDictionary.Add("[i]", dcpuRegisterCodes.I_Mem);
            m_regDictionary.Add("[j]", dcpuRegisterCodes.J_Mem);

            m_regDictionary.Add("pop", dcpuRegisterCodes.POP);
            m_regDictionary.Add("peek", dcpuRegisterCodes.PEEK);
            m_regDictionary.Add("push", dcpuRegisterCodes.PUSH);

            m_regDictionary.Add("sp", dcpuRegisterCodes.SP);
            m_regDictionary.Add("pc", dcpuRegisterCodes.PC);
            m_regDictionary.Add("o", dcpuRegisterCodes.O);

            m_regDictionary.Add("[+a]", dcpuRegisterCodes.A_NextWord);
            m_regDictionary.Add("[+b]", dcpuRegisterCodes.B_NextWord);
            m_regDictionary.Add("[+c]", dcpuRegisterCodes.C_NextWord);
            m_regDictionary.Add("[+x]", dcpuRegisterCodes.X_NextWord);
            m_regDictionary.Add("[+y]", dcpuRegisterCodes.Y_NextWord);
            m_regDictionary.Add("[+z]", dcpuRegisterCodes.Z_NextWord);
            m_regDictionary.Add("[+i]", dcpuRegisterCodes.I_NextWord);
            m_regDictionary.Add("[+j]", dcpuRegisterCodes.J_NextWord);
        }


        private OpParamResult ParseParam(string _param)
        {
            OpParamResult opParamResult = new OpParamResult();

            // Find easy ones. 
            string Param = _param.Replace(" ","").Trim(); // strip spaces
            if (m_regDictionary.ContainsKey(Param) != false)
            {
                // Ok things are really easy in this case. 
                opParamResult.Param = (ushort)m_regDictionary[Param];
            }
            else
            {  
                if(Param[0] == '[' && Param[Param.Length-1] == ']')
                {
                    if (Param.Contains("+") != false)
                    {
                        string[] psplit = Param.Replace("[","").Replace("]","").Split('+');
                        if (psplit.Length < 2)
                        {
                            throw new Exception(string.Format("malformated memory reference '{0}'",Param));
                        }
                        string addressValue = psplit[0];
                        if (m_regDictionary.ContainsKey("[+" + psplit[1] + "]") != true)
                        {
                           throw new Exception(string.Format("Invalid register reference in '{0}'",Param));
                        }
                        opParamResult.Param = (ushort)m_regDictionary["[+" + psplit[1] + "]"];
                        opParamResult.nextWord = true;
                        if (psplit[0].Contains("0x") != false)
                            opParamResult.NextWordValue = Convert.ToUInt16(psplit[0], 16);
                        else
                            opParamResult.NextWordValue = Convert.ToUInt16(psplit[0], 10);                                                
                    }
                    else
                    {
                        opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Mem;
                        opParamResult.nextWord = true;
                        if(Param.Contains("0x") != false)
                            opParamResult.NextWordValue = Convert.ToUInt16(Param.Replace("[","").Replace("]",""), 16);
                        else
                            opParamResult.NextWordValue = Convert.ToUInt16(Param.Replace("[", "").Replace("]", ""), 10);
                    }
                }
                else
                {
                    // if value is < 0x1F we can encode it into the param directly, 
                    // else it has to be next value!
                    
                    UInt16 maxValue = Convert.ToUInt16("0x1F", 16);
                    UInt16 literalValue = 0;
                    try
                    {
                        if (Param.Contains("0x") != false)
                            literalValue = Convert.ToUInt16(Param, 16);
                        else
                            literalValue = Convert.ToUInt16(Param, 10);

                        if (literalValue < maxValue)
                        {
                            opParamResult.Param = Convert.ToUInt16("0x20", 16);
                            opParamResult.Param += literalValue;
                        }
                        else
                        {
                            opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                            opParamResult.nextWord = true;
                            opParamResult.NextWordValue = literalValue;
                        }
                    }
                    catch
                    {
                        opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                        opParamResult.nextWord = true;
                        opParamResult.labelName = Param;             
                    }

                }
            }


            return opParamResult;
        }

        private void ParseData(string _data)
        {
            string[] dataFields = _data.Substring(3,_data.Length-3).Trim().Split(',');

            foreach (string dat in dataFields)
            {
                string valStr = dat.Trim();
                if (valStr.IndexOf('"') > -1)
                {
                    string asciiLine = dat.Replace("\"", "").Trim();
                    for (int i = 0; i < asciiLine.Length; i++)
                    {
                        machineCode.Add((ushort)asciiLine[i]);
                    }
                }
                else
                {
                    ushort val = 0;
                    if (valStr.Contains("0x") != false)
                        val = Convert.ToUInt16(valStr, 16);
                    else
                        val = Convert.ToUInt16(valStr, 10);

                    machineCode.Add((ushort)val);
                }
            }

            int d = 0;
        }

        private void AssembleLine(string _line)
        {
            if (_line.Trim().Length == 0) return;

            string line = _line.ToLower();

            if (line[0] == ':')
            {  // this is awful
                int sIndex = line.IndexOf(' ');                
                string labelName = line.Substring(1, line.Length-1);
                if (sIndex > 1)
                    labelName = line.Substring(1, sIndex);                

                if (m_labelAddressDitionary.ContainsKey(labelName) != false)
                {                    
                    throw new Exception(string.Format("Error! Label '{0}' already exists!", labelName));                 
                }
                m_labelAddressDitionary.Add(labelName.Trim(), (ushort)machineCode.Count);

                if (sIndex < 0) return;

                line = line.Remove(0, sIndex).Trim();
                if (line.Length < 1) return;
            }            
            
            string[] splitLine = line.Replace(",","").Split(' ');
            uint opCode = 0x0;

            string opCommand = splitLine[0].Trim();

            if (opCommand.ToLower() == "dat")
            {
                ParseData(line);
                return;
            }

            string opParam1 = splitLine[1].Trim();
            string opParam2 = "";



            if (m_opDictionary.ContainsKey(opCommand) != true)
            {                
                throw new Exception(string.Format("Illegal cpu opcode --> {0}",splitLine[0]));                
            }
            opCode = (uint)m_opDictionary[opCommand] & 0xF;
            
            if(opCode > 0x0)
                opParam2 = splitLine[2];// basic function, has second param.

            if (opCode > 0x00)
            {
                // Basic! 
                OpParamResult p1 = ParseParam(opParam1);
                OpParamResult p2 = ParseParam(opParam2);
                opCode |= ((uint)p1.Param << 4) & 0x3F0;
                opCode |= ((uint)p2.Param << 10) & 0xFC00;

                machineCode.Add((ushort)opCode);

                if (p1.nextWord != false)
                {
                    if (p1.labelName.Length > 0)
                    {
                        m_labelReferences.Add((ushort)machineCode.Count, p1.labelName);
                    }
                    machineCode.Add(p1.NextWordValue);


                }
                if (p2.nextWord != false)
                {
                    if (p2.labelName.Length > 0)
                    {
                        m_labelReferences.Add((ushort)machineCode.Count, p2.labelName);
                    }
                    machineCode.Add(p2.NextWordValue);

                }
            }
            else
            {
                // Non basic
                opCode = (uint)m_opDictionary[opCommand];                
                OpParamResult p1 = ParseParam(opParam1);
                opCode |= ((uint)p1.Param << 10) & 0xFC00;

                machineCode.Add((ushort)opCode);

                if (p1.nextWord != false)
                {
                    if (p1.labelName.Length > 0)
                    {
                        m_labelReferences.Add((ushort)machineCode.Count, p1.labelName);
                    }
                    machineCode.Add(p1.NextWordValue);

                }
            }
        }


        private void SetLabelAddressReferences()
        {
            // lets loop through all the locations where we have label references
            foreach (ushort key in m_labelReferences.Keys)
            {
                string labelName = m_labelReferences[key];
                if (m_labelAddressDitionary.ContainsKey(labelName) != true)
                {                    
                    throw new Exception(string.Format("Unknown label reference '{0}'",labelName));                    
                }

                machineCode[key] = m_labelAddressDitionary[labelName];
            }
        }

        public bool Assemble(string _filename)
        {
            try
            {
                if (File.Exists(_filename) != true)
                {
                    Console.WriteLine(string.Format("File '{0}' Not Found",_filename));
                    return false;
                }
                m_filename = _filename;
                machineCode.Clear();
                m_labelReferences.Clear();

                string[] lines = File.ReadAllLines(_filename);
                
                foreach (string line in lines)
                {
                    if (line.Trim().Length < 1) continue;
                    if (line[0] == ';') continue;
                    string processLine = line.Trim();                    
                    int commentIndex = 0;
                    commentIndex = line.IndexOf(";");
                    if (commentIndex > 0) processLine = line.Substring(0, commentIndex).Trim();
                    if (processLine.Trim().Length < 1) continue;

                    AssembleLine(processLine);

                }
                SetLabelAddressReferences();

                Console.WriteLine("Debug Dump");
                Console.WriteLine("*****");
                int count = 1;
                foreach (ushort code in machineCode)
                {
                    if (count % 4 == 0)
                        Console.WriteLine(code.ToString("X"));
                    else
                        Console.Write(code.ToString("X") + " ");
                    count++;
                }

                SaveOBJ();
            }
            catch (Exception E)
            {
                Console.WriteLine(string.Format("Exception: {0}",E.Message));
                return false;
            }
            return true;
        }

        private void SaveOBJ()
        {
            string saveFileName = m_filename.Split('.')[0] + ".obj";

            try
            {
                MemoryStream outfile = new MemoryStream();
                foreach(ushort word in machineCode)
                {
                    byte B = (byte)(word >> 8);
                    byte A = (byte)(word & 0xFF);

                    outfile.WriteByte(B);
                    outfile.WriteByte(A);                    
                }
                File.WriteAllBytes(saveFileName, outfile.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("EXCEPTION: {0}\nStackTrace: {1} ",e.Message,e.StackTrace));
                return;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Saved to '{0}",saveFileName));
        }
    }


   /// <summary>
   /// Program entry point. 
   /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("DCPU-16 ASM.NET Assember Version 0.1 Super-ALPHA");
            Console.WriteLine();

            if (args.Length < 1)
            {
                Console.WriteLine("usage: dcpu16-ASM <filename>");
                Console.WriteLine();
                //return; 
            }

            string filename = args[0];
            CDCPU16Assemble test = new CDCPU16Assemble();
            Console.WriteLine(string.Format("Assembling ASM file '{0}'",filename));

            test.Assemble(filename);


        }
    }
}
