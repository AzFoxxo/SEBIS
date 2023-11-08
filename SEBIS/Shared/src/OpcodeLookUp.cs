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
    public class OpcodeLookUp
    {
        private Dictionary<Opcodes, uint> opcodes = new() {
            // Short opcodes
            { Opcodes.NOP, 0 },
            { Opcodes.LDA, 1 },
            { Opcodes.LDB, 2 },
            { Opcodes.STA, 3 },
            { Opcodes.STB, 4 },
            { Opcodes.STC, 5 },
            { Opcodes.SYSCALL, 6 },
            { Opcodes.ABC, 7 },
            // Long opcodes
            { Opcodes.MUL, 0},
            { Opcodes.JMP, 1},
            { Opcodes.JZ, 2},
            { Opcodes.JNZ, 3},
            { Opcodes.CMP, 4},
            { Opcodes.CPL, 5},
            { Opcodes.AND, 6},
            { Opcodes.XOR, 7},
            { Opcodes.OR, 8},
            { Opcodes.HLT, 9},
            { Opcodes.PANIC, 10},
            { Opcodes.MBNKROM, 11},
            { Opcodes.MBNKRAM, 12},
            { Opcodes.ZERO, 13},
            { Opcodes.LDL, 14},
            { Opcodes.LDH, 15},
            { Opcodes.CLD, 16},
        };

        /// <summary>
        /// Return the decimal value corresponding to an opcode.
        /// Expect collisions and use VariableLengthOpcode and Length
        /// to write how many bits need to be written.
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns>uint representation of the opcode</returns>
        public uint this[Opcodes opcode] => opcodes[opcode];
    }
}