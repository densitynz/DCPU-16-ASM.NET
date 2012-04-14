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


using System;
using System.Collections.Generic;
using System.IO;

namespace DCPU16_ASM.Emulator
{

    /// <summary>
    /// DCPU-16 Memory struct
    /// </summary>
    public struct cpuMemory
    {
        /// <summary>
        /// unsigned 16bit Ram array.         
        /// </summary>
        public ushort[] RAM;

        public cpuMemory(uint _ramSize)
        {
            RAM = new ushort[_ramSize];            
        }
    }

    /// <summary>
    /// DCPU-16 Registers struct
    /// </summary>
    public struct cpuRegisters
    {        
        /// <summary>
        /// General purpose Registers
        /// </summary>
        public ushort[] GP;

        /// <summary>
        /// Stack Pointer
        /// </summary>
        public ushort SP;
        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort PC;

        /// <summary>
        /// Overflow Register
        /// </summary>
        public ushort O;
    }

    /// <summary>
    /// DCPU-16 Computer class    
    /// </summary>
    public class CDCPU16
    {
        /// <summary>
        /// If true, CPU will ignore next instruction
        /// </summary>
        private bool m_ignoreNextInstruction = false;

        /// <summary>
        /// IF true, a DCPU-16 Program has been loaded into RAM
        /// </summary>
        private bool m_programLoaded = false; 

        /// <summary>
        /// DCPU-16 Registers 
        /// </summary>
        private cpuRegisters m_registers;
        
        /// <summary>
        /// DCPU-16 Memory
        /// </summary>
        private cpuMemory m_memory;

        public cpuRegisters Registers
        {
            get { return m_registers; }
        }

        public cpuMemory Memory
        {
            get { return m_memory; }
        }

        /* 
         * Cycle counting notes
         * 
         * From the Notch Bible: 
         * 
         * "
         * SET, AND, BOR and XOR take 1 cycle, plus the cost of a and b
         * ADD, SUB, MUL, SHR, and SHL take 2 cycles, plus the cost of a and b
         * DIV and MOD take 3 cycles, plus the cost of a and b
         * IFE, IFN, IFG, IFB take 2 cycles, plus the cost of a and b, plus 1 if the test fails
         * JSR takes 2 cycles, plus the cost of a.
         * 
         * All values that read a word (0x10-0x17, 0x1e, and 0x1f) take 1 cycle to look up. The rest take 0 cycles.
         * "
         * Some operations require 2 WORD reads. one or next value, one for memory lookup. I'm assuming that BOTH
         * tasks take 1 cycle each (for a total of 2), else it simply won't make any sense.
         */
        /// <summary>
        /// DCPU-16 Cycle counter
        /// </summary>
        private long m_cycles = 0;


        /// <summary>
        /// Parameter Read and Store location type
        /// Writing to literal types will be ignored
        /// </summary>
        enum ParamType
        {
            /// <summary>
            /// Parameter is stored in Memory
            /// </summary>
            Memory,
            /// <summary>
            /// Parameter is stored in a Register
            /// </summary>
            Register,
            /// <summary>
            /// Parameter is stored as a literal value 
            /// (either in the parameter or next word)
            /// </summary>
            Literal
        }

        /// <summary>
        /// Read Parameter Values
        /// </summary>
        struct readParamValue
        {            
            /// <summary>
            /// Parameter Read/Store Location type
            /// </summary>
            public ParamType ParmType;
            /// <summary>
            /// Parameter's read location. This could be 
            /// a Register or Memory location.
            /// </summary>
            public ushort Location;
            /// <summary>
            /// Actual Parameter Value
            /// </summary>
            public ushort Value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CDCPU16()
        {
            InitCPU();
        }

        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="_machineCode">list array containing DCPU-16 Program</param>
        public CDCPU16(ref List<ushort> _machineCode)
        {
            InitCPU();
            SetProgram(ref _machineCode);
        }

        /// <summary>
        /// Initialize DCPU-16 CPU.
        /// 
        /// Allocates required memory space for RAM and General purpose Registers
        /// </summary>
        public void InitCPU()
        {
            // Initialize Memory
            m_memory.RAM = new ushort[0x10000];
            Array.Clear(m_memory.RAM, 0, 0x10000);

            // Initialize CPU registers
            m_registers.GP = new ushort[8];
            Array.Clear(m_registers.GP, 0, 8);

            m_registers.SP = 0x0;
            m_registers.PC = 0x0;
            m_registers.O = 0;

            m_cycles = 0;
        }

