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

namespace SEBIS.Shared
{
    /// <summary>
    /// Opcode utility class.
    /// </summary>
    public static class OpcodesUtils
    {
        /// <summary>
        /// Convert VariableLengthOpcode to binary representation (string).
        /// </summary>
        /// <param name="OpcodeVariable"></param>
        /// <returns>Returns string representation of the opcode</returns>
        public static string ToBinaryString(this VariableLengthOpcode OpcodeVariable)
        {
            if (OpcodeVariable.length == OpcodeByteLength.Short)
                return Convert.ToString(OpcodeVariable.opcodeValueAsDecimal, 2).PadLeft(3, '0');
            else if (OpcodeVariable.length == OpcodeByteLength.Long)
                return Convert.ToString(OpcodeVariable.opcodeValueAsDecimal, 2).PadLeft(11, '0');
            else
                return Convert.ToString(OpcodeVariable.opcodeValueAsDecimal, 2).PadLeft(19, '0');
        }


        /// <summary>
        /// Convert decimal opcode to binary opcode
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns>binary opcode (last 3 bits only should be used) as uint</returns>
        public static uint DecimalToShortOpcode(this Opcodes opcode) {
            // Return only the last 3 bits of the opcode
            return (uint)opcode & 0b00000111;
        }
        
        /// <summary>
        /// Convert decimal opcode to binary opcode
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns>binary opcode (last 11 bits only should be used) as uint</returns>
        public static uint DecimalToLongOpcode(this Opcodes opcode) {
            // Return only the last 11 bits of the opcode
            return (uint)opcode & 0b00001111111;
        }

        /// <summary>
        /// Convert decimal opcode to binary opcode
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns>binary opcode (last 19 bits only should be used) as uint</returns>
        public static uint DecimalToExtraLongOpcode(this Opcodes opcode) {
            // Return only the last 19 bits of the opcode
            return (uint)opcode & 0b0000000000001111111111;
        }
    }
}