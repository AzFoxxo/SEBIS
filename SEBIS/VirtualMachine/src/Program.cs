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
        }
    }
}