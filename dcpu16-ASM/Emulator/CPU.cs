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

// CPU Emulation code is here. 
// Again another Speed job so the code isn't terribly flash.

/**
 * WORK IN PROGRESS - 10th April 2012
 * - DensitY
 */


using System;
using System.Collections.Generic;
using System.IO;

namespace dcpu16_ASM.Emulator
{

    struct cpuMemory
    {
        public ushort[] RAM;

        public cpuMemory(uint _ramSize)
        {
            RAM = new ushort[_ramSize];            
        }
    }

    struct cpuRegisters
    {
        // General purpose registers
        public ushort[] GP;

        public ushort SP; // StackPointer
        public ushort PC; // Program Counter

        public ushort O;  // Overflow Register
    }


    class CDCPU16
    {
        private bool m_ignoreNextInstruction = false;

        private bool m_programLoaded = false; 

        private cpuRegisters m_registers;
        private cpuMemory m_memory;

        struct readParamValue
        {
            public bool Mem;
            public ushort Location;
            public ushort Value;
        }

        public CDCPU16()
        {
            InitCPU();
        }

        public CDCPU16(ref List<ushort> _machineCode)
        {
            InitCPU();
            SetProgram(ref _machineCode);
        }

        private void InitCPU()
        {
            // Initialize Memory
            m_memory.RAM = new ushort[0xFFFF];
            Array.Clear(m_memory.RAM, 0, 0xFFFF);

            // Initialize CPU registers
            m_registers.GP = new ushort[8];
            Array.Clear(m_registers.GP, 0, 8);

            m_registers.SP = 0xFFFF;
            m_registers.PC = 0x0;
            m_registers.O = 0;
        }

        public void SetProgram(ref List<ushort> _machineCode)
        {
            if (_machineCode == null) return;
            /*
             * TODO: once memory standards have been outline, ensure program fits within them! 
             */
            for (int i = 0; i < _machineCode.Count; i++)
            {
                m_memory.RAM[i] = _machineCode[i];
            }

            m_programLoaded = true;
        }


        private void SetResultValue(readParamValue _resultValue)
        {
            if (_resultValue.Mem != false)
            {
                // write to memory location
                m_memory.RAM[_resultValue.Location] = _resultValue.Value;
            }
            else
            {
                // Write to respective register
                switch (_resultValue.Location)
                {
                    case (ushort)dcpuRegisterCodes.A:
                    case (ushort)dcpuRegisterCodes.B:
                    case (ushort)dcpuRegisterCodes.C:
                    case (ushort)dcpuRegisterCodes.X:
                    case (ushort)dcpuRegisterCodes.Y:
                    case (ushort)dcpuRegisterCodes.Z:
                    case (ushort)dcpuRegisterCodes.I:
                    case (ushort)dcpuRegisterCodes.J:
                        m_registers.GP[_resultValue.Location] = _resultValue.Value;
                        break;

                    case (ushort)dcpuRegisterCodes.PC:  // program Counter
                        m_registers.PC = _resultValue.Value;
                        break;

                    case (ushort)dcpuRegisterCodes.SP:  // stack pointer
                        m_registers.SP = _resultValue.Value;
                        break;

                    case (ushort)dcpuRegisterCodes.O:   // overflow reigster
                        m_registers.O = _resultValue.Value;
                        break;

                    default:
                        break;
                }
            }
                
        }

