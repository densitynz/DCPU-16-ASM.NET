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

// Quick code editor. 
// Please note that the focus of this project is not really editing code.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using DCPU16_ASM.Assembler;

using FastColoredTextBoxNS;

namespace DCPU16_ASM.Winforms
{
    /// <summary>
    /// Main Code Assembler Window
    /// </summary>
    public partial class AssemblerForm : Form
    {
        /// <summary>
        /// opened filename
        /// </summary>
        private string m_fileName = "";

        /// <summary>
        /// Has any code been modified
        /// </summary>
        private bool m_modified = false;

        private TextStyle BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        
        private TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        private TextStyle GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        private TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
        private TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        private TextStyle RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close button on click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (m_modified != false)
            {
                // Ask the question
                DialogResult result = MessageBox.Show("Do you wish to save changes?", Globals.ProgramName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Cancel) return;
                else if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveSourceCode();
                }
            }
            this.Close();
        }

        /// <summary>
        /// Source code Save
        /// </summary>
        private void SaveSourceCode()
        {
            saveFileDialog1.FileName = m_fileName;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, ColorCodeTextBox.Text);
                    m_fileName = saveFileDialog1.FileName;
                    m_modified = false;
                    this.Text = string.Format("Assembler - {0}", m_fileName);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message, Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);                        
                }
            }
        }

        /// <summary>
        /// Open Source code file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_fileName = openFileDialog1.FileName;

                ColorCodeTextBox.Text = File.ReadAllText(m_fileName);

                this.Text = string.Format("DCPU-16 ASM.NET Code Editor - {0}", m_fileName);
                ColorCodeTextBox.Selection.Start = Place.Empty;
                ColorCodeTextBox.DoCaretVisible();
                ColorCodeTextBox.IsChanged = false;
                ColorCodeTextBox.ClearUndo();
            }
        }

        /// <summary>
        /// Compile Source code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (ColorCodeTextBox.Text.Trim() == "")
            {                
                MessageBox.Show("Nothing to compile", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var assemble = new Parser();
            var lines = ColorCodeTextBox.Text.Split('\n');
            ushort[] machineCode = assemble.Parse(lines);
            if (machineCode == null)
            {
                textBox1.Text = assemble.MessageOuput;
                MessageBox.Show("Issue compiling code, check the 'Messages' box", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var generator = new Generator();
            var output = generator.Generate(machineCode, this.m_fileName);
            if (output == string.Empty)
            {
                textBox1.Text = generator.MessageOuput;
                MessageBox.Show("Issue compiling code, check the 'Messages' box", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            textBox1.Text = assemble.MessageOuput;
            MessageBox.Show(string.Format("Code successfully compiled to\n\n'{0}'", output), Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.None);
        }


        /// <summary>
        /// Code text box on change event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void codeTextBox_TextChanged(object sender, EventArgs e)
        {
            m_modified = true;
            if(m_fileName.Trim() != "")
                this.Text = string.Format("DCPU-16 ASM.NET Code Editor - *MODIFIED* {0}", m_fileName);
            else
                this.Text = string.Format("DCPU-16 ASM.NET Code Editor - *MODIFIED* Untitled");
        }

        /// <summary>
        /// Save source code Button on click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            SaveSourceCode();
        }

        /// <summary>
        /// On form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssemblerForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("DCPU-16 ASM.NET Code Editor - Untitled");

        }

        /// <summary>
        /// Message textbox right click clear.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            MessageBox.Show("Message box cleared", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ColorCodeTextBox_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        ///  On text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorCodeTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {            
            e.ChangedRange.ClearStyle(BlueStyle, MagentaStyle, GreenStyle, BrownStyle,RedStyle);
            
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'"); // strings            
            e.ChangedRange.SetStyle(GreenStyle, @";.*$", RegexOptions.Multiline); // comments            
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b"); // numbers

            // dcpu instructions (yes I suck at regex)
            e.ChangedRange.SetStyle(BlueStyle, @"\b(\bset\b|\badd\b|\bsub\b|\bmul\b|\bdiv\b|\bmod\b|\bshl\b|\bshr\b|\band\b|\bbor\b|\bxor\b|\bife\b|\bifn\b|\bifg\b|\bifb\b|\bjsr\b|\bdat\b|\bSET\b|\bADD\b|\bSUB\b|\bMUL\b|\bDIV\b|\bMOD\b|\bSHL\b|\bSHR\b|\bAND\b|\bBOR\b|\bXOR\b|\bIFE\b|\bIFN\b|\bIFG\b|\bIFB\b|\bJSR\b|\bDAT\b)");

            // dcpu registers
            e.ChangedRange.SetStyle(RedStyle, @"\b(\ba\b|\bb\b|\bc\b|\bx\b|\by\b|\bz\b|\bi\b|\bj\b|\bpop\b|\bpeek\b|\bpush\b|\bpc\b|\bsp\b|\bo\b|\bA\b|\bB\b|\bC\b|\bX|\bY\b|\bZ\b|\bI\b|\bJ\b|\bPOP\b|\bPEEK\b|\bPUSH\b|\bPC\b|\bSP\b|\bO\b)");
        }
    }
}
