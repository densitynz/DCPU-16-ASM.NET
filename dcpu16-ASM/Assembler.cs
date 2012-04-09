using System;
using System.Collections.Generic;
using System.IO;

namespace dcpu16_ASM
{
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
            m_opDictionary.Add("jsr", dcpuOpCode.JSR_OP);

            // basic instructions
            m_opDictionary.Add("set", dcpuOpCode.SET_OP);
            m_opDictionary.Add("add", dcpuOpCode.ADD_OP);
            m_opDictionary.Add("sub", dcpuOpCode.SUB_OP);
            m_opDictionary.Add("mul", dcpuOpCode.MUL_OP);
            m_opDictionary.Add("div", dcpuOpCode.DIV_OP);
            m_opDictionary.Add("mod", dcpuOpCode.MOD_OP);
            m_opDictionary.Add("shl", dcpuOpCode.SHL_OP);
            m_opDictionary.Add("shr", dcpuOpCode.SHR_OP);
            m_opDictionary.Add("and", dcpuOpCode.AND_OP);
            m_opDictionary.Add("bor", dcpuOpCode.BOR_OP);
            m_opDictionary.Add("xor", dcpuOpCode.XOR_OP);
            m_opDictionary.Add("ife", dcpuOpCode.IFE_OP);
            m_opDictionary.Add("ifn", dcpuOpCode.IFN_OP);
            m_opDictionary.Add("ifg", dcpuOpCode.IFG_OP);
            m_opDictionary.Add("ifb", dcpuOpCode.IFB_OP);

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

        private string[] SafeSplit(string data)
        {
            string inString = data.Replace("\t", " ").Replace(",", " ");
            string[] tmp = inString.Split(' ');
            List<string> dat = new List<string>();
            foreach (string t in tmp) if (t.Trim() != "") dat.Add(t);
            return dat.ToArray();
        }

        private OpParamResult ParseParam(string _param)
        {
            OpParamResult opParamResult = new OpParamResult();

            // Find easy ones. 
            string Param = _param.Replace(" ", "").Trim(); // strip spaces
            if (m_regDictionary.ContainsKey(Param) != false)
            {
                // Ok things are really easy in this case. 
                opParamResult.Param = (ushort)m_regDictionary[Param];
            }
            else
            {
                if (Param[0] == '[' && Param[Param.Length - 1] == ']')
                {
                    if (Param.Contains("+") != false)
                    {
                        string[] psplit = Param.Replace("[", "").Replace("]", "").Replace(" ", "").Split('+');
                        if (psplit.Length < 2)
                        {
                            throw new Exception(string.Format("malformated memory reference '{0}'", Param));
                        }
                        string addressValue = psplit[0];
                        if (m_regDictionary.ContainsKey("[+" + psplit[1] + "]") != true)
                        {
                            throw new Exception(string.Format("Invalid register reference in '{0}'", Param));
                        }
                        opParamResult.Param = (ushort)m_regDictionary["[+" + psplit[1] + "]"];
                        opParamResult.nextWord = true;
                        try
                        {
                            if (psplit[0].Contains("0x") != false)
                            {
                                ushort val = Convert.ToUInt16(psplit[0].Trim(), 16);
                                opParamResult.NextWordValue = (ushort)val;
                            }
                            else
                            {
                                ushort val = Convert.ToUInt16(psplit[0].Trim(), 10);
                                opParamResult.NextWordValue = (ushort)val;
                            }
                        }
                        catch
                        {

                            opParamResult.nextWord = true;
                            opParamResult.labelName = psplit[0].Trim();
                        }
                    }
                    else
                    {
                        opParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Mem;
                        opParamResult.nextWord = true;
                        try
                        {

                            if (Param.Contains("0x") != false)
                            {
                                ushort val = (ushort)Convert.ToUInt16(Param.Replace("[", "").Replace("]", "").Trim(), 16);
                                opParamResult.NextWordValue = val;
                            }
                            else
                            {
                                ushort val = (ushort)Convert.ToUInt16(Param.Replace("[", "").Replace("]", "").Trim(), 10);
                                opParamResult.NextWordValue = val;
                            }
                        }
                        catch
                        {

                            opParamResult.nextWord = true;
                            opParamResult.labelName = Param.Replace("[", "").Replace("]", "").Trim();
                        }
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
            string[] dataFields = _data.Substring(3, _data.Length - 3).Trim().Split(',');

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

        }

        private void AssembleLine(string _line)
        {
            if (_line.Trim().Length == 0) return;

            string line = _line.ToLower();

            if (line[0] == ':')
            {  // this is awful
                int sIndex = -1;
                int sIndex1 = line.IndexOf(' ');
                int sIndex2 = line.IndexOf('\t');
                if (sIndex1 < sIndex2 || sIndex2 == -1) sIndex = sIndex1;
                else if (sIndex2 < sIndex1 || sIndex1 != -1) sIndex = sIndex2;
                string labelName = line.Substring(1, line.Length - 1);
                if (sIndex > 1)
                    labelName = line.Substring(1, sIndex - 1);

                if (m_labelAddressDitionary.ContainsKey(labelName) != false)
                {
                    throw new Exception(string.Format("Error! Label '{0}' already exists!", labelName));
                }
                m_labelAddressDitionary.Add(labelName.Trim(), (ushort)machineCode.Count);

                if (sIndex < 0) return;

                line = line.Remove(0, sIndex).Trim();
                if (line.Length < 1) return;
            }

            string[] splitLine = SafeSplit(line);
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
                throw new Exception(string.Format("Illegal cpu opcode --> {0}", splitLine[0]));
            }
            opCode = (uint)m_opDictionary[opCommand] & 0xF;

            if (opCode > 0x0)
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
                    throw new Exception(string.Format("Unknown label reference '{0}'", labelName));
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
                    Console.WriteLine(string.Format("File '{0}' Not Found", _filename));
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
                Console.WriteLine(string.Format("Exception: {0}\n\tStackTrace: {1}", E.Message, E.StackTrace));
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
                foreach (ushort word in machineCode)
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
                Console.WriteLine(string.Format("EXCEPTION: {0}\nStackTrace: {1} ", e.Message, e.StackTrace));
                return;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Saved to '{0}", saveFileName));
        }
    }
}