        private readParamValue ReadParamValue(ushort _opParam)
        {
            readParamValue result = new readParamValue();            

            switch (_opParam)
            {
                // read Value in register
                case (ushort)dcpuRegisterCodes.A:
                case (ushort)dcpuRegisterCodes.B:
                case (ushort)dcpuRegisterCodes.C:
                case (ushort)dcpuRegisterCodes.X:
                case (ushort)dcpuRegisterCodes.Y:
                case (ushort)dcpuRegisterCodes.Z:
                case (ushort)dcpuRegisterCodes.I:
                case (ushort)dcpuRegisterCodes.J:                    
                    result.Mem = false;
                    result.Location = _opParam;
                    result.Value = m_registers.GP[result.Location];
                    break;

                // read value at memory location where register is pointing too
                case (ushort)dcpuRegisterCodes.A_Mem:
                case (ushort)dcpuRegisterCodes.B_Mem:
                case (ushort)dcpuRegisterCodes.C_Mem:
                case (ushort)dcpuRegisterCodes.X_Mem:
                case (ushort)dcpuRegisterCodes.Y_Mem:
                case (ushort)dcpuRegisterCodes.Z_Mem:
                case (ushort)dcpuRegisterCodes.I_Mem:
                case (ushort)dcpuRegisterCodes.J_Mem:                    
                    result.Mem = true;
                    result.Location = m_registers.GP[_opParam - (ushort)dcpuRegisterCodes.A_Mem];
                    result.Value = m_memory.RAM[result.Location];
                    break;

                case (ushort)dcpuRegisterCodes.A_NextWord:
                case (ushort)dcpuRegisterCodes.B_NextWord:
                case (ushort)dcpuRegisterCodes.C_NextWord:
                case (ushort)dcpuRegisterCodes.X_NextWord:
                case (ushort)dcpuRegisterCodes.Y_NextWord:
                case (ushort)dcpuRegisterCodes.Z_NextWord:
                case (ushort)dcpuRegisterCodes.I_NextWord:
                case (ushort)dcpuRegisterCodes.J_NextWord:                                        
                    result.Mem = true;
                    result.Location = (ushort)(m_registers.GP[_opParam - (ushort)dcpuRegisterCodes.A_NextWord] + ReadNextWord());
                    result.Value = m_memory.RAM[result.Location];
                    break;

                case (ushort)dcpuRegisterCodes.POP:
                    result.Mem = true;
                    result.Location = m_registers.SP++;
                    result.Value = m_memory.RAM[result.Location];     
                    break;

                case (ushort)dcpuRegisterCodes.PEEK:
                    result.Mem = true;
                    result.Location = m_registers.SP;
                    result.Value = m_memory.RAM[result.Location];                     
                    break;

                case (ushort)dcpuRegisterCodes.PUSH:
                    result.Mem = true;
                    result.Location = --m_registers.SP;
                    result.Value = m_memory.RAM[result.Location]; 
                    break;

                case (ushort)dcpuRegisterCodes.O:
                    result.Mem = false;
                    result.Location = _opParam;
                    result.Value = m_registers.O;
                    break;

                case (ushort)dcpuRegisterCodes.PC:
                    result.Mem = false;
                    result.Location = _opParam;
                    result.Value = m_registers.PC;
                    break;

                case (ushort)dcpuRegisterCodes.SP:
                    result.Mem = false;
                    result.Location = _opParam;
                    result.Value = m_registers.SP;
                    break;

                case (ushort)dcpuRegisterCodes.NextWord_Literal_Mem:
                    result.Mem = true;
                    result.Location = ReadNextWord();
                    result.Value = m_memory.RAM[result.Location];                    
                    break;

                /*
                 * DensitY here once again :)
                 * 
                 * We're going todo something special with the result
                 * basically someone might do something like this
                 * SET 0x8000, I
                 * when they mean
                 * SET [0x8000], I
                 * So in case we do that, we'll handle it. 
                 * 
                 * SAYING THAT!
                 * SET I, 0x8000
                 * and 
                 * SET I, [0x8000]
                 * Will work properly! (first putting 0x8000 into the register I, second reading data from the memory location 0x8000 into register I)
                 */
                case (ushort)dcpuRegisterCodes.NextWord_Literal_Value:
                    result.Mem = true; // Read above to see why I'm doing this!
                    result.Location = ReadNextWord();
                    result.Value = result.Location;
                    break;

                default:
                    break;
            }
            // Special Case for literal Values that are stored in the byte's param and not next word. used for values < 0x1F. 
            if (_opParam >= 0x20 && _opParam < 0x3F)
            {
                result.Mem = true; // See the large comment above to why I'm doing this. Although values this low are going to overwrite program memory <_<
                result.Location = (ushort)(_opParam - 0x20);
                result.Value = result.Location;
            }

            return result;
        }

        private ushort ReadNextWord()
        {
            return m_memory.RAM[m_registers.PC++];
        }

