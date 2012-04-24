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

// Quickly written Font editor! 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DCPU16_ASM.Emulator;
using DCPU16_ASM.Tools;

namespace DCPU16_ASM.Winforms
{    


    /// <summary>
    /// Form Class
    /// </summary>
    public partial class FontEditForm : Form
    {
        private int m_MouseX = 0;
        private int m_MouseY = 0;        

        private int m_xGridStep = 1;
        private int m_yGridStep = 1;

        private cpuDoubleBuffer m_dcpuRef = null;

        private CFontCharSet m_FontSet = new CFontCharSet();
        private int m_FontCharIndex = 0; // max 128!
        private int m_HoverCharIndex = 0;

        private Bitmap m_FontCharsetBitmap = null;

        public FontEditForm()
        {
            InitializeComponent();

            m_FontCharsetBitmap = new Bitmap(128, 32);
        }

        /// <summary>
        /// dCPU double buffer.
        /// </summary>
        /// <param name="_dcpu"></param>
        public void SetDcpuRef(ref cpuDoubleBuffer _dcpu)
        {
            if (_dcpu == null) return;

            m_dcpuRef = _dcpu;
        }

        /// <summary>
        /// Close Dialog. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Main Timer think (30hz)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Refresh(); // Editing Font
            pictureBox2.Refresh(); // Charset list

            label1.Text = string.Format("Font Field:      0x{0:X4} 0x{1:X4}",
                m_FontSet.FontCharacters[(m_FontCharIndex * 2)],
                m_FontSet.FontCharacters[(m_FontCharIndex * 2) + 1]);           
        }

