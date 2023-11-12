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

namespace SEBIS.VirtualMachine
{
    public class CPU
    {
        public Register A, B, C, L, H;   // 8 byte registers
        public DoubleRegister LRAP; // 16 byte registers

        public bool M; // 1 bit register

        // PC
        ushort PC;

        public CPU()
        {
            // Create the registers

            // General purpose registers
            A = new Register(true, true);
            B = new Register(true, true);
            C = new Register(true, false);

            // High and low registers (double registers but need to be addressed as singular so low and high)
            L = new Register(false, true);
            H = new Register(false, true);

            // Special registers
            M = true;
            PC = 0;

            // Double registers
            LRAP = new DoubleRegister(false, false);
        }

        public void Entry(Memory ROM, Memory RAM)
        {
            // Set the PC to the address specified in the ROM (bytes 1-2)
            var initLocation = new byte[] { ROM.ReadExecution(0).Item2, ROM.ReadExecution(1).Item2 };

            // Convert to ushort
            PC = BitConverter.ToUInt16(initLocation, 0);

            while (true)
            {
                Execute(RAM, ROM);
            }
        }

        public void Execute(Memory RAM, Memory ROM)
        {
            // Get current time

            // Read the next instruction from the program counter
            byte b = RetrieveByteAndAdvance(RAM, ROM);

            // Decode instruction metadata

            // First two bits of byte (size)
            var size = b >> 6;

            // Third bit of byte (flip)
            var flip = (b >> 5) & 1;

            // Fourth and fifth bits of byte (mode)
            var mode = (b >> 3) & 3;

            // 6-8 bits of byte (opcode)
            var opcode = b & 0x3F;
            byte opcodeS = b;

            // If size is 1, get other byte
            byte opcodeL = 0;
            byte opcodeX = 0;
            if (size == 1) opcodeL = RetrieveByteAndAdvance(RAM, ROM);
            if (size == 2) opcodeX = RetrieveByteAndAdvance(RAM, ROM);

            // Constant
            byte longReg1 = 0;
            byte longReg2 = 0;
            if (size == (byte)AddressMode.MEMORY)
            {
                longReg1 = RetrieveByteAndAdvance(RAM, ROM);
                longReg2 = RetrieveByteAndAdvance(RAM, ROM);
            }
            if (size == (byte)AddressMode.CONSTANT)
            {
                longReg1 = RetrieveByteAndAdvance(RAM, ROM);
            }

            // Print
            Console.WriteLine($"Size: {size + 1}, Flip: {flip}, Mode: {mode}, Opcode {opcodeS}S {opcodeL}L {opcodeX}X, Long: {longReg1}:{longReg2}");

            // Send data to execution
            InstructionExecution(new byte[] { opcodeS, opcodeL, opcodeX, longReg1, longReg2 }, ROM, RAM);

            // TODO: Wait till sufficient time has passed to execute the next instruction (5HZ)
            Thread.Sleep(1000);
        }

