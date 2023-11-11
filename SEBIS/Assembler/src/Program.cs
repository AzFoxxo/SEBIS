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

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using SEBIS.Shared;

namespace SEBIS.Assembler
{
    class Program
    {
        private static List<Label> labels = new();
        private static List<Instruction> instructions = new();

        static void Main(string[] args)
        {
            // Check if args is one
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: assembler <filename>");
                return;
            }

            // Read the contents of the file and split it into lines
            string[] lines = ReadFile(args[0]).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            // Output file
            string outputRom = "ROM.sebin";

            #region Clean up
            // Discard any empty lines
            lines = lines.Where(line => !string.IsNullOrEmpty(line)).ToArray();

            // Discard any lines starting with ; even if space before
            lines = lines.Where(line => !line.Trim().StartsWith(";")).ToArray();

            // Clear post instruction comments
            lines = lines.Select(line => line.Split(';')[0].Trim()).ToArray();
            #endregion

            // Print each line
            foreach (var line in lines)
            {
                // Change colour to blue for :
                if (line.Trim().StartsWith(":"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(line);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(line);
                }
            }

            // Label parsing
            (labels, lines) = FindLabels(lines);

            // Check if labels contains init label
            // init label
            if (!labels.Exists(label => label.name == "init"))
            {
                Console.WriteLine("Error: No init label found");
                Environment.Exit((int)ErrorCodes.MissingRequiredLabels);
                return;
            }
            // end label
            if (!labels.Exists(label => label.name == "end"))
            {
                Console.WriteLine("Error: No end label found");
                Environment.Exit((int)ErrorCodes.MissingRequiredLabels);
                return;
            }
            // data label
            if (!labels.Exists(label => label.name == "panic"))
            {
                Console.WriteLine("Error: No panic label found");
                Environment.Exit((int)ErrorCodes.MissingRequiredLabels);
                return;
            }

            // Label resolution
            lines = LabelResolution(lines, labels);

            // Line by line tokenisation
            instructions = LineTokenisation(lines);

            // Print the tokens a table
            foreach (var instruction in instructions)
            {
                Console.WriteLine(instruction);
            }

            // Final binary construction
            WriteRom(outputRom, instructions, labels);
        }