        /// <summary>
        /// Load Font
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_FontSet.Load(openFontDialog.FileName);                    
            }
        }

        /// <summary>
        /// Save Font dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_FontSet.Save(saveFontDialog.FileName);
            }
        }

        /// <summary>
        /// Picture box on paint event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.LightSteelBlue);

            DrawFont(ref g);

            DrawGrid(ref g);            
        }

        /// <summary>
        /// Draw Grid including
        /// </summary>
        /// <param name="g"></param>
        private void DrawGrid(ref Graphics g)
        {
            if (g == null) return;

            for (int x = 0; x <= FontConstants.FontWidth; x++)
            {
                g.DrawLine(new Pen(Brushes.Black), x * m_xGridStep, 0, x * m_xGridStep, pictureBox1.Height);
            }
            for (int y = 0; y <= FontConstants.FontHeight; y++)
            {
                g.DrawLine(new Pen(Brushes.Black),0,y*m_yGridStep,pictureBox1.Width,y*m_yGridStep);
            }

            int selectTileOriginX = (m_MouseX - (m_MouseX % m_xGridStep)) / m_xGridStep;
            int selectTileOriginY = (m_MouseY - (m_MouseY % m_yGridStep)) / m_yGridStep;
            selectTileOriginX = selectTileOriginX >= FontConstants.FontWidth ? FontConstants.FontWidth-1 : selectTileOriginX;
            selectTileOriginY = selectTileOriginY >= FontConstants.FontHeight ? FontConstants.FontHeight - 1 : selectTileOriginY;

            g.DrawRectangle(new Pen(Brushes.LightGreen), selectTileOriginX * m_xGridStep, selectTileOriginY * m_yGridStep, m_xGridStep, m_yGridStep);
        }

        /// <summary>
        /// World's Slowest font pixel draw  :)
        /// (no point doing bitmap locks here)
        /// </summary>
        /// <param name="g"></param>
        private void DrawFont(ref Graphics g)
        {
            if (g == null) return;

            for (int x = 0; x < FontConstants.FontWidth; x++)
            {
                for (int y = 0; y < FontConstants.FontHeight; y++)
                {
                    int checkFlag = ((1 << y) << (x + 1 & 1) * FontConstants.FontHeight);

                    if((m_FontSet.FontCharacters[(m_FontCharIndex * 2) + x / 2] & checkFlag) != 0)
                    {                        
                        g.FillRectangle(Brushes.Gray, x * m_xGridStep, y * m_yGridStep, m_xGridStep, m_yGridStep);
                    }
                }
            }

        }

        /// <summary>
        /// Update ascii Code text
        /// </summary>
        private void UpdateSelectedAsciiCodeText()
        {
            AsciiCodeTextBox.Text = string.Format("{0}", (char)m_FontCharIndex);
            HexCodeTextBox.Text = string.Format("0x{0:X4}", m_FontCharIndex);
        }

        /// <summary>
        /// Hover ascii code
        /// </summary>
        private void HoverAsciiCodeText()
        {
            AsciiCharHoverTextBox.Text = string.Format("{0}", (char)m_HoverCharIndex);
            AsciiCodeHoverTextBox.Text = string.Format("0x{0:X4}", m_HoverCharIndex);
        }

        /// <summary>
        /// Picturebox Mouse Move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {            
            m_MouseX = e.X;
            m_MouseY = e.Y;
        }

        /// <summary>
        /// Picture box on click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int selectTileOriginX = (m_MouseX - (m_MouseX % m_xGridStep)) / m_xGridStep;
            int selectTileOriginY = (m_MouseY - (m_MouseY % m_yGridStep)) / m_yGridStep;
            selectTileOriginX = selectTileOriginX >= FontConstants.FontWidth ? FontConstants.FontWidth - 1 : selectTileOriginX;
            selectTileOriginY = selectTileOriginY >= FontConstants.FontHeight ? FontConstants.FontHeight - 1 : selectTileOriginY;

            ushort bitSet = (ushort)((1 << selectTileOriginY) << (selectTileOriginX + 1 & 1) * FontConstants.FontHeight);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                m_FontSet.FontCharacters[(m_FontCharIndex * 2) + selectTileOriginX / 2] |= bitSet; // Set pixel
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {              
                m_FontSet.FontCharacters[(m_FontCharIndex * 2) + selectTileOriginX / 2] &= (ushort)(~(int)bitSet); // Clear pixel
            }
        }

        /// <summary>
        /// On form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontEditForm_Load(object sender, EventArgs e)
        {
            m_xGridStep = (int)Math.Floor((double)((pictureBox1.Width - 1) / FontConstants.FontWidth));
            m_yGridStep = (int)Math.Floor((double)((pictureBox1.Height - 1) / FontConstants.FontHeight));

            m_FontCharIndex = 0;
            UpdateSelectedAsciiCodeText();
        }

        /// <summary>
        /// Clear current Character that is being edited. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            m_FontSet.FontCharacters[(m_FontCharIndex * 2) + 0] = 0;
            m_FontSet.FontCharacters[(m_FontCharIndex * 2) + 1] = 0;
        }

        /// <summary>
        /// Import from running DCPU program!!!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importFromRunningDCPU16ProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dcpuRef == null)
            {
                MessageBox.Show("No reference to Running DCPU-16 Found!", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("WARNING!: This action will wipe any unsaved modified!\r\n\r\nDo you wish to continue?", Globals.ProgramName, 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }
            m_FontSet.ImportFromDCPU(ref m_dcpuRef);
        }

        /// <summary>
        /// On numerical value change. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {         
        }

        /// <summary>
        /// Charset on pain event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                              
            g.Clear(Color.Black);            
            RenderCharSetToBitmap();          

            g.DrawImage(m_FontCharsetBitmap, 0, 0, pictureBox2.Width,pictureBox2.Height);
        }


        /// <summary>
        /// Render character set to a bitmap
        /// </summary>
        private void RenderCharSetToBitmap()
        {
            if (m_FontCharsetBitmap == null) return;

            BitmapData Data = m_FontCharsetBitmap.LockBits(new Rectangle(0, 0, m_FontCharsetBitmap.Width, m_FontCharsetBitmap.Height),
                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                IntPtr charsetScan = Data.Scan0;
                
                for (int i = 0; i < FontConstants.MaxFontEntries; i++) // we have 128 entries in our character map, 2 Words big.
                {
                    int screenOffsetX = (i % 32) * (FontConstants.FontWidth);
                    int screenOffsetY = (i / 32) * (FontConstants.FontHeight);

                    for (int x = 0; x < FontConstants.FontWidth; x++)
                    {                        
                        int fontBits = (m_FontSet.FontCharacters[i * 2 + (x >> 1)] >> (x + 1 & 1) * FontConstants.FontHeight);

                        for (int y = 0; y < FontConstants.FontHeight; y++)
                        {
                            int bitSet = (fontBits >> y) & 1;
                            int offset = ((screenOffsetY + y) * (m_FontCharsetBitmap.Width) + (screenOffsetX + x)) * 4;
                            if (i == m_FontCharIndex)
                            {
                                if (bitSet != 0)
                                {
                                    *((byte*)(void*)charsetScan + offset + 0) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 1) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 2) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0xFF;
                                }
                                else
                                {
                                    *((byte*)(void*)charsetScan + offset + 0) = (byte)0x7F;
                                    *((byte*)(void*)charsetScan + offset + 1) = (byte)0x1F;
                                    *((byte*)(void*)charsetScan + offset + 2) = (byte)0x3F;
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0xFF;
                                }
                            }
                            else if (i == m_HoverCharIndex)
                            {
                                if (bitSet != 0)
                                {
                                    *((byte*)(void*)charsetScan + offset + 0) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 1) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 2) = (byte)0xFF;
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0xFF;
                                }
                                else
                                {
                                    *((byte*)(void*)charsetScan + offset + 0) = (byte)0x00;
                                    *((byte*)(void*)charsetScan + offset + 1) = (byte)0x7F;
                                    *((byte*)(void*)charsetScan + offset + 2) = (byte)0x3F;
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0xFF;
                                }
                            }
                            else
                            {
                                if (bitSet != 0)
                                {
                                    *((byte*)(void*)charsetScan + offset + 0) = (byte)0xBF;
                                    *((byte*)(void*)charsetScan + offset + 1) = (byte)0xBF;
                                    *((byte*)(void*)charsetScan + offset + 2) = (byte)0xBF;
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0xFF;
                                }
                                else
                                    *((byte*)(void*)charsetScan + offset + 3) = (byte)0x00;
                            }
                        }
                    }

                }
            }
            m_FontCharsetBitmap.UnlockBits(Data);
        }

        /// <summary>
        /// Char set selector Mouse click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            float xGridStep = (float)((float)FontConstants.FontWidth * ((float)pictureBox2.Width / (float)m_FontCharsetBitmap.Width));
            float yGridStep = (float)((float)FontConstants.FontHeight * ((float)pictureBox2.Height / (float)m_FontCharsetBitmap.Height));
            int selectTileOriginX = (int)(((float)e.X - ((float)e.X % xGridStep)) / xGridStep);
            int selectTileOriginY = (int)(((float)e.Y - ((float)e.Y % yGridStep)) / yGridStep);

            m_FontCharIndex = selectTileOriginY * 32 + selectTileOriginX;

            UpdateSelectedAsciiCodeText();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            float xGridStep = (float)((float)FontConstants.FontWidth * ((float)pictureBox2.Width / (float)m_FontCharsetBitmap.Width));
            float yGridStep = (float)((float)FontConstants.FontHeight * ((float)pictureBox2.Height / (float)m_FontCharsetBitmap.Height));
            int selectTileOriginX = (int)(((float)e.X - ((float)e.X % xGridStep)) / xGridStep);
            int selectTileOriginY = (int)(((float)e.Y - ((float)e.Y % yGridStep)) / yGridStep);

            m_HoverCharIndex = selectTileOriginY * 32 + selectTileOriginX;

            HoverAsciiCodeText();
        }

        /// <summary>
        /// New Font
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to make a new Character set?", Globals.ProgramName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            m_FontSet.Clear();
        }

        /// <summary>
        /// Export to assembly file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToDCPU16AssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveAsmDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (m_FontSet.DumpToAssembly(saveAsmDialog.FileName) != false)
            {
                MessageBox.Show(string.Format("Font dumped to assembly file\n\n'{0}'", saveAsmDialog.FileName),Globals.ProgramName,MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Import Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importImage128x32ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            string error = string.Empty;

            if (m_FontSet.ImportFromImage(openImageDialog.FileName, out error) != true)
            {
                MessageBox.Show(error, Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Successfully imported", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
