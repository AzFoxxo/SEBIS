/*
 Copyright (c) 2023 Az Foxxo

 Permission is hereby granted, free of charge, to any person obtaining a copy of
 this software and associated documentation files (the "Software"), to deal in
 the Software without restriction, including without limitation the rights to
 use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 the Software, and to permit persons to whom the Software is furnished to do so,
 subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace SEBIS.Shared
{
    /// <summary>
    /// VariableLengthOpcode is a struct that holds the decimal representation of the opcode,
    /// and the number of bytes needed for the instruction.
    /// It's value should not directly be written but the appropriate bits of the value written
    /// with the given length property e.g.
    /// 1 - 3 bits written (short instruction)
    /// 2 - 11 bits written (long instruction)
    /// 3 - 19 bits written (extra long instruction)
    /// </summary>
    public struct VariableLengthOpcode {
        public uint opcodeValueAsDecimal {get; private set; } // The opcode value as a decimal (some opcodes in decimal may have colliding values)
        public OpcodeByteLength length {get; private set; } // How many extra bytes are needed for the instruction after the 3 bits

        public VariableLengthOpcode(uint opcodeValueAsDecimal, OpcodeByteLength length) {
            this.opcodeValueAsDecimal = opcodeValueAsDecimal;
            this.length = length;
        }
    }
}