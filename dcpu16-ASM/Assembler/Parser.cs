/*
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
 * TODOs: 
 * - Far Far Better error checking (provide useful messages on compile failure
 * - 'safer' parsing. Speed coding this has resulted in some bad things
 * - Allow compile time arithmetic, for things like this: 
 *   :dataInMemory  0x9000 
 *   
 *      SET I, [dataInMemory + 0x1] 
 *   Right now you can only do something like this if you throw 0x1 into a seperate register :/ 
 */

namespace DCPU16_ASM.Assembler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Parser
    {
        private readonly List<ushort> machineCode;
        private readonly Dictionary<ushort, string> labelReferences;
        private readonly Dictionary<string, dcpuOpCode> opcodeDictionary;
        private readonly Dictionary<string, ushort> labelAddressDitionary;
        private readonly Dictionary<string, dcpuRegisterCodes> regiterDictionary;

        public Parser()
        {
            this.labelAddressDitionary = new Dictionary<string, ushort>();
            this.labelReferences = new Dictionary<ushort, string>();
            this.machineCode = new List<ushort>();

            this.opcodeDictionary = new Dictionary<string, dcpuOpCode>
                {
                    // non basic instructions
                    { "jsr", dcpuOpCode.JSR_OP },

                    // basic instructions
                    { "set", dcpuOpCode.SET_OP },
                    { "add", dcpuOpCode.ADD_OP },
                    { "sub", dcpuOpCode.SUB_OP },
                    { "mul", dcpuOpCode.MUL_OP },
                    { "div", dcpuOpCode.DIV_OP },
                    { "mod", dcpuOpCode.MOD_OP },
                    { "shl", dcpuOpCode.SHL_OP },
                    { "shr", dcpuOpCode.SHR_OP },
                    { "and", dcpuOpCode.AND_OP },
                    { "bor", dcpuOpCode.BOR_OP },
                    { "xor", dcpuOpCode.XOR_OP },
                    { "ife", dcpuOpCode.IFE_OP },
                    { "ifn", dcpuOpCode.IFN_OP },
                    { "ifg", dcpuOpCode.IFG_OP },
                    { "ifb", dcpuOpCode.IFB_OP },
                };

            // Register dictionary, We'll only include the most common ones in here, others have to be constructred. 
            this.regiterDictionary = new Dictionary<string, dcpuRegisterCodes>
                {
                    { "a", dcpuRegisterCodes.A },
                    { "b", dcpuRegisterCodes.B },
                    { "c", dcpuRegisterCodes.C },
                    { "x", dcpuRegisterCodes.X },
                    { "y", dcpuRegisterCodes.Y },
                    { "z", dcpuRegisterCodes.Z },
                    { "i", dcpuRegisterCodes.I },
                    { "j", dcpuRegisterCodes.J },
                    { "[a]", dcpuRegisterCodes.A_Mem },
                    { "[b]", dcpuRegisterCodes.B_Mem },
                    { "[c]", dcpuRegisterCodes.C_Mem },
                    { "[x]", dcpuRegisterCodes.X_Mem },
                    { "[y]", dcpuRegisterCodes.Y_Mem },
                    { "[z]", dcpuRegisterCodes.Z_Mem },
                    { "[i]", dcpuRegisterCodes.I_Mem },
                    { "[j]", dcpuRegisterCodes.J_Mem },
                    { "pop", dcpuRegisterCodes.POP },
                    { "peek", dcpuRegisterCodes.PEEK },
                    { "push", dcpuRegisterCodes.PUSH },
                    { "sp", dcpuRegisterCodes.SP },
                    { "pc", dcpuRegisterCodes.PC },
                    { "o", dcpuRegisterCodes.O },
                    { "[+a]", dcpuRegisterCodes.A_NextWord },
                    { "[+b]", dcpuRegisterCodes.B_NextWord },
                    { "[+c]", dcpuRegisterCodes.C_NextWord },
                    { "[+x]", dcpuRegisterCodes.X_NextWord },
                    { "[+y]", dcpuRegisterCodes.Y_NextWord },
                    { "[+z]", dcpuRegisterCodes.Z_NextWord },
                    { "[+i]", dcpuRegisterCodes.I_NextWord },
                    { "[+j]", dcpuRegisterCodes.J_NextWord }
                };
        }

        public string MessageOuput { get; private set; }

        public ushort[] Parse(ref string[] lines)
        {
            try
            {
                this.machineCode.Clear();
                this.labelReferences.Clear();
                this.MessageOuput = string.Empty;

                foreach (var line in lines)
                {
                    var currentLine = line.Trim();

                    if (currentLine.Length < 1 || line[0] == ';')
                    {
                        continue;
                    }

                    currentLine = this.RemoveLineComments(line);

                    if (currentLine.Trim().Length < 1)
                    {
                        continue;
                    }

                    this.AssembleLine(currentLine);
                }

                this.SetLabelAddressReferences();

                this.AddMessageLine("Debug Dump");
                this.AddMessageLine("*****");
                var count = 1;

                foreach (var code in this.machineCode)
                {
                    if (count % 4 == 0)
                    {
                        this.AddMessageLine(string.Format("{0:X4}", code));
                    }
                    else
                    {
                        this.AddMessage(string.Format("{0:X4} ", code));
                    }

                    count++;
                }

                return this.machineCode.ToArray();
            }
            catch (Exception ex)
            {
                this.AddMessageLine(string.Format("{0}", ex.Message));
                return null;
            }
        }

        private string RemoveLineComments(string line)
        {
            var clearedLine = line;

            var commentIndex = line.IndexOf(";");

            if (commentIndex > 0)
            {
                clearedLine = line.Substring(0, commentIndex).Trim();
            }

            return clearedLine;
        }

        private void AssembleLine(string line)
        {
            line = line.ToLower();

            if (line[0] == ':')
            {
                var remaiderLineContentIndex = this.ParseLabel(line);

                if (remaiderLineContentIndex <= 0)
                {
                    return;
                }

                line = line.Remove(0, remaiderLineContentIndex).Trim();

                if (line.Length < 1)
                {
                    return;
                }
            }

            var tokens = this.Tokenize(line);

            var candidateOpcode = tokens[0].Trim();

            if (candidateOpcode.ToLower() == "dat")
            {
                this.ParseDat(line);
                return;
            }

            if (!this.opcodeDictionary.ContainsKey(candidateOpcode))
            {
                throw new Exception(string.Format("Illegal cpu opcode --> {0}", tokens[0]));
            }

            var opcode = (uint)this.opcodeDictionary[candidateOpcode] & 0xF;
            var param = tokens[1].Trim();

            if (opcode > 0x0)
            {
                var param1 = tokens[2]; // basic function, has second param.
                var p1 = this.ParseParam(param);
                var p2 = this.ParseParam(param1);

                opcode |= ((uint)p1.Param << 4) & 0x3F0;
                opcode |= ((uint)p2.Param << 10) & 0xFC00;

                this.machineCode.Add((ushort)opcode);

                if (p1.NextWord)
                {
                    if (p1.LabelName.Length > 0)
                    {
                        this.labelReferences.Add((ushort)this.machineCode.Count, p1.LabelName);
                    }

                    this.machineCode.Add(p1.NextWordValue);
                }

                if (p2.NextWord)
                {
                    if (p2.LabelName.Length > 0)
                    {
                        this.labelReferences.Add((ushort)this.machineCode.Count, p2.LabelName);
                    }

                    this.machineCode.Add(p2.NextWordValue);
                }
            }
            else
            {
                opcode = (uint)this.opcodeDictionary[candidateOpcode];
                OpcodeParamResult p1 = this.ParseParam(param);
                opcode |= ((uint)p1.Param << 10) & 0xFC00;

                this.machineCode.Add((ushort)opcode);

                if (p1.NextWord)
                {
                    if (p1.LabelName.Length > 0)
                    {
                        this.labelReferences.Add((ushort)this.machineCode.Count, p1.LabelName);
                    }

                    this.machineCode.Add(p1.NextWordValue);
                }
            }
        }

        private int ParseLabel(string line)
        {
            var index1 = line.IndexOf(' ');
            var index2 = line.IndexOf('\t');
            var index = index1 < index2 || index2 == -1 ? index1 : index2 < index1 || index1 != -1 ? index2 : -1;

            var labelName = index > 1 ? line.Substring(1, index - 1) : line.Substring(1, line.Length - 1);

            if (this.labelAddressDitionary.ContainsKey(labelName))
            {
                throw new Exception(string.Format("Error! Label '{0}' already exists!", labelName));
            }

            this.labelAddressDitionary.Add(labelName.Trim(), (ushort)this.machineCode.Count);

            return index;
        }

        private string[] Tokenize(string data)
        {
            var tokens = data.Split(new[] { ' ', '\t', ',' });
            return tokens.Where(t => t.Trim() != string.Empty).ToArray();
        }

        private OpcodeParamResult ParseParam(string param)
        {
            var opcodeParamResult = new OpcodeParamResult();

            var clearedParameter = param.Replace(" ", string.Empty).Trim();

            if (this.regiterDictionary.ContainsKey(clearedParameter))
            {
                // Ok things are really easy in this case. 
                opcodeParamResult.Param = (ushort)this.regiterDictionary[clearedParameter];
            }
            else
            {
                if (clearedParameter[0] == '[' && clearedParameter[clearedParameter.Length - 1] == ']')
                {
                    if (clearedParameter.Contains("+"))
                    {
                        var psplit = clearedParameter.Replace("[", string.Empty).Replace("]", string.Empty).Replace(" ", string.Empty).Split('+');
                        if (psplit.Length < 2)
                        {
                            throw new Exception(string.Format("malformated memory reference '{0}'", clearedParameter));
                        }

                        var addressValue = psplit[1];
                        if (!this.regiterDictionary.ContainsKey("[+" + addressValue + "]"))
                        {
                            throw new Exception(string.Format("Invalid register reference in '{0}'", clearedParameter));
                        }

                        opcodeParamResult.Param = (ushort)this.regiterDictionary["[+" + psplit[1] + "]"];
                        opcodeParamResult.NextWord = true;

                        // nasty
                        if (psplit[0][0] == '\'' && psplit[0][psplit[0].Length - 1] == '\'' && psplit[0].Length == 3)
                        {
                            var val = (ushort)psplit[0][1];
                            opcodeParamResult.NextWordValue = val;
                        }
                        else if (psplit[0].Contains("0x"))
                        {
                            ushort val = Convert.ToUInt16(psplit[0].Trim(), 16);
                            opcodeParamResult.NextWordValue = val;
                        }
                        else if (psplit[0].Trim().All(x => char.IsDigit(x)))
                        {
                            var val = Convert.ToUInt16(psplit[0].Trim(), 10);
                            opcodeParamResult.NextWordValue = val;
                        }
                        else
                        {
                            opcodeParamResult.NextWord = true;
                            opcodeParamResult.LabelName = psplit[0].Trim();
                        }
                    }
                    else
                    {
                        opcodeParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Mem;
                        opcodeParamResult.NextWord = true;

                        // nasty
                        if (clearedParameter[1] == '\'' && clearedParameter[clearedParameter.Length - 2] == '\'' && clearedParameter.Length == 5)
                        {
                            ushort val = clearedParameter[1];
                            opcodeParamResult.NextWordValue = val;
                        }
                        else if (clearedParameter.Contains("0x"))
                        {
                            ushort val = Convert.ToUInt16(clearedParameter.Replace("[", string.Empty).Replace("]", string.Empty).Trim(), 16);
                            opcodeParamResult.NextWordValue = val;
                        }
                        else if (clearedParameter.Trim().All(x => char.IsDigit(x)))
                        {
                            ushort val = Convert.ToUInt16(clearedParameter.Replace("[", string.Empty).Replace("]", string.Empty).Trim(), 10);
                            opcodeParamResult.NextWordValue = val;
                        }
                        else
                        {
                            opcodeParamResult.NextWord = true;
                            opcodeParamResult.LabelName = clearedParameter.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                        }
                    }
                }
                else
                {
                    // if value is < 0x1F we can encode it into the param directly, 
                    // else it has to be next value!
                    ushort maxValue = Convert.ToUInt16("0x1F", 16);

                    ushort literalValue;
                    if (clearedParameter[0] == '\'' && clearedParameter[clearedParameter.Length - 1] == '\'' && clearedParameter.Length == 3)
                    {
                        literalValue = clearedParameter[1];
                    }
                    else if (clearedParameter.Contains("0x"))
                    {
                        literalValue = Convert.ToUInt16(clearedParameter, 16);
                    }
                    else if (clearedParameter.Trim().All(x => char.IsDigit(x)))
                    {
                        literalValue = Convert.ToUInt16(clearedParameter, 10);
                    }
                    else
                    {
                        opcodeParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                        opcodeParamResult.NextWord = true;
                        opcodeParamResult.LabelName = clearedParameter;
                        return opcodeParamResult;
                    }

                    if (literalValue < maxValue)
                    {
                        opcodeParamResult.Param = Convert.ToUInt16("0x20", 16);
                        opcodeParamResult.Param += literalValue;
                    }
                    else
                    {
                        opcodeParamResult.Param = (ushort)dcpuRegisterCodes.NextWord_Literal_Value;
                        opcodeParamResult.NextWord = true;
                        opcodeParamResult.NextWordValue = literalValue;
                    }
                }
            }

            return opcodeParamResult;
        }

        private void ParseDat(string line)
        {
            var dataFields = new List<string>();

            foreach (var field in line.Substring(3, line.Length - 3).Trim().Split(','))
            {
                if (dataFields.Count == 0)
                {
                    dataFields.Add(field);
                }
                else
                {
                    var count = 0;
                    var last = -1;
                    var lastStr = dataFields[dataFields.Count - 1];

                    while ((last = lastStr.IndexOf('\"', last + 1)) != -1)
                    {
                        count++;
                    }

                    if (count == 1)
                    {
                        dataFields[dataFields.Count - 1] += "," + field;
                    }
                    else
                    {
                        dataFields.Add(field);
                    }
                }
            }

            foreach (var dat in dataFields)
            {
                var valStr = dat.Trim();
                if (valStr.IndexOf('"') > -1)
                {
                    var asciiLine = dat.Replace("\"", string.Empty).Trim();
                    foreach (var t in asciiLine)
                    {
                        this.machineCode.Add(t);
                    }
                }
                else
                {
                    var val = valStr.Contains("0x") ? Convert.ToUInt16(valStr, 16) : Convert.ToUInt16(valStr, 10);
                    this.machineCode.Add(val);
                }
            }
        }

        private void SetLabelAddressReferences()
        {
            // lets loop through all the locations where we have label references
            foreach (ushort key in this.labelReferences.Keys)
            {
                var labelName = this.labelReferences[key];

                if (!this.labelAddressDitionary.ContainsKey(labelName))
                {
                    throw new Exception(string.Format("Unknown label reference '{0}'", labelName));
                }

                this.machineCode[key] = this.labelAddressDitionary[labelName];
            }
        }

        private void AddMessageLine(string input)
        {
            this.MessageOuput += string.Format("{0}\r\n", input);
        }

        private void AddMessage(string input)
        {
            this.MessageOuput += input;
        }
    }
}