        public void ExecuteInstruction()
        {
            if (m_programLoaded != true) return;

            ushort programWord = ReadNextWord();
            ushort opCode   = (ushort)((int)programWord & 0xF);
            ushort opP1     = (ushort)((int)(programWord >> 4) & 0x3F);     // A = Dest
            ushort opP2     = (ushort)((int)(programWord >> 10) & 0x3F);    // B = Source

            readParamValue A = ReadParamValue(opP1);
            readParamValue B = ReadParamValue(opP2);

            if (m_ignoreNextInstruction != false)
            {
                m_ignoreNextInstruction = false;
                return;
            }

            int tmpVal = 0;

            switch (opCode)
            {
                // non basic instruction
                case (ushort)dcpuOpCode.NB_OP:
                    /*
                     * For non basic instructions, the instruction is actually
                     * included in param A's spot (6bits)
                     */
                    switch (opP1)
                    {
                        // JSR instruction
                        case ((ushort)dcpuOpCode.JSR_OP >> 4):                            
                            m_memory.RAM[--m_registers.SP] = m_registers.PC;                            
                            m_registers.PC = (ushort)(B.Value);
                            break;
                        default:
                            Console.WriteLine(string.Format("Illegal Non-Basic instruction {0} at Address {1}", opP1.ToString("X"), m_registers.PC.ToString("X")));
                            break;
                    }                                        
                    break;

                // Assignment Instruction
                case (ushort)dcpuOpCode.SET_OP:                    
                    A.Value = B.Value;
                    SetResultValue(A);
                    break;

                // Basic Arithmetic instructions
                case (ushort)dcpuOpCode.ADD_OP:                    
                    tmpVal = A.Value + B.Value;
                    if (tmpVal > 0xFFFF) m_registers.O = 0x0001;
                    A.Value = (ushort)tmpVal;
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.SUB_OP:                    
                    tmpVal = A.Value - B.Value;
                    if (tmpVal < 0) m_registers.O = 0xFFFF;
                    A.Value = (ushort)tmpVal;
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.MUL_OP:
                    A.Value *= B.Value;
                    m_registers.O = (ushort)((A.Value >> 16) & 0xFFFF);
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.DIV_OP:
                    if (B.Value == 0)
                    {
                        A.Value = 0; m_registers.O = 0;
                    }
                    else
                    {
                        m_registers.O = (ushort)(((A.Value << 16) / B.Value) & 0xFFFF);
                        A.Value /= B.Value;
                    }
                    SetResultValue(A);
                    break;

                // Boolean Instructions
                case (ushort)dcpuOpCode.MOD_OP:
                    if (B.Value == 0)
                    {
                        A.Value = 0;
                    }
                    else
                    {
                        A.Value %= B.Value;
                    }

                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.SHL_OP:
                    A.Value <<= B.Value;
                    m_registers.O = (ushort)(((A.Value << B.Value) >> 16) & 0xFFFF);
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.SHR_OP:
                    A.Value >>= B.Value;
                    m_registers.O = (ushort)(((A.Value << 16) >> B.Value) & 0xFFFF);
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.AND_OP:
                    A.Value &= B.Value;
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.BOR_OP:
                    A.Value |= B.Value;
                    SetResultValue(A);
                    break;
                case (ushort)dcpuOpCode.XOR_OP:
                    A.Value ^= B.Value;
                    SetResultValue(A);
                    break;

                // Branch Instructions
                /*
                 * Basically skip the next instruction on failure
                 */
                case (ushort)dcpuOpCode.IFE_OP:
                    if ((A.Value == B.Value) != true) m_ignoreNextInstruction = true;
                    break;
                case (ushort)dcpuOpCode.IFN_OP:
                    if ((A.Value != B.Value) != true) m_ignoreNextInstruction = true;
                    break;
                case (ushort)dcpuOpCode.IFG_OP:
                    if ((A.Value > B.Value) != true) m_ignoreNextInstruction = true;
                    break;
                case (ushort)dcpuOpCode.IFB_OP:
                    if (((A.Value & B.Value) != 0) != true) m_ignoreNextInstruction = true;
                    break;

                default:
                    Console.WriteLine(string.Format("Illegal Basic instruction {0} at Address {1}",opCode.ToString("X"),m_registers.PC.ToString("X")));
                    break;
            }

            int d = 0; // I throw break points on these.

            // DEBUG! 
            DebugShowRegisters();
            Console.ReadLine();
            // TODO: show memory map
            // END DEBUG
        }

        public bool ProgramLoaded
        {
            get { return m_programLoaded; }
            set { m_programLoaded = value; }
        }

        public void DebugShowRegisters()
        {
            Console.WriteLine("----------------");
            Console.WriteLine("Register Info");
            Console.WriteLine(string.Format("\tA = {0},\tB = {1},\tC = {2}",
                m_registers.GP[(int)dcpuRegisterCodes.A].ToString("X"),
                m_registers.GP[(int)dcpuRegisterCodes.B].ToString("X"),
                m_registers.GP[(int)dcpuRegisterCodes.C].ToString("X")));

            Console.WriteLine(string.Format("\tX = {0},\tY = {1},\tZ = {2}",
                m_registers.GP[(int)dcpuRegisterCodes.X].ToString("X"),
                m_registers.GP[(int)dcpuRegisterCodes.Y].ToString("X"),
                m_registers.GP[(int)dcpuRegisterCodes.Z].ToString("X")));

            Console.WriteLine(string.Format("\tI = {0},\tJ = {1}",
                m_registers.GP[(int)dcpuRegisterCodes.I].ToString("X"),
                m_registers.GP[(int)dcpuRegisterCodes.J].ToString("X")));

            Console.WriteLine(string.Format("\tPC = {0},\tSP = {1},\tO = {2}",
                m_registers.PC.ToString("X"),
                m_registers.SP.ToString("X"),
                m_registers.O.ToString("X")));
    

        }
    }
}