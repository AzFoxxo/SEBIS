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
    /// DoubleRegister struct is an abstraction of a 16-bit register.
    /// </summary>
    public struct DoubleRegister
    {
        private Register A, B;
        public bool readable { get; private set; }
        public bool writable { get; private set; }

        public DoubleRegister(bool readable = false, bool writable = false, byte value = 0)
        {
            // Create the registers
            A = new Register(readable, writable, value);
            B = new Register(readable, writable, value);
        }

        /// <summary>
        /// Read value from the register. Returns (true, value) if read successful, (false, 0) if not readable.
        /// </summary>
        /// <param name="forceRead">Force read</param>
        /// <returns>Tuple of if read was successful and byte value</returns>
        public (bool, ushort) Read(bool forceRead = false)
        {
            // Get the values from both A and B
            (bool, byte) a = A.Read();
            (bool, byte) b = B.Read();

            return (a.Item1, (ushort)((a.Item2 << 8) | b.Item2));
        }

        /// <summary>
        /// Write value to the register. Returns (true) if write successful, (false) if not writable. 
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="forceWrite">Force write</param>
        /// <returns>Returns if write was successful</returns>
        public bool Write(ushort value, bool forceWrite = false)
        {
            // Split the value into two bytes
            byte a = (byte)(value >> 8);
            byte b = (byte)(value & 0xFF);

            // Write the bytes to the registers
            A.Write(a);
            return B.Write(b);
        }
    }
}