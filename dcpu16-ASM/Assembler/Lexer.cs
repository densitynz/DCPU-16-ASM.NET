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

/* 
 * !!!!!!!!!!!!!!!!!WORK IN PROGRESSSSSSSSS!!!!!!!!!!!!! 
 * This code won't be used in the assembler until version 0.8 alpha! 
 * Testing has largely been manually looking and generated token lists. 
 * 
 * complete rewrite of DCPU-16 ASM.NET's assembler. We can consider this version 2. 
 * 
 * multi Stage Compile/Assemble process which is a little more advance than my old one.
 * 
 * Stages for new system.
 * 1. base Tokenlization pass. Assumes all Label references are valid, throws errors on unknown
 *    symbols
 * 2. label verification pass. Ensures LABELREFs are pointing to actual LABELs
 * 3. Parse token stage. which'll simply be a linear token buffer walk with a
 *    state machine.
 * 4. Label Patch up LABELREF's addressing. 
 * 5. Save binary to disk. 
 * 
 * NOTE: got some performance issues due to my poor Regular expressions :/
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DCPU16_ASM.Assembler
{
    /// <summary>
    /// Lexer Token type
    /// </summary>
    public enum dcpuLexerTokenType
    {
        /// <summary>
        /// DCPU instruction
        /// add, set, jsr etc etc
        /// </summary>
        INSTRUCTION,
        /// <summary>
        /// Register
        /// a,b,c,x,y,z,i,j,pop,push,peek
        /// </summary>
        REGISTER,
        /// <summary>
        /// Integer value 
        /// </summary>
        INT,
        /// <summary>
        /// Hexdecminal value
        /// </summary>
        HEX,
        /// <summary>
        /// String in quotation marks
        /// </summary>
        STRING,
        /// <summary>
        /// Common seperator
        /// </summary>
        COMMA,
        /// <summary>
        /// Open Bracket -> [ or (
        /// </summary>
        OPENBRACKET,
        /// <summary>
        /// Close bracket -> ] or )
        /// </summary>
        CLOSEBRACKET,
        /// <summary>
        /// Raw Label
        /// aka 
        /// :label
        /// </summary>
        LABEL,
        /// <summary>
        /// Label reference (these aren't verified until
        /// the second pass)
        /// </summary>
        LABELREF,
        /// <summary>
        /// Plus operator
        /// </summary>
        PLUS,
        /// <summary>
        /// White space
        /// </summary>
        WHITESPACE,
        /// <summary>
        /// Comment
        /// </summary>
        COMMENT,
        /// <summary>
        /// End of file
        /// </summary>                
        EOF
    }

    //source[currentIndex], currentIndex, currentLine, currentColumn

    /// <summary>
    /// Error Message object
    /// </summary>
    class CDCPULexerError
    {
    }

    /// <summary>
    /// Lexer Token
    /// </summary>
    public struct CDCPULexerToken
    {
        /// <summary>
        /// Token's index within main file string
        /// </summary>
        private int m_index; // index within file string
        /// <summary>
        /// 
        /// </summary>
        private int m_line;  // parsed Line token is on
        private int m_column; // column within line where token is located
        private dcpuLexerTokenType m_type; // token type
        private string m_value; // value

        public CDCPULexerToken(int _index, int _line, int _column, dcpuLexerTokenType _type, string _value)
        {
            m_index = _index;
            m_line = _line;
            m_column = _column;
            m_type = _type;
            m_value = _value;
        }

        public int Index { get { return m_index; } set { m_index = value; } }
        public int Line { get { return m_line; } set { m_line = value; } }
        public int Column { get { return m_column; } set { m_column = value; } }
        public dcpuLexerTokenType Type { get { return m_type; } set { m_type = value; } }
        public string Value { get { return m_value; } set { m_value = value; } }

        public override string ToString()
        {
            return string.Format("Type: {0} Value: {1} Line: {2} Column: {3}",
                    m_type.ToString(), m_value, m_line, m_column);
        }
    }

    /// <summary>
    /// Lexer token definition
    /// </summary>
    class CDCPULexerTokenDefinition
    {
        private dcpuLexerTokenType m_type;
        private Regex m_matchString;
        private bool m_ignoreDefinition = false;

        public CDCPULexerTokenDefinition(dcpuLexerTokenType _type, Regex _matchString, bool _ignore)
        {
            m_type = _type;
            m_matchString = _matchString;
            m_ignoreDefinition = _ignore;
        }

        public dcpuLexerTokenType Type { get { return m_type; } set { m_type = value; } }
        public Regex MatchString { get { return m_matchString; } set { m_matchString = value; } }
        public bool Ignore { get { return m_ignoreDefinition; } set { m_ignoreDefinition = value; } }

    }

    /// <summary>
    /// DCPU-16 Assembly Lexer
    /// </summary>
    public class CDCPULexer
    {
        /// <summary>
        /// DCPU-16 Assembly Token definition list. 
        /// </summary>
        private readonly List<CDCPULexerTokenDefinition> m_tokenDef = new List<CDCPULexerTokenDefinition>()
        {
            // Whitespace (Ignored)
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.WHITESPACE, new Regex(@"(\r\n|\s+)"),true),             //\r\n|
            // Comment (Ignored)
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.COMMENT, new Regex(@";[^\n]*\r\n?"),true),

            // Label ':\w+
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.LABEL, new Regex(@":\w+"),false),
 
            // Hex number
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.HEX, new Regex(@"(0x[0-9a-fA-F]+)"),false),
            // integer

            new CDCPULexerTokenDefinition(dcpuLexerTokenType.INT, new Regex(@"[0-9]+"),false),
            // Plus 
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.PLUS, new Regex(@"\+"),false),
            // comma
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.COMMA, new Regex(@","),false),

            // Open/Close Breakets [ ]
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.OPENBRACKET, new Regex(@"[\[\(]"), false),
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.CLOSEBRACKET,new Regex(@"[\]\)]"), false),

            // Below 2 are insanely slow
            // Instructions
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.INSTRUCTION, 
                new Regex(@"\b(\bdat\b|\bset\b|\badd\b|\bsub\b|\bmul\b|\bdiv\b|\bmod\b|\bshl\b|\bshr\b|\band\b|\bbor\b|\bxor\b|\bife\b|\bifn\b|\bifg\b|\bifb\b|\bjsr\b|\b\bSET\b|\bADD\b|\bSUB\b|\bMUL\b|\bDIV\b|\bMOD\b|\bSHL\b|\bSHR\b|\bAND\b|\bBOR\b|\bXOR\b|\bIFE\b|\bIFN\b|\bIFG\b|\bIFB\b|\bJSR\b|\bDAT\b)"),
                false),

            // Registers
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.REGISTER,
               new Regex(@"(\ba\b|\bb\b|\bc\b|\bx\b|\by\b|\bz\b|\bi\b|\bj\b|\bpop\b|\bpush\b|\bpeek\b|\bpc\b|\bsp|\bo\b|\bA\b|\bB\b|\bC\b|\bX\b|\bY\b|\bZ\b|\bI\b|\bJ\b|\bPOP\b|\bPUSH\b|\bPEEK\b|\bPC\b|\bSP\b|\bO\b)"), 
                false),


            // String fields            
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.STRING, new Regex(@"@?\""(\""\""|[^\""])*\"""), false),


            // Label ref, has to be last
            new CDCPULexerTokenDefinition(dcpuLexerTokenType.LABELREF, new Regex(@"[a-zA-Z0-9_]+"),false)
        };

        private readonly List<string> m_foundLabels = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public CDCPULexer()
        {
        }

        /// <summary>
        /// Validate LABELREF in tokenlized tree. 
        /// </summary>
        /// <param name="_tokenList"></param>
        private bool ValidateTokenizedLabels(ref List<CDCPULexerToken> _tokenList, out string _errorString)
        {
            _errorString = "";            

            foreach (CDCPULexerToken token in _tokenList)
            {                
                if (token.Type != dcpuLexerTokenType.LABELREF) continue;

                bool fEntry = false;
                foreach (string label in m_foundLabels)
                {
                    if (string.Compare(label, token.Value, true) == 0)
                    {
                        fEntry = true;
                        break;
                    }
                }
                if (fEntry != true)
                {                    
                    _errorString = string.Format("Undeclared label '{0}' referenced (line {1}, column {2})", token.Value, token.Line, token.Column);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tokenize DCPU Assembly file.
        /// 
        /// TODO: profile!
        /// </summary>
        /// <param name="_fileString"></param>
        /// <returns></returns>
        public List<CDCPULexerToken> TokenizeFileString(string _fileString, out string _errorString)
        {
            _errorString = "";
            m_foundLabels.Clear();

            List<CDCPULexerToken> tokenList = new List<CDCPULexerToken>();
            tokenList.Capacity = 200000;
            Regex newLineCheck = new Regex(@"\r\n|\r|\n");
            // counters
            int cIndex = 0;
            int cLine = 1;
            int cColumn = 0;
            
            while (cIndex < _fileString.Length)
            {
                int matchLength = 0;
                CDCPULexerTokenDefinition foundDefinition = null;
                
                // loop through all token definitions
                foreach (CDCPULexerTokenDefinition def in m_tokenDef)
                {
                    Match regMatch = def.MatchString.Match(_fileString, cIndex);
                    if (regMatch.Success != false &&
                        (regMatch.Index - cIndex) == 0)
                    {
                        foundDefinition = def;
                        matchLength = regMatch.Length;
                        break;
                    }
                }

                if (foundDefinition != null)
                {
                    // found!, pull out value
                    string val = _fileString.Substring(cIndex, matchLength);

                    // if value is not ignorable, add to our token list
                    if (foundDefinition.Ignore != true)
                    {                        
                        tokenList.Add(new CDCPULexerToken(cIndex, cLine, cColumn, foundDefinition.Type, val));
                        if (foundDefinition.Type == dcpuLexerTokenType.LABEL)
                        {
                            m_foundLabels.Add(val.Replace(":","").Trim());
                        }
                    }

                    // Perform end of line check. 
                    Match eolMatch = newLineCheck.Match(val);
                    if (eolMatch.Success != false)
                    {
                        cColumn = val.Length - (eolMatch.Index + eolMatch.Length);
                        cLine++;
                    }
                    else
                    {
                        cColumn += matchLength;
                    }
                    cIndex += matchLength;
                }
                else
                {
                    // We don't know the symbol, report it.
                    // Log Error! 
                    _errorString = string.Format("Unrecognized symbol '{0}' (line {1}, column {2}).", _fileString[cIndex], cLine, cColumn);

                    return null;
                }  

            }

            // end of file. 
            tokenList.Add(new CDCPULexerToken(cIndex, cLine, cColumn, dcpuLexerTokenType.EOF, ""));

            // validate label tokens
            ValidateTokenizedLabels(ref tokenList, out _errorString);

            return tokenList;
        }
    }    
}