        /// <summary>
        /// Resets DCPU-16's Registers.
        /// 
        /// NOTE: we do Video/Keyboard buffer clearing as well. Will shift soon :)
        /// </summary>
        public void ResetCPURegisters()
        {
            // Reset registers
            if (m_registers.GP == null) m_registers.GP = new ushort[8];

            Array.Clear(m_registers.GP, 0, 8);

            m_registers.SP = 0x0;
            m_registers.PC = 0x0;
            m_registers.O = 0;

            m_cycles = 0;

            Array.Clear(m_memory.RAM, (int)dcpuMemoryLayout.VIDEO_TEXT_START, (int)(dcpuMemoryLayout.VIDEO_TEXT_END+1 - dcpuMemoryLayout.VIDEO_TEXT_START));
            Array.Clear(m_memory.RAM, (int)dcpuMemoryLayout.KEYBOARD_START, (int)(dcpuMemoryLayout.KEYBOARD_END+1 - dcpuMemoryLayout.KEYBOARD_START));
        }

        /// <summary>
        /// Clears Video/Keyboard (and more to come) buffers
        /// </summary>
        public void ClearMemoryBuffers()
        {
            Array.Clear(m_memory.RAM, (int)dcpuMemoryLayout.VIDEO_TEXT_START, (int)(dcpuMemoryLayout.VIDEO_TEXT_END - dcpuMemoryLayout.VIDEO_TEXT_START));
            Array.Clear(m_memory.RAM, (int)dcpuMemoryLayout.KEYBOARD_START, (int)(dcpuMemoryLayout.KEYBOARD_END - dcpuMemoryLayout.KEYBOARD_START));
        }

        /// <summary>
        /// Sets DPU-16 up with a program to execute
        /// </summary>
        /// <param name="_machineCode">list array containing DCPU-16 Program</param>
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

        /// <summary>
        /// Stores Read param values back into RAM or Registers.
        /// This is normally called after resolving a OpCode instruction in
        /// ExecuteInstruction().
        /// </summary>
        /// <param name="_resultValue">read param value</param>
        private void SetResultValue(readParamValue _resultValue)
        {
            if (_resultValue.ParmType == ParamType.Memory)
            {
                // write to memory location
                m_memory.RAM[_resultValue.Location] = _resultValue.Value;

                /**
                 * Notch's spec doesn't say anything (that I can find) regarding cost of write backs. So I'm assuming
                 * they are the same as reads. Meaning registers are free, Memory/word writebacks cost 1 cycle
                 */
                m_cycles++; 
            }
            else if (_resultValue.ParmType == ParamType.Register)
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
            // ignore literal assignment as per Notch's spec                
        }

        /// <summary>
        /// Read OpCode Parameter value. 
        /// 
        /// BBBBBB AAAAAA 0000
        /// 
        /// Will decode either A or B (based on what is given in _opParam) and
        /// populate data into a readParamValue struct. 
        /// 
        /// The PC register is automatically incremented based on what needs to be read.
        /// </summary>
        /// <param name="_opParam"></param>
        /// <returns></returns>
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
                    result.ParmType = ParamType.Register;
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
                    result.ParmType = ParamType.Memory;
                    result.Location = m_registers.GP[_opParam - (ushort)dcpuRegisterCodes.A_Mem];
                    result.Value = m_memory.RAM[result.Location];
                    m_cycles++;
                    break;

                case (ushort)dcpuRegisterCodes.A_NextWord:
                case (ushort)dcpuRegisterCodes.B_NextWord:
                case (ushort)dcpuRegisterCodes.C_NextWord:
                case (ushort)dcpuRegisterCodes.X_NextWord:
                case (ushort)dcpuRegisterCodes.Y_NextWord:
                case (ushort)dcpuRegisterCodes.Z_NextWord:
                case (ushort)dcpuRegisterCodes.I_NextWord:
                case (ushort)dcpuRegisterCodes.J_NextWord:
                    result.ParmType = ParamType.Memory;
                    result.Location = (ushort)(m_registers.GP[_opParam - (ushort)dcpuRegisterCodes.A_NextWord] + ReadNextWord());
                    result.Value = m_memory.RAM[result.Location];
                    m_cycles += 2; // we read 2 words so this must cost 2 cycles. 
                    break;