        private void InstructionExecution(byte[] raw, Memory ROM, Memory RAM)
        {
            // Instruction size (two bits of first byte)
            var size = raw[0] >> 6;

            // Instruction flip (third bit of first byte)
            var flip = ((raw[0] >> 5) & 1) == 1 ? true : false;

            // Instruction mode (fourth and fifth bits of first byte)
            var mode = (raw[0] >> 3) & 3;

            // Last three bits of first byte (opcode)
            var opcode = raw[0] & 0x3F;

            // Convert opcode to byte (keep only last 3 bits)
            byte opcodeByte = (byte)(opcode & 0x7);

            // Instruction mode
            if (mode == (int)AddressMode.MEMORY)
            {
                // Combine last two bytes to ushort
                ushort address = BitConverter.ToUInt16(raw, 3);
            }
            else if (mode == (int)AddressMode.CONSTANT)
            {
                // Second to last byte is constant
                byte constant = raw[3];
            }

            // Short instructions
            if (size == (int)OpcodeByteLength.Short)
            {
                // Get opcode
                var op = OpcodeLookUp.GetOpcodeFromBinary(new byte[] { opcodeByte, 0, 0 });

                // Lookup and print opcode
                Console.WriteLine($"Opcode: {op}");

                // switch case for opcode
                switch (op)
                {
                    // NOP
                    case Opcodes.NOP:
                        Console.WriteLine("NOP");
                        NOPInstruction:
                        break;

                    // LDA
                    case Opcodes.LDA:
                        Console.WriteLine("LDA");
                        // If LDA constant
                        if (mode == (int)AddressMode.CONSTANT)
                        {
                            A.Write(raw[3]);
                        }
                        // If LDA memory
                        else if (mode == (int)AddressMode.MEMORY)
                        {
                            ushort address = BitConverter.ToUInt16(new byte[] { raw[3], raw[4] }, 0);
                            // TODO: Swap bank
                            A.Write(RAM.ReadExecution(address).Item2);
                        }
                        break;

                    // LDB
                    case Opcodes.LDB:
                        Console.WriteLine("LDB");
                        // If LDB constant
                        if (mode == (int)AddressMode.CONSTANT)
                        {
                            A.Write(raw[3]);
                        }
                        // If LDB memory
                        else if (mode == (int)AddressMode.MEMORY)
                        {
                            ushort address = BitConverter.ToUInt16(new byte[] { raw[3], raw[4] }, 0);
                            // TODO: Swap bank
                            A.Write(RAM.ReadExecution(address).Item2);
                        }
                        break;

                    // STA
                    case Opcodes.STA:
                        Console.WriteLine("STA");
                        ushort memoryAddress = BitConverter.ToUInt16(new byte[] { raw[3], raw[4] }, 0);

                        // TODO: Swap bank
                        RAM.WriteExecution(memoryAddress, A.Read().Item2);
                        break;

                    // STB
                    case Opcodes.STB:
                        Console.WriteLine("STB");
                        memoryAddress = BitConverter.ToUInt16(new byte[] { raw[3], raw[4] }, 0);

                        // TODO: Swap bank
                        RAM.WriteExecution(memoryAddress, A.Read().Item2);
                        break;

                    // STC
                    case Opcodes.STC:
                        Console.WriteLine("STC");
                        memoryAddress = BitConverter.ToUInt16(new byte[] { raw[3], raw[4] }, 0);

                        // TODO: Swap bank
                        RAM.WriteExecution(memoryAddress, A.Read().Item2);
                        break;

                    // SYSCALL
                    case Opcodes.SYSCALL:
                        Console.WriteLine("SYSCALL - performing NOP");
                        goto NOPInstruction; // Not implemented yet
                        break;

                    // ABC
                    default:
                    Console.WriteLine("ABC");
                        // Check flag
                        if (!flip)
                        {
                            C.Write((byte)(A.Read().Item2 + B.Read().Item2));
                        }
                        else
                        {
                            C.Write((byte)(A.Read().Item2 - B.Read().Item2));
                        }
                    break;
                }
            }
            // Long instructions
            else if (size == (int)OpcodeByteLength.Long)
            {
                Console.WriteLine("Long instruction");

                // Lookup and print opcode
                Console.WriteLine("Opcode: " + OpcodeLookUp.GetOpcodeFromBinary(new byte[] { opcodeByte, raw[1], 0 }));
            }
            // Extra long instructions
            else if (size == (int)OpcodeByteLength.Extra)
            {
                Console.WriteLine("Extra long instruction");

                // Lookup and print opcode
                Console.WriteLine("Opcode: " + OpcodeLookUp.GetOpcodeFromBinary(new byte[] { opcodeByte, raw[1], raw[2] }));
            }
            // Error
            else
            {
                Console.WriteLine($"Error length {size}");
            }
        }

        private byte RetrieveByteAndAdvance(Memory ROM, Memory RAM)
        {
            // TODO: RAM reading instead of only ROM
            var data = ROM.ReadExecution(PC).Item2;

            Console.WriteLine("Instruction: " + data + " - $" + PC.ToString("X2"));

            // Print binary of instruction
            Console.WriteLine(Convert.ToString(data, 2).PadLeft(8, '0'));

            // Increase the program counter
            PC++;

            return data;
        }
    }
}