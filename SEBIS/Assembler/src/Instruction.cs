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

using SEBIS.Shared;

namespace SEBIS.Assembler
{
    struct Instruction
    {
        public OpcodeByteLength opcodeLength { get; private set; }  // Opcode size
        public bool flagBitValue { get; private set; }              // Flag value
        public AddressMode addressMode { get; private set; }        // Addressing mode
        public Opcodes opcode { get; private set; }                 // Opcode (enum)
        public bool IncludeLongRegisterInROM { get; private set; }
        public string longRegisterValue { get; private set; }

        public Instruction(Opcodes opcode, bool flagBitValue, bool IncludeLongRegisterInROM, string longRegisterValue, OpcodeByteLength opcodeLength, AddressMode addressMode)
        {
            this.opcode = opcode;
            this.flagBitValue = flagBitValue;
            this.IncludeLongRegisterInROM = IncludeLongRegisterInROM;
            this.longRegisterValue = longRegisterValue;
            this.opcodeLength = opcodeLength;
            this.addressMode = addressMode;
        }

        // Implicitly convert Instruction to List<Instruction> when needed
        public static implicit operator List<Instruction>(Instruction instruction)
        {
            return new List<Instruction> { instruction };
        }

        // Implicit to string conversion
        public static implicit operator string(Instruction instruction)
        {
            var variableOpcode = new VariableLengthOpcode((uint)instruction.opcode, instruction.opcodeLength);
            
            var op = OpcodesUtils.ToBinaryString(variableOpcode);
            return $"{op} {instruction.opcode.ToString()} {instruction.flagBitValue} {instruction.IncludeLongRegisterInROM} {instruction.longRegisterValue} ";
        }
    }
}