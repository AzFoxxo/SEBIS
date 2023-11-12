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
    public class Memory
    {
        // Memory size
        public ushort Size { get; private set; }

        // Memory pointer
        private ContiguousMemory memory;

        // Memory protections
        private MemoryProtections[] protections;

        // Constructor
        public Memory(ushort size, MemoryProtections[] protections)
        {
            // Memory protections
            this.protections = protections;

            // Create the memory pool
            memory = new ContiguousMemory(size);
        }

        /// <summary>
        /// Check for violation of memory protections
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="read">Check for read</param>
        /// <param name="write">Check for write</param>
        /// <param name="execute">To check for execute</param>
        /// <returns>Return if there was a violation</returns>
        private bool CheckViolation(ushort address, bool read, bool write, bool execute)
        {
            bool violation = false;
            foreach (var protection in protections)
            {
                if (protection.lowBound <= address)
                {
                    if (protection.highBound >= address)
                    {
                        if (read && protection.disallowRead) violation = true;
                        else if (write && protection.disallowWrite) violation = true;
                        else if (execute && protection.disallowExecute) violation = true;
                    }
                }
            }

            return violation;
        }

        /// <summary>
        /// Write a byte to memory with bounds checking
        /// </summary>
        /// <param name="address">address (index) to write to</param>
        /// <param name="value">Byte to write</param>
        /// <returns>True if write was successful</returns>
        public bool Write(ushort address, byte value)
        {
            // check within bounds
            if (address >= Size)
            {
                if (!CheckViolation(address, false, true, false))
                {
                    memory.WriteUnsafe(address, value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Write a byte to memory with bounds checking
        /// </summary>
        /// <param name="address">address (index) to write to</param>
        /// <param name="value">Byte to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteExecution(ushort address, byte value)
        {
            // check within bounds
            if (address >= Size)
            {
                if (!CheckViolation(address, false, false, true))
                {
                    memory.WriteUnsafe(address, value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Read a byte from memory with bounds checking
        /// </summary>
        /// <param name="address">address (index) to read</param>
        /// <returns>Tuple if read was successful and byte value</returns>
        public (bool, byte) Read(ushort address)
        {
            // check within bounds
            if (address >= Size)
            {
                if (!CheckViolation(address, true, false, false)) return (true, memory.ReadUnsafe(address));
            }

            return (false, 0);
        }

        /// <summary>
        /// Read byte as execution with bounds checking
        /// </summary>
        /// <param name="address">address (index) to read</param>
        /// <returns>Tuple if read was successful and byte value</returns>
        public (bool, byte) ReadExecution(ushort address)
        {
            // check within bounds
            if (address >= Size)
            {
                if (!CheckViolation(address, false, false, true)) return (true, memory.ReadUnsafe(address));
            }

            return (false, 0);
        }
    }
}