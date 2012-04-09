namespace dcpu16_ASM
{
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
}
