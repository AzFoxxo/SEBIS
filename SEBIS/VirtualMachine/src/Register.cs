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
    /// Register struct contains a singular byte value which can be configured with permissions to read, write.
    /// </summary>
    public struct Register
    {
        private byte value;
        public bool readable { get; private set; }
        public bool writable { get; private set; }

        /// <summary>
        /// Create new register
        /// </summary>
        /// <param name="value">Uninitialised value</param>
        /// <param name="readable">Readable</param>
        /// <param name="writable">Writable</param>
        public Register(bool readable = false, bool writable = false, byte value = 0)
        {
            this.value = value;
            this.readable = readable;
            this.writable = writable;
        }

        /// <summary>
        /// Read value from the register. Returns (true, value) if read successful, (false, 0) if not readable.
        /// </summary>
        /// <param name="forceRead">Force read</param>
        /// <returns>Tuple of if read was successful and byte value</returns>
        public (bool, byte) Read(bool forceRead = false)
        {
            if (readable || forceRead)
                return (true, value);

            return (false, 0);
        }

        /// <summary>
        /// Write value to the register. Returns (true) if write successful, (false) if not writable. 
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="forceWrite">Force write</param>
        /// <returns>Returns if write was successful</returns>
        public bool Write(byte value, bool forceWrite = false)
        {
            if (writable || forceWrite)
            {
                this.value = value;
                return true;
            }

            return false;
        }
    }
}