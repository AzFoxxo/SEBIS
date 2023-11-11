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
        public static Dictionary<Opcodes, byte[]> opcodesBinary = new Dictionary<Opcodes, byte[]>()
        {
            // Short opcodes
            { Opcodes.NOP, new byte [] {0, 0, 0} },
            { Opcodes.LDA, new byte [] {1, 0, 0} },
            { Opcodes.LDB, new byte [] {2, 0, 0} },
            { Opcodes.STA, new byte [] {3, 0, 0} },
            { Opcodes.STB, new byte [] {4, 0, 0} },
            { Opcodes.STC, new byte [] {5, 0, 0} },
            { Opcodes.SYSCALL, new byte [] {6, 0, 0} },
            { Opcodes.ABC, new byte [] {7, 0, 0} },
            // Long opcodes
            { Opcodes.MUL, new byte [] {0, 0, 0} },
            { Opcodes.JMP, new byte [] {0, 1, 0} },
            { Opcodes.JZ, new byte [] {0, 2, 0} },
            { Opcodes.JNZ, new byte [] {0, 3, 0} },
            { Opcodes.CMP, new byte [] {0, 4, 0} },
            { Opcodes.CPL, new byte [] {0, 5, 0} },
            { Opcodes.AND, new byte [] {0, 6, 0} },
            { Opcodes.XOR, new byte [] {0, 7, 0} },
            { Opcodes.OR, new byte [] {0, 8, 0} },
            { Opcodes.HLT, new byte [] {0, 9, 0} },
            { Opcodes.PANIC, new byte [] {0, 10, 0} },
            { Opcodes.MBNKROM, new byte [] {0, 11, 0} },
            { Opcodes.MBNKRAM, new byte [] {0, 12, 0} },
            { Opcodes.ZERO, new byte [] {0, 13, 0} },
            { Opcodes.LDL, new byte [] {0, 14, 0} },
            { Opcodes.LDH, new byte [] {0, 15, 0} },
            { Opcodes.CLD, new byte [] {0, 16, 0} }
        };

        /// <summary>
        /// Return the binary representation of an opcode in byte array form.
        /// </summary>
        /// <param name="opcode">Opcode</param>
        /// <returns>Three byte byte array</returns>
        public static byte[] GetOpcodeBinary(Opcodes opcode) => opcodesBinary[opcode];
    }
}