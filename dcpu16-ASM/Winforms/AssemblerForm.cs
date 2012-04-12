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

namespace DCPU16_ASM.Winforms
{
    using DCPU16_ASM.Assembler;

    using System;
    using System.Windows.Forms;
    using System.IO;

    public partial class AssemblerForm : Form
    {
        private string m_fileName = "";
        private bool m_modified = false;

        public AssemblerForm()
        {
            InitializeComponent();
        }

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

        private void SaveSourceCode()
        {
            saveFileDialog1.FileName = m_fileName;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, codeTextBox.Text);
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_fileName = openFileDialog1.FileName;

                codeTextBox.Text = File.ReadAllText(m_fileName);

                this.Text = string.Format("Assembler - {0}", m_fileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (codeTextBox.Text.Trim() == "")
            {                
                MessageBox.Show("Nothing to compile", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var assemble = new Parser();
            var lines = codeTextBox.Text.Split('\n');
            ushort[] machineCode = assemble.Parse(ref lines);
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

        private void codeTextBox_TextChanged(object sender, EventArgs e)
        {
            m_modified = true;
            if(m_fileName.Trim() != "")
                this.Text = string.Format("Assembler - *MODIFIED* {0}", m_fileName);
            else
                this.Text = string.Format("Assembler - *MODIFIED* Untitled");
        }

        // save source code. 
        private void button4_Click(object sender, EventArgs e)
        {
            SaveSourceCode();
        }

        private void AssemblerForm_Load(object sender, EventArgs e)
        {            
            this.Text = string.Format("Assembler - Untitled");
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            MessageBox.Show("Message box cleared", Globals.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
