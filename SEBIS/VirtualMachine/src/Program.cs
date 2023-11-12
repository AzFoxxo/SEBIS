﻿/*
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

namespace SEBIS.VirtualMachine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Check one argument is provided
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: virtualmachine <ROM>");
                return;
            }

            // Check the file exists
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File '{args[0]}' does not exist");
                return;
            }

            // Create the CPU
            CPU cpu = new CPU();

            // Create the RAM
            Memory RAM = new Memory(
                ushort.MaxValue,
                new MemoryProtections[] {
                    new MemoryProtections(0, ushort.MaxValue/4, true, true, false), // Disable ROM section in RAM from general read/write
                    new MemoryProtections(ushort.MaxValue/4+1, ushort.MaxValue-1001, false, false, true), // Disable RAM section for execution
                    new MemoryProtections(ushort.MaxValue-1000, ushort.MaxValue, false, false, true)  // Disable IO section for execution
                }
            );

            // Create the ROM
            Memory ROM = new Memory(
                ushort.MaxValue,
                new MemoryProtections[] {
                    new MemoryProtections(0, ushort.MaxValue, false, true, false), // Disable write
                }
            );

            // Load ROM
            LoadROM(ROM, RAM, args[0]);
        }

        private static void LoadROM(Memory ROM, Memory RAM, string filename)
        {
            // Get file size in bytes
            long fileSize = new FileInfo(filename).Length;

            // Check file size is big enough to contain header
            if (fileSize < 11)
            {
                Console.WriteLine($"Error: File '{filename}' is too small to contain a header");
                return;
            }

            // Open ROM file with binary reader
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                // Read header
                byte[] magic = reader.ReadBytes(6);

                // Convert byte array to string
                string magicString = System.Text.Encoding.ASCII.GetString(magic);

                // Check if magic is .SEBIS
                if (magicString != ".SEBIS")
                {
                    Console.WriteLine($"Error: File '{filename}' is not a valid .SEBIS file");
                    return;
                }

                // Read three bytes for version major, intermediate and minor
                byte[] version = reader.ReadBytes(3);
                Console.WriteLine($"File format version: {version[0]}.{version[1]}.{version[2]}");

                // Read two bytes for rom size (ushort)
                ushort romSize = reader.ReadUInt16();
                Console.WriteLine($"ROM size: {romSize} bytes");

                // Bounds check - ROM
                if (fileSize < romSize + 11)
                {
                    Console.WriteLine($"Error: File '{filename}' is too small to contain the specified ROM size");
                    return;
                }

                // Read ROM data
                byte[] romData = reader.ReadBytes(romSize);

                // TODO: checksum

                // Load ROM data into ROM
                for (ushort i = 0; i < romSize; i++)
                {
                    ROM.WriteExecution(i, romData[i]);

                    // Log binary data (convert byte to 1-0)
                    Console.Write(Convert.ToString(romData[i], 2).PadLeft(8, '0'));
                }
                Console.WriteLine();
                Console.WriteLine($"Loaded {romSize} bytes of ROM data into ROM");

                // Copy first 16K of ROM to RAM
                for (ushort i = 0; i < ushort.MaxValue/4; i++)
                {
                    (bool, byte) w = ROM.Read(i);

                    RAM.WriteExecution(i, w.Item2);
                }

                // Print labels
                for (ushort i = 0; i < 6; i++)
                {
                    (bool, byte) w = RAM.ReadExecution(i);i++;
                    (bool, byte) z = RAM.ReadExecution(i);
                    
                    // Turn byte array to ushort
                    var _bytes = new[] { w.Item2, z.Item2 };
                    ushort _value = BitConverter.ToUInt16(_bytes, 0);
                    Console.WriteLine($"Section[{i/2}]: {_value}");
                }
            }
        }
    }
}