        /// <summary>
        /// Write the bytes to the file
        /// </summary>
        /// <param name="outputRom"></param>
        /// <param name="instructions"></param>
        /// <param name="labels"></param>
        private static void WriteRom(string outputRom, List<Instruction> instructions, List<Label> labels)
        {
            // Header info
            // Byte 1-6: `.SEBIS` in ASCII
            // Byte 5-9: ROM format version
            // Byte 9: ROM size in bytes after the header
            string magic = ".SEBIS";
            byte versionMajor = 1;
            byte versionIntermediate = 0;
            byte versionMinor = 0;
            byte initAddressInRom = 0;
            uint romSize = 6; // Add 2 for each label in ROM (end, panic, init)

            // Calculate the rom size
            // Instructions are 1 byte with an optional extra byte for operands
            foreach (var instruction in instructions)
            {
                // Short instructions are 1 byte
                // Long instructions are 2 bytes
                // Extra long instructions are 3 bytes
                // Constant + 1 byte
                // Address + 2 bytes

                ushort sizeOfInstruction = 0;

                // Base opcode size
                if (instruction.opcodeLength == OpcodeByteLength.Short) sizeOfInstruction = 1;  // Short
                if (instruction.opcodeLength == OpcodeByteLength.Long) sizeOfInstruction = 2;   // Long
                if (instruction.opcodeLength == OpcodeByteLength.Extra) sizeOfInstruction = 3;  // Extra long

                // Add long register value in size calculation
                if (instruction.IncludeLongRegisterInROM)
                {
                    // Memory address
                    if (instruction.addressMode == AddressMode.MEMORY) sizeOfInstruction += 2;

                    // Constant
                    if (instruction.addressMode == AddressMode.CONSTANT) sizeOfInstruction += 1;
                }

                romSize += sizeOfInstruction;
            }

            Console.WriteLine($"ROM instruction size: {romSize} bytes out of {byte.MaxValue} bytes supported");

            // Delete the file if it exists
            if (File.Exists(outputRom)) File.Delete(outputRom);


            using (BinaryWriter writer = new BinaryWriter(File.Open(outputRom, FileMode.Create)))
            {
                // Write the header
                writer.Write(System.Text.Encoding.ASCII.GetBytes(magic));
                writer.Write(versionMajor);
                writer.Write(versionIntermediate);
                writer.Write(versionMinor);
                writer.Write(romSize);

                // Machine code to generate hash on
                List<byte> machineCodeAll = new();

                // Write address of `init`, `panic` and `end` sections
                ushort m_init = labels.Find(label => label.name == "init").address;
                ushort m_panic = labels.Find(label => label.name == "panic").address;
                ushort m_end = labels.Find(label => label.name == "end").address;
                // Write, log and add to all code (first and second byte of ushort)
                writer.Write(m_init); Console.WriteLine($"ADDR {m_init}"); machineCodeAll.Add((byte)(m_init >> 8)); machineCodeAll.Add((byte)(m_init & 0xFF));
                writer.Write(m_init); Console.WriteLine($"ADDR {m_panic}"); machineCodeAll.Add((byte)(m_panic >> 8)); machineCodeAll.Add((byte)(m_panic & 0xFF));
                writer.Write(m_init); Console.WriteLine($"ADDR {m_end}"); machineCodeAll.Add((byte)(m_end >> 8)); machineCodeAll.Add((byte)(m_end & 0xFF));

                // Write the instructions
                foreach (var instruction in instructions)
                {
                    // Instruction variables to write
                    byte instructionSize = (byte)instruction.opcodeLength; // First two bits - instruction size
                    byte instructionFlag = (byte)(instruction.flagBitValue ? 1 : 0); // Second two bits - instruction flag
                    byte addressMode = (byte)instruction.addressMode; // Second two bits - address mode

                    // Create a byte array of size three to represent 24 bits plus 16 bit of address or 8 bit constant
                    byte[] machineCodeBytes = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };

                    // Add instructionSize
                    machineCodeBytes[0] |= (byte)(instructionSize << 6);

                    // Add instructionFlag
                    machineCodeBytes[0] |= (byte)(instructionFlag << 5);

                    // Add addressMode
                    machineCodeBytes[0] |= (byte)(addressMode << 3);

                    // Get the three bytes corresponding to each opcode
                    byte[] opcodeBytes = OpcodeLookUp.GetOpcodeBinary(instruction.opcode);

                    // Add the first byte opcode
                    machineCodeBytes[0] |= opcodeBytes[0];
                    machineCodeAll.Add(machineCodeBytes[0]);

                    // Add second byte
                    if (instruction.opcodeLength == OpcodeByteLength.Long || instruction.opcodeLength == OpcodeByteLength.Extra)
                    {
                        machineCodeBytes[1] |= opcodeBytes[1];
                        machineCodeAll.Add(machineCodeBytes[1]);
                    }

                    // Add third byte
                    if (instruction.opcodeLength == OpcodeByteLength.Extra)
                    {
                        machineCodeBytes[2] |= opcodeBytes[2];
                        machineCodeAll.Add(machineCodeBytes[2]);
                    }

                    // Add constant
                    if (instruction.addressMode == AddressMode.CONSTANT)
                    {
                        machineCodeBytes[3] |= ConvertOperandTextToConstantByte(instruction.longRegisterValue);
                        machineCodeAll.Add(machineCodeBytes[3]);
                    }

                    // Add memory address
                    if (instruction.addressMode == AddressMode.MEMORY)
                    {
                        ushort address = ConvertOperandTextTo16BitAddress(instruction.longRegisterValue);
                        machineCodeBytes[3] |= (byte)(address >> 8);
                        machineCodeBytes[4] |= (byte)(address & 0xFF);
                        machineCodeAll.Add(machineCodeBytes[3]);
                        machineCodeAll.Add(machineCodeBytes[4]);
                    }

                    // Print bytes
                    Console.Write("Binary: " + Convert.ToString(machineCodeBytes[0], 2).PadLeft(8, '0'));
                    if (instruction.opcodeLength == OpcodeByteLength.Long || instruction.opcodeLength == OpcodeByteLength.Extra)
                        Console.Write(" : " + Convert.ToString(machineCodeBytes[1], 2).PadLeft(8, '0'));
                    if (instruction.opcodeLength == OpcodeByteLength.Extra)
                        Console.Write(" : " + Convert.ToString(machineCodeBytes[2], 2).PadLeft(8, '0'));
                    if (instruction.addressMode == AddressMode.CONSTANT)
                        Console.Write($" WITH CONSTANT: `{Convert.ToString(machineCodeBytes[3], 2).PadLeft(8, '0')}`");
                    if (instruction.addressMode == AddressMode.MEMORY)
                    {
                        Console.Write($" WITH MEMORY ADDRESS: `{Convert.ToString(machineCodeBytes[3], 2).PadLeft(8, '0')}");
                        Console.Write($"-{Convert.ToString(machineCodeBytes[4], 2).PadLeft(8, '0')}`");
                    }
                    Console.WriteLine();

                    // Write bytes
                    if (instruction.opcodeLength == OpcodeByteLength.Short)
                        writer.Write(machineCodeBytes[0]);
                    if (instruction.opcodeLength == OpcodeByteLength.Long)
                        writer.Write(machineCodeBytes[1]);
                    if (instruction.opcodeLength == OpcodeByteLength.Extra)
                        writer.Write(machineCodeBytes[2]);

                    // Write data
                    if (instruction.addressMode == AddressMode.CONSTANT || instruction.addressMode == AddressMode.MEMORY)
                        writer.Write(machineCodeBytes[3]);
                    if (instruction.addressMode == AddressMode.MEMORY)
                        writer.Write(machineCodeBytes[4]);
                }

                // How much ROM is used out of 64K
                Console.WriteLine($"ROM used: {romSize} bytes out of 65,536 used!");

                // Check if ROM is full
                if (romSize > byte.MaxValue)
                {
                    Console.WriteLine("ROM is full!");
                    Environment.Exit((int)ErrorCodes.RomFull);
                    return;
                }

                // Generate an SHA256 and write it to the end of the file
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] hash = sha256Hash.ComputeHash(machineCodeAll.ToArray());

