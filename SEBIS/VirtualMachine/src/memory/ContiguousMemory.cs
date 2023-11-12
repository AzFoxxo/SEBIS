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
    /// <summary>
    /// ContiguousMemory is a contiguous block of memory which can be written to and read from.
    /// </summary>
    unsafe public class ContiguousMemory
    {
        // Memory pointer
        private byte* memory;

        // Constructor
        public ContiguousMemory(ushort size)
        {
            // Create pool of memory
            byte* memory = stackalloc byte[size];

            // Set all values in memory to zero
            for (int i = 0; i < size; i++)
                memory[i] = 0;
        }

        // Finaliser
        ~ContiguousMemory()
        {
            // Free memory
            System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)memory);
        }

        /// <summary>
        /// Write a byte to memory without bounds checking
        /// </summary>
        /// <param name="address">address (index) to write to</param>
        /// <param name="value">Byte to write</param>
        public void WriteUnsafe(ushort address, byte value)
        {
            memory[address] = value;
        }
        
        /// <summary>
        /// Read a byte from memory without bounds checking
        /// </summary>
        /// <param name="address">address (index) to read</param>
        /// <returns>Byte value</returns>
        public byte ReadUnsafe(ushort address)
        {
            return memory[address];
        }
    }
}