using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    internal enum Endian
    {
        Little,
        Big
    }

    internal static class Toolbox
    {
        public static byte[] UInt16ToBytes(UInt16 value, Endian endian)
        {
            byte[] bytes = new byte[2];

            //Read direction :
            // [LSB] [MSB]
            if (endian == Endian.Big)
            {
                bytes[1] = (byte)(value >> 0);
                bytes[0] = (byte)(value >> 8);
            }

            //Read direction :
            // [MSB] [LSB]
            else if (endian == Endian.Little)
            {
                bytes[1] = (byte)(value >> 8);
                bytes[0] = (byte)(value >> 0);
                      
            }

            return bytes;
        }

        public static UInt16 BytesToUInt16(byte[] bytes, Endian endian)
        {
            UInt16 result = new UInt16();

            if(endian == Endian.Big)
            {
                result = (ushort)((bytes[1] << 0) + (bytes[0] << 8));
            }

            else if (endian == Endian.Little)
            {
                result = (ushort)((bytes[0] << 0) + (bytes[1] << 8));
            }

            return result;
        }

        public static byte[] UInt32ToBytes(UInt32 value, Endian endian) 
        {
            byte[] bytes = new byte[4];

            //Read direction :
            // [LSB] [MSB]
            if (endian == Endian.Big)
            {
                for(int i = bytes.Length - 1; i >= 0; i--)
                {
                    bytes[i] = (byte)(value >> 8 * i);
                }
            }

            //Read direction :
            // [MSB] [LSB]
            else if (endian == Endian.Little)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(value >> 8 * (bytes.Length - 1 - i));
                }
            }

            return bytes;
        }

        public static UInt32 BytesToUInt32(byte[] bytes, Endian endian)
        {
            UInt32 result = new UInt32();

            if(endian == Endian.Big)
            {
                result = (UInt32)((bytes[3] << 0) + (bytes[2] << 8) + (bytes[1] << 16) + (bytes[0] << 24));
            }

            else if (endian == Endian.Little)
            {
                result = (UInt32)((bytes[0] << 0) + (bytes[1] << 8) + (bytes[2] << 16) + (bytes[3] << 24));
            }

            return result;

        }

    }
}
