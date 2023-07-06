using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    /// <summary>
    /// Bytes corresponding to Application ID
    /// </summary>
    public enum AppID
    {
        NeoPlasma = 0xD0
    }

    /// <summary>
    /// Bytes corresponding to Protocol version
    /// </summary>
    public enum ProtocolVersion
    {
        V1_0 = 0x01
    }

    /// <summary>
    /// Byte corresponding to the handler packet data type
    /// </summary>
    public enum DataType
    {
        ClientAuthentication = 0x01,
        SerializedObject = 0x03,
        ClientDeAuthentication = 0x05,
        PacketAck = 0xFF
    }
    
    /// <summary>
    /// IDs for serialized objects
    /// </summary>
    public enum ObjectID
    {
        Test_Object = 0xBEEF
    }

    public static class SIZES
    {
        public static readonly int SOF = 2;
        public static readonly int ProtocolVersion = 1;
        public static readonly int AppID = 1;
        public static readonly int DataType = 1;
        public static readonly int PayloadSize = 2;
        public static readonly int RPT = 4;
        public static readonly int EOF = 2;

        public static readonly int OBJECT_ID = 2;
        public static readonly int SERIALIZED_DATA_SIZE = 2;
    }

    /// <summary>
    /// Bytes corresponding to Protocol constants such as SOF, EOF...
    /// </summary>
    public static class CONSTANTS 
    {
        public static readonly ushort SOF = 0xFFEE;
        public static readonly ushort EOF = 0xEEFF;
    }


}
