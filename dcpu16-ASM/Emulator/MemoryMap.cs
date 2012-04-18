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

// Memory map/layout definitions.

using System;
using System.Collections.Generic;
using System.IO;

namespace DCPU16_ASM.Emulator
{
    /// <summary>
    /// Enumeration defines of Specific areas of the DCPU's memory layout. 
    /// 
    /// </summary>
    public enum dcpuMemoryLayout : ushort
    {
        /// <summary>
        /// Starting memory location for DCPU-16's Text mode 
        /// Frame buffer
        /// </summary>
        VIDEO_TEXT_START    = 0x8000,
        /// <summary>
        /// Ending memory location for DCPU-16's Text mode 
        /// Frame buffer
        /// </summary>
        VIDEO_TEXT_END      = 0x817F, // Resolution is 32x12 chars

        /// <summary>
        /// Character set start
        /// </summary>
        VIDEO_CHARSET_START = 0x8180,
        /// <summary>
        /// Character set end position
        /// </summary>
        VIDEO_CHARSET_END   = 0x827F,
        /// <summary>
        /// Bottom lines 12-16
        /// </summary>
        VIDEO_MISC_DATA     = 0x8280,

        /// <summary>
        /// Keyboard Starting memory address
        /// </summary>
        KEYBOARD_START      = 0x9000,
        /// <summary>
        /// Keyboard ending addres
        /// </summary>
        KEYBOARD_END        = 0x900F,
        /// <summary>
        /// Keyboard ring buffer index 
        /// </summary>
        KEYBOARD_INDEX      = 0x9010 
    }
}