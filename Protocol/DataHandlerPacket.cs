using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    /// <summary>
    /// Structure defining the necessary data for a handlerPacket
    /// </summary>
    public class DataHandlerPacket : PacketBase
    {
        private byte[] SOF;
        public ushort sof
        {
            set { SOF = Toolbox.UInt16ToBytes(value, Endian.Big); }
            get { return Toolbox.BytesToUInt16(SOF, Endian.Big); }
        }

        private byte PROTOCOL_VERSION;
        public ProtocolVersion version
        {
            set { PROTOCOL_VERSION = (byte)value; }
            get { return (ProtocolVersion)PROTOCOL_VERSION; }
        }

        private byte APP_ID;
        public AppID appID
        {
            set { APP_ID = (byte)value; }
            get { return (AppID)APP_ID; }
        }

        private byte DATA_TYPE;
        public DataType dataType
        {
            set { DATA_TYPE = (byte)value; }
            get { return (DataType)DATA_TYPE; }
        }

        public ushort payloadSize
        {
            set { PAYLOAD_SIZE = Toolbox.UInt16ToBytes(value, Endian.Big); }
            get { return Toolbox.BytesToUInt16(PAYLOAD_SIZE, Endian.Big); }
        }
        private byte[] PAYLOAD_SIZE;
         
        public byte[] data
        {
            set {PAYLOAD_DATA = value;}
            get { return PAYLOAD_DATA; }
        }
        private byte[] PAYLOAD_DATA;

        public UInt32 rptMs
        {
            //This code should be looked at, inversion is due to buffer blockCopy
            //in decode() method
            set { RPT_MS = Toolbox.UInt32ToBytes(value, Endian.Little); }
            get { return Toolbox.BytesToUInt32(RPT_MS, Endian.Big); }
        }
        private byte[] RPT_MS;

        private byte[] EOF;
        public ushort eof
        {
            set { EOF = Toolbox.UInt16ToBytes(value, Endian.Big); }
            get { return Toolbox.BytesToUInt16(EOF, Endian.Big); }
        }

        /// <summary>
        /// Total size of the packet
        /// </summary>
        public int totalSize
        {
            get { return SIZES.SOF + SIZES.ProtocolVersion + SIZES.AppID + SIZES.DataType + SIZES.PayloadSize + payloadSize + SIZES.RPT + SIZES.SOF; }
        }

        public DataHandlerPacket(ProtocolVersion version, AppID appID, DataType dataType, byte[] data, UInt32 RPTms = 0)
        {
            this.version = version;
            this.appID = appID;
            this.dataType = dataType;
            this.data = data;
            this.rptMs = RPTms;

            payloadSize = (ushort)data.Length;

            sof = CONSTANTS.SOF;
            eof = CONSTANTS.EOF;
        }

        public DataHandlerPacket() { }

        public override byte[] Encode()
        {
            int ptr = 0;
            byte[] flatData = new byte[totalSize];

            //Adding SOF
            Buffer.BlockCopy(SOF, 0, flatData, 0, SIZES.SOF);
            ptr += SIZES.SOF;

            //Adding version, ID and data type
            flatData[ptr++] = PROTOCOL_VERSION;
            flatData[ptr++] = APP_ID;
            flatData[ptr++] = DATA_TYPE;

            //Adding DATA Len
            Buffer.BlockCopy(PAYLOAD_SIZE, 0, flatData, ptr, SIZES.PayloadSize);
            ptr += SIZES.PayloadSize;

            //Adding DATA
            Buffer.BlockCopy(PAYLOAD_DATA, 0, flatData, ptr, payloadSize);
            ptr += payloadSize;

            //Adding RPT
            Buffer.BlockCopy(RPT_MS, 0, flatData, ptr, SIZES.RPT);
            ptr += SIZES.RPT;

            //Adding EOF
            Buffer.BlockCopy(EOF, 0, flatData, ptr, SIZES.EOF);
            ptr += SIZES.EOF;

            return flatData;
        }

        /// <summary>
        /// Decodes the encoded packet
        /// </summary>
        /// <param name="encodedData">The encoded bytes</param>
        /// <returns>A reconstructed DataHandlerPacket </returns>
        public override void Decode(byte[] encodedData)
        {
            int ptr = 0;
            
            //Decoding SOF
            SOF = new byte[SIZES.SOF];
            SOF[0] = encodedData[ptr++];
            SOF[1] = encodedData[ptr++];

            //Decoding next bytes...
            PROTOCOL_VERSION = encodedData[ptr++];
            APP_ID = encodedData[ptr++];
            DATA_TYPE = encodedData[ptr++];

            //Decoding payload size
            PAYLOAD_SIZE = new byte[SIZES.PayloadSize];
            PAYLOAD_SIZE[0] = encodedData[ptr++];
            PAYLOAD_SIZE[1] = encodedData[ptr++];

            //Decoding payload data
            PAYLOAD_DATA = new byte[payloadSize];
            Buffer.BlockCopy(encodedData, ptr, PAYLOAD_DATA, 0, payloadSize);
            ptr += payloadSize;

            //Decoding RPT
            RPT_MS = new byte[SIZES.RPT];
            Buffer.BlockCopy(encodedData, ptr, RPT_MS, 0, SIZES.RPT);
            ptr += SIZES.RPT;

            //Decoding EOF
            EOF = new byte[SIZES.EOF];
            EOF[0] = encodedData[ptr++];
            EOF[1] = encodedData[ptr++];

        }
    }
}