                    // Print the hash
                    Console.WriteLine("SHA256 of machine code: " + BitConverter.ToString(hash).Replace("-", ""));

                    // Write the hash
                    writer.Write(hash);
                }
            }
        }

        private static ushort ConvertOperandTextTo16BitAddress(string address)
        {
            // Valid address formats include:
            // $ hex

            // Convert $ to 0x
            address = address.Replace("$", "0x");

            // If word does not contain 0x, append at the start of the string
            if (!address.StartsWith("0x")) address = "0x" + address;

            // Try convert string (hexadecimal) to byte
            if (address.StartsWith("0x") && ushort.TryParse(address.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out ushort memoryUshort))
            {
                return memoryUshort;
            }

            // Log error
            Console.WriteLine($"Invalid long register, cannot convert memory address: `{address}`");
            Environment.Exit((int)ErrorCodes.InvalidLongRegister);
            return 0;
        }


        private static byte ConvertOperandTextToConstantByte(string constant)
        {
            // Valid constant formats include:
            // decimal
            // binary (0b)
            // hexadecimal (0x)
            // octal (0)

            // Try convert string (decimal) to byte
            if (byte.TryParse(constant, out byte constantByte))
            {
                return constantByte;
            }

            // Try convert string (binary) to byte
            if (constant.StartsWith("0b") && byte.TryParse(constant.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out constantByte))
            {
                return constantByte;
            }

            // Try convert string (hexadecimal) to byte
            if (constant.StartsWith("0x") && byte.TryParse(constant.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out constantByte))
            {
                return constantByte;
            }

            // Try convert string (octal) to byte
            if (constant.StartsWith("0") && byte.TryParse(constant.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier, null, out constantByte))
            {
                return constantByte;
            }

            // Log error
            Console.WriteLine($"Invalid long register, cannot convert constant: `{constant}`");
            Environment.Exit((int)ErrorCodes.InvalidLongRegister);
            return 0;
        }

        private static List<Instruction> LineTokenisation(string[] lines)
        {
            List<Instruction> instructions = new List<Instruction>();

            // Get enum name of Opcodes using reflection
            // SET is not a machine instruction so contains no definition in Opcodes so must be manually added
            string[] enumNames = Enum.GetNames(typeof(Opcodes)).Concat(new[] { "DIV", "SUB" }).ToArray();

            // Loop through each line
            foreach (var line in lines)
            {
                // Split the line into tokens
                string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Check if the opcode is valid
                if (enumNames.Contains(tokens[0].ToUpper()))
                {
                    Console.WriteLine(tokens[0]);

                    // Consume the line and return the opcode
                    foreach (var instruction in ConsumeLineTokens(tokens))
                    {
                        instructions.Add(instruction);
                    }

                }
                else
                {
                    // Invalid opcode found
                    Console.WriteLine($"Error: Invalid opcode found {tokens[0]}");
                    Environment.Exit((int)ErrorCodes.InvalidOpcode);
                }
            }

            return instructions;
        }

        /// <summary>
        /// Consume all tokens in the line and return a valid instruction
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>instruction from line</returns>
        private static List<Instruction> ConsumeLineTokens(string[] tokens)
        {
            // Single opcode no operand instructions
            // Disregard any misplaced tokens after no operand instructions
            if (tokens[0].ToUpper() == "ABC") return new Instruction(Opcodes.ABC, false, false, "", OpcodeByteLength.Short, AddressMode.REGISTER);
            if (tokens[0].ToUpper() == "SUB") return new Instruction(Opcodes.ABC, true, false, "", OpcodeByteLength.Short, AddressMode.REGISTER); // SUB flag
            else if (tokens[0].ToUpper() == "HLT") return new Instruction(Opcodes.HLT, false, false, "", OpcodeByteLength.Long, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "NOP") return new Instruction(Opcodes.NOP, false, false, "", OpcodeByteLength.Short, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "MUL") return new Instruction(Opcodes.MUL, false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);
            else if (tokens[0].ToUpper() == "DIV") return new Instruction(Opcodes.MUL, true, false, "", OpcodeByteLength.Long, AddressMode.REGISTER); // MUL flag
            else if (tokens[0].ToUpper() == "XOR") return new Instruction(Opcodes.XOR, false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);
            else if (tokens[0].ToUpper() == "OR") return new Instruction(Opcodes.OR, false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);
            else if (tokens[0].ToUpper() == "AND") return new Instruction(Opcodes.AND, false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);
            else if (tokens[0].ToUpper() == "SYSCALL") return new Instruction(Opcodes.SYSCALL, false, false, "", OpcodeByteLength.Short, AddressMode.REGISTER);
            else if (tokens[0].ToUpper() == "PANIC") return new Instruction(Opcodes.PANIC, false, false, "", OpcodeByteLength.Long, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "MBNKROM") return new Instruction(Opcodes.MBNKROM, false, false, "", OpcodeByteLength.Long, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "MBNKRAM") return new Instruction(Opcodes.MBNKRAM, false, false, "", OpcodeByteLength.Long, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "ZERO") return new Instruction(Opcodes.ZERO, false, false, "", OpcodeByteLength.Long, AddressMode.NULL);
            else if (tokens[0].ToUpper() == "CLD") return new Instruction(Opcodes.CLD, false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);

            // Jump instructions
            switch (tokens[0].ToUpper())
            {
                case "JMP":
                case "JZ":
                case "JNZ":
                    Opcodes op;
                    if (tokens[0].ToUpper() == "JMP") op = Opcodes.JMP;
                    else if (tokens[0].ToUpper() == "JNZ") op = Opcodes.JNZ;
                    else if (tokens[0].ToUpper() == "JZ") op = Opcodes.JZ;
                    else break;
                    if (tokens.Length == 2) return new Instruction(op, false, true, tokens[1], OpcodeByteLength.Long, AddressMode.MEMORY);
                    else if (tokens.Length < 2)
                    { // Allow too many
                        Console.WriteLine("Error: Invalid number of operands for jump instructions");
                        Environment.Exit((int)ErrorCodes.InvalidNumberOfArguments);
                    }
                    break;
            }


            // Comparison instructions
            switch (tokens[0].ToUpper())
            {
                case "CPL":
                case "CMP":
                    Opcodes op;
                    if (tokens[0].ToUpper() == "CPL") op = Opcodes.CPL;
                    else if (tokens[0].ToUpper() == "CMP") op = Opcodes.CMP;
                    else break;
                    return new Instruction(op, tokens.Length == 2 ? (tokens[1] != "0" ? true : false) : false, false, "", OpcodeByteLength.Long, AddressMode.REGISTER);
            }

            // Load register instructions
            switch (tokens[0].ToUpper())
            {
                case "LDA":
                case "LDB":
                    Opcodes op;
                    if (tokens[0].ToUpper() == "LDA") op = Opcodes.LDA;
                    else if (tokens[0].ToUpper() == "LDB") op = Opcodes.LDB;
                    else if (tokens[0].ToUpper() == "LDL") op = Opcodes.LDL;
                    else if (tokens[0].ToUpper() == "LDH") op = Opcodes.LDH;
                    else break;
                    if (tokens.Length < 2)
                    { // Allow too many
                        Console.WriteLine("Error: Invalid number of operands for load instructions");
                        Environment.Exit((int)ErrorCodes.InvalidNumberOfArguments);
                    }

                    // This could be AddressMode.CONSTANT or AddressMode.MEMORY

                    // Check if the operand is an address
                    if (tokens[1].StartsWith("$"))
                    {
                        return new Instruction(op, false, true, tokens[1], OpcodeByteLength.Short, AddressMode.MEMORY);
                    }

                    return new Instruction(op, false, true, tokens[1], OpcodeByteLength.Short, AddressMode.CONSTANT);
            }

            // Store register instructions
            switch (tokens[0].ToUpper())
            {
                case "STA":
                case "STB":
                case "STC":
                    Opcodes op;
                    if (tokens[0].ToUpper() == "STA") op = Opcodes.STA;
                    else if (tokens[0].ToUpper() == "STB") op = Opcodes.STB;
                    else if (tokens[0].ToUpper() == "STC") op = Opcodes.STC;
                    else break;
                    if (tokens.Length < 2)
                    { // Allow too many
                        Console.WriteLine("Error: Invalid number of operands for store instructions");
                        Environment.Exit((int)ErrorCodes.InvalidNumberOfArguments);
                    }
                    return new Instruction(op, false, true, tokens[1], OpcodeByteLength.Short, AddressMode.MEMORY);
            }

            // Error
            Console.WriteLine("Error: Invalid opcode found");
            return new Instruction(Opcodes.NOP, false, false, "", OpcodeByteLength.Short, AddressMode.NULL);
        }

        /// <summary>
        /// Finds all labels in the lines provided and returns a list of Labels and cleaned up lines without label declarations
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static Tuple<List<Label>, string[]> FindLabels(string[] lines)
        {
            List<Label> labels = new();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i].Trim().StartsWith(":"))
                {
                    // Store the name and discard the character letter `:`
                    var name = lines[i].Trim()[1..];

                    // Get the current line number (factoring in labels)
                    byte lineNumber = ((byte)(i - labels.Count));

                    // Print label in red
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(name);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(" " + lineNumber);
                    Console.WriteLine();

                    // Append the label to the list
                    labels.Add(new Label(lineNumber, name));
                }
            };

            // Delete label declarations
            lines = lines.Where(line => !line.Trim().StartsWith(":")).ToArray();

            return new Tuple<List<Label>, string[]>(labels, lines);
        }

        /// <summary>
        /// Label resolution: replaces all labels with the address
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="labels"></param>
        /// <returns>All lines with labels replaced</returns>
        private static string[] LabelResolution(string[] lines, List<Label> labels)
        {
            // Loop through all instructions
            for (int i = 0; i < lines.Length; i++)
            {
                // If line begins with JMP, JZ or JNZ, find and replace the label with the direct address
                if (lines[i].Trim().StartsWith("JMP") || lines[i].Trim().StartsWith("JZ") || lines[i].Trim().StartsWith("JNZ"))
                {
                    // Print until the space in yellow
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(lines[i].Split(' ')[0] + " ");
                    Console.ResetColor();


                    // Check if there is a label after the instruction
                    string[] parts = lines[i].Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        // Print the label in blue
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(parts[1]);
                        Console.ResetColor();

                        // Replace the label with the direct address in the line
                        Label label = labels.Find(l => l.name == parts[1]);

                        // Check the label isn't null
                        if (label.name == null)
                        {
                            // Invalid label name
                            Console.WriteLine("Error: Invalid label name");
                            Environment.Exit((int)ErrorCodes.UnknownLabelName);
                        }

                        // Replace label
                        lines[i] = lines[i].Replace(parts[1], label.address.ToString());
                        Console.WriteLine($"Now: {lines[i]}");
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// Read the contents a file and validate the file extension and file exists
        /// </summary>
        /// <param name="file"></param>
        /// <returns>A string representation of the file contents</returns>
        private static string ReadFile(string file)
        {
            // Check the file exists
            if (!File.Exists(file))
            {
                Console.WriteLine($"Error: File does not exist {file}");
                Environment.Exit((int)ErrorCodes.MissingFile);
            }

            // Check if file extension ends in .qkvt (lower case only)
            if (!file.EndsWith(".asebis"))
            {
                Console.WriteLine("Error: File extension must be .asebis");
                Environment.Exit((int)ErrorCodes.InvalidFileExtension);
            }

            // Read the file
            var contents = File.ReadAllText(file) ?? "";

            return contents;
        }
    }
}
