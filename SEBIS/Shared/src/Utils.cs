/*
 *   Copyright (c) 2023 Az Foxxo
 *   All rights reserved.
 */
namespace SEBIS.Shared
{
    public class Utils
    {
        public static bool GetBit(byte b, int bitNumber)
        {
            //black magic goes here
            return (b & (1 << bitNumber - 1)) != 0;
        }
        public static uint AppendBits(byte[] bytes)
        {
            uint result = 0;
            int shift = 0;
            foreach (byte b in bytes)
            {
                result |= ((uint)b << shift);
                shift += 8;
            }
            return result;
        }

    }
}