                case (ushort)dcpuRegisterCodes.POP:
                    if (m_ignoreNextInstruction != true)
                    {
                        result.ParmType = ParamType.Memory;
                        result.Location = m_registers.SP++;
                        result.Value = m_memory.RAM[result.Location];
                    }
                    m_cycles++;
                    break;

                case (ushort)dcpuRegisterCodes.PEEK:
                    result.ParmType = ParamType.Memory;
                    result.Location = m_registers.SP;
                    result.Value = m_memory.RAM[result.Location];
                    m_cycles++; 
                    break;

                case (ushort)dcpuRegisterCodes.PUSH:
                    if (m_ignoreNextInstruction != true)
                    {
                        result.ParmType = ParamType.Memory;
                        result.Location = --m_registers.SP;
                        result.Value = m_memory.RAM[result.Location];
                    }
                    m_cycles++;
                    break;

                case (ushort)dcpuRegisterCodes.O:
                    result.ParmType = ParamType.Register;
                    result.Location = _opParam;
                    result.Value = m_registers.O;
                    break;

                case (ushort)dcpuRegisterCodes.PC:
                    result.ParmType = ParamType.Register;
                    result.Location = _opParam;
                    result.Value = m_registers.PC;
                    break;

                case (ushort)dcpuRegisterCodes.SP:
                    result.ParmType = ParamType.Register;
                    result.Location = _opParam;
                    result.Value = m_registers.SP;
                    break;

                case (ushort)dcpuRegisterCodes.NextWord_Literal_Mem:
                    result.ParmType = ParamType.Memory;
                    result.Location = ReadNextWord();
                    result.Value = m_memory.RAM[result.Location];     
                    m_cycles += 2; // we read 2 words so this must cost 2 cycles. 
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
                 * 
                 * ----------------------------------------------------------------------------------
                 * UPDATE: DensitY 11th April 2012
                 * From Notch's Bible
                 * "If any instruction tries to assign a literal value, the assignment fails silently. Other than that, the instruction behaves as normal."
                 *  Basically what I decided on doing above was against Notch's spec, so I'm taking it out.
                 *  ---------------------------------------------------------------------------------
                 */
                case (ushort)dcpuRegisterCodes.NextWord_Literal_Value:
                    result.ParmType = ParamType.Literal;
                    result.Location = ReadNextWord();
                    result.Value = result.Location;
                    m_cycles++;
                    break;

                default:
                    break;
            }
            // Special Case for literal Values that are stored in the byte's param and not next word. used for values < 0x1F. 
            if (_opParam >= 0x20 && _opParam <= 0x3F)
            {
                result.ParmType = ParamType.Literal; 
                result.Location = (ushort)(_opParam - 0x20);
                result.Value = result.Location;
                // cycle free!
            }

            return result;
        }

        /// <summary>
        /// Read next Word from DCPU-16's ram @ the program counter's Location.
        /// </summary>
        /// <returns></returns>
        private ushort ReadNextWord()
        {
            return m_memory.RAM[m_registers.PC++];
        }

