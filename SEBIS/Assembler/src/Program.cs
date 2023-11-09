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
            // Byte 1-4: `QKVT` in ASCII
            // Byte 5-8: ROM format version
            // Byte 9: ROM size in bytes after the header
            string magic = "QKVT";
            byte versionMajor = 1;
            byte versionIntermediate = 0;
            byte versionMinor = 0;
            byte initAddressInRom = 0;
            byte romSize = 0;

            // Find the label for `init`'s corresponding address
            initAddressInRom = labels.Find(label => label.name == "init").address;

            // Calculate the rom size
            // Instructions are 1 byte with an optional extra byte for operands
            foreach (var instruction in instructions)
            {
                romSize += (byte)(instruction.IncludeLongRegisterInROM ? 2 : 1);
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
                writer.Write(initAddressInRom);
                writer.Write(romSize);

                // Write the instructions
                foreach (var instruction in instructions)
                {
                    var flag = instruction.flagBitValue ? 0b1 : 0b0; // First bit of byte
                    var IncludeLongRegisterInROM = instruction.IncludeLongRegisterInROM ? 0b1 : 0b0; // Second bit of byte
                    var op = instruction.opcode.DecimalToShortOpcode(); // Last six bits of byte

                    // Combine flag, lng, and op into a single byte
                    byte machineCode = (byte)((flag << 7) | (IncludeLongRegisterInROM << 6) | op);

                    // Write byte
                    writer.Write(machineCode);

                    // Long register
                    if (instruction.IncludeLongRegisterInROM)
                    {
                        // Remove $ - address from operand
                        var operand = instruction.longRegisterValue.Replace("$", "");

                        // Convert hex to byte
                        if (Byte.TryParse(operand, System.Globalization.NumberStyles.HexNumber, null, out byte value) && instruction.longRegisterValue.Contains('$'))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"`{instruction.longRegisterValue}` to `{operand}` to `{Convert.ToByte(value)}`");
                            Console.ResetColor();
                        }
                        // Convert decimal to byte
                        else if (Byte.TryParse(operand, out value))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"`{instruction.longRegisterValue}` to `{operand}` to `{Convert.ToByte(value)}`");
                            Console.ResetColor();
                        }
                        else
                        {
                            // Log error
                            Console.WriteLine($"Error: Invalid long register {instruction.longRegisterValue}");
                            Environment.Exit((int)ErrorCodes.InvalidLongRegister);
                            return;
                        }

                        // Write the byte
                        writer.Write(value);
                    }
                }

                // Write the end of file
                writer.Write(System.Text.Encoding.ASCII.GetBytes(magic));
            }

        }

        private static List<Instruction> LineTokenisation(string[] lines)
        {
            List<Instruction> instructions = new List<Instruction>();

            // Get enum name of Opcodes using reflection
            // SET is not a machine instruction so contains no definition in Opcodes so must be manually added
            string[] enumNames = Enum.GetNames(typeof(Opcodes)).Concat(new[] { "SET" }).ToArray();

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
            if (tokens[0].ToUpper() == "SUB") return new Instruction(Opcodes.SUB, true, false, "", OpcodeByteLength.Short, AddressMode.REGISTER); // SUB flag
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
