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
    public class CPU
    {
        public Register A, B, C, L, H, M;   // 8 byte registers
        public DoubleRegister PC, LRAP; // 16 byte registers

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
            M = new Register(false, true);

            // Double registers
            PC = new DoubleRegister(false, false);
            LRAP = new DoubleRegister(false, false);
        }
    }
}