        static int dd = 0;
        /// <summary>
        /// Execute a DCPU-16 Instruction
        /// </summary>
        public void ExecuteInstruction()
        {
            if (m_programLoaded != true) return;

            dd++;
            ushort programWord = ReadNextWord();
            ushort opCode   = (ushort)((int)programWord & 0xF);
            ushort opP1     = (ushort)((int)(programWord >> 4) & 0x3F);     // A = Dest
            ushort opP2     = (ushort)((int)(programWord >> 10) & 0x3F);    // B = Source

            readParamValue A = ReadParamValue(opP1);
            readParamValue B = ReadParamValue(opP2);

            if (m_ignoreNextInstruction != false)
            {
                m_ignoreNextInstruction = false;

                // branch failure generates a 1 cycle penality.
                m_cycles++;                
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
                            m_memory.RAM[--m_registers.SP] = (ushort)(m_registers.PC);                            
                            m_registers.PC = (ushort)(B.Value);
                            m_cycles += 3; // 2 for JSR, 1 for RAM lookup. 
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
                    m_cycles++;
                    break;

                // Basic Arithmetic instructions
                case (ushort)dcpuOpCode.ADD_OP:                    
                    tmpVal = A.Value + B.Value;
                    if (tmpVal > 0xFFFF) m_registers.O = 0x0001;
                    A.Value = (ushort)tmpVal;
                    SetResultValue(A);
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.SUB_OP:                    
                    tmpVal = A.Value - B.Value;
                    if (tmpVal < 0) m_registers.O = 0xFFFF;
                    A.Value = (ushort)tmpVal;
                    SetResultValue(A);
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.MUL_OP:
                    A.Value *= B.Value;
                    m_registers.O = (ushort)((A.Value >> 16) & 0xFFFF);
                    SetResultValue(A);
                    m_cycles += 2;
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
                    m_cycles += 3; // seems too cheap
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
                    m_cycles += 3; // seems too cheap
                    break;
                case (ushort)dcpuOpCode.SHL_OP:
                    A.Value <<= B.Value;
                    m_registers.O = (ushort)(((A.Value << B.Value) >> 16) & 0xFFFF);
                    SetResultValue(A);
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.SHR_OP:
                    A.Value >>= B.Value;
                    m_registers.O = (ushort)(((A.Value << 16) >> B.Value) & 0xFFFF);
                    SetResultValue(A);
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.AND_OP:
                    A.Value &= B.Value;
                    SetResultValue(A);
                    m_cycles++;
                    break;
                case (ushort)dcpuOpCode.BOR_OP:
                    A.Value |= B.Value;
                    SetResultValue(A);
                    m_cycles++;
                    break;
                case (ushort)dcpuOpCode.XOR_OP:
                    A.Value ^= B.Value;
                    SetResultValue(A);
                    m_cycles++;
                    break;

                // Branch Instructions
                /*
                 * Basically skip the next instruction on failure
                 */
                case (ushort)dcpuOpCode.IFE_OP:
                    if ((A.Value == B.Value) != true) m_ignoreNextInstruction = true;
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.IFN_OP:
                    if ((A.Value != B.Value) != true) m_ignoreNextInstruction = true;
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.IFG_OP:
                    if ((A.Value > B.Value) != true) m_ignoreNextInstruction = true;
                    m_cycles += 2;
                    break;
                case (ushort)dcpuOpCode.IFB_OP:
                    if (((A.Value & B.Value) != 0) != true) m_ignoreNextInstruction = true;
                    m_cycles += 2;
                    break;

                default:
                    Console.WriteLine(string.Format("Illegal Basic instruction {0} at Address {1}",opCode.ToString("X"),m_registers.PC.ToString("X")));                    
                    break;
            }

        }

        public bool ProgramLoaded
        {
            get { return m_programLoaded; }
            set { m_programLoaded = value; }
        }

        public long CycleCount
        {
            get { return m_cycles; }
        }

        /// <summary>
        /// Write contents of Registers to Console.
        /// </summary>
        public void DebugShowRegisters()
        {
            Console.WriteLine("----------------");
            Console.WriteLine("Register Info");
            Console.WriteLine(string.Format("\tA = {0:X4},\tB = {1:X4},\tC = {2:X4}",
                m_registers.GP[(int)dcpuRegisterCodes.A],
                m_registers.GP[(int)dcpuRegisterCodes.B],
                m_registers.GP[(int)dcpuRegisterCodes.C]));

            Console.WriteLine(string.Format("\tX = {0:X4},\tY = {1:X4},\tZ = {2:X4}",
                m_registers.GP[(int)dcpuRegisterCodes.X],
                m_registers.GP[(int)dcpuRegisterCodes.Y],
                m_registers.GP[(int)dcpuRegisterCodes.Z]));

            Console.WriteLine(string.Format("\tI = {0:X4},\tJ = {1:X4}",
                m_registers.GP[(int)dcpuRegisterCodes.I],
                m_registers.GP[(int)dcpuRegisterCodes.J]));

            Console.WriteLine(string.Format("\tPC = {0:X4},\tSP = {1:X4},\tO = {2:X4}",
                m_registers.PC,
                m_registers.SP,
                m_registers.O));
    

        }
    }
}