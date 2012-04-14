
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DCPU16_ASM.Emulator;

namespace DCPU16_ASM.Winforms
{
    public partial class UserWindowForm : Form
    {
        private Bitmap m_RefFrameBuffer;
        private MainForm m_RefMainform;

        public UserWindowForm()
        {
            InitializeComponent();
        }

        public void FeedReferences(ref Bitmap _frameBuffer, ref MainForm _mainForm)
        {
            m_RefFrameBuffer = _frameBuffer;
            m_RefMainform = _mainForm;
        }


        /// <summary>
        /// On keypress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameWindowForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(m_RefMainform != null)
            {
                m_RefMainform.ProcessKeyPress((ushort)e.KeyChar);
            }
        }

        /// <summary>
        /// Quick hack to get arrow keys as Keypress doesn't capture it :/
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (m_RefMainform != null)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        m_RefMainform.ProcessKeyPress((ushort)0x4);
                        break;
                    case Keys.Up:
                        m_RefMainform.ProcessKeyPress((ushort)0x3);
                        break;
                    case Keys.Left:
                        m_RefMainform.ProcessKeyPress((ushort)0x1);
                        break;
                    case Keys.Enter:
                        m_RefMainform.ProcessKeyPress((ushort)0x000A);
                        break;

                    case Keys.Right:
                        m_RefMainform.ProcessKeyPress((ushort)0x2);
                        break;

                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Tick timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_RefFrameBuffer == null) return;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Picture box Paint
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (m_RefFrameBuffer == null) return;

            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            g.DrawImage(m_RefFrameBuffer, 0, 0, pictureBox1.Width, pictureBox1.Height);
        }
    }
}
