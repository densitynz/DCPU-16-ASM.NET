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
 * WITH THE
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DCPU16_ASM.Emulator;
using DCPU16_ASM.Tools;

namespace DCPU16_ASM.Emulator
{
    /// <summary>
    /// Display system Constants
    /// </summary>
    static class DisplayConstants
    {        
        public static int ScreenTextWidth = 32;
        public static int ScreenTextHeight = 12;

        public static int ScreenPixelWidth = ScreenTextWidth * FontConstants.FontWidth;
        public static int ScreenPixelHeight = ScreenTextHeight * FontConstants.FontHeight;

        public static int[] BaseColor = new int[256];
        public static int[] OffsetColor = new int[256];

        /// <summary>
        /// Generate color
        /// Based on Notch's specs
        /// </summary>
        /// <param name="_i"></param>
        /// <returns></returns>
        public static int GenerateColor(int _i)
        {
            int blue = (_i >> 0 & 1) * 0xAA;
            int green = (_i >> 1 & 1) * 0xAA;
            int red = (_i >> 2 & 1) * 0xAA;

            if (_i >= 8)
            {
                blue += 0x55;
                green += 0x55;
                red += 0x55;
            }
            else if (_i == 6)
            {
                blue -= 0x55;
            }

            return (0xFF << 24) + (red << 16) + (green << 8) + blue;
        }

        /// <summary>
        /// Generate color maps
        /// Based on notch's specs
        /// </summary>
        public static void GenerateColorMaps()
        {
            for(int i = 0; i < 256;i++)
            {
                int background = DisplayConstants.GenerateColor(i % 16);
                int foreground = DisplayConstants.GenerateColor(i / 16);
                BaseColor[i] = background;
                OffsetColor[i] = foreground - background;
            }
        }

    }
    
    // TODO: Display code refactor into here!
}
