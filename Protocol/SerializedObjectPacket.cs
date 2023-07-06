using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{

    public class SerializedObjectPacket : PacketBase
    {
        /// <summary>
        /// The Serialized object ID
        /// </summary>
        public ObjectID objectID
        {
            //Encode method
            set
            {
                ushort objID = (ushort)value;
                OBJECT_ID = new byte[SIZES.OBJECT_ID];

                OBJECT_ID[SIZES.OBJECT_ID - 2] = (byte)(objID >> 8);
                OBJECT_ID[SIZES.OBJECT_ID - 1] = (byte)(objID >> 0);
            }

            //Decode method
            get
            {
                return (ObjectID)(OBJECT_ID[1] + (OBJECT_ID[0] << 8));
            }
        }
        private byte[] OBJECT_ID;

        /// <summary>
        /// The serialized byte array that represents the object
        /// </summary>
        public byte[] serializedData
        {
            get { return SERIALIZED_DATA; }
            set { SERIALIZED_DATA = value; }
        }
        private byte[] SERIALIZED_DATA;

        /// <summary>
        /// The serialized data size
        /// </summary>
        public ushort serializedDataSize
        {
            //Encode method
            set
            {

                SERIALIZED_DATA_SIZE = new byte[SIZES.SERIALIZED_DATA_SIZE];

                SERIALIZED_DATA_SIZE[SIZES.SERIALIZED_DATA_SIZE - 1] = (byte)(value >> 0);
                SERIALIZED_DATA_SIZE[SIZES.SERIALIZED_DATA_SIZE - 2] = (byte)(value >> 8);
            }

            //Decode method
            get
            {
                return (ushort)(SERIALIZED_DATA_SIZE[1] + (SERIALIZED_DATA_SIZE[0] << 8));
            }
        }
        private byte[] SERIALIZED_DATA_SIZE;

        /// <summary>
        /// The total packet size
        /// </summary>
        public int totalSize
        {
            get
            {
                return SIZES.OBJECT_ID + SIZES.SERIALIZED_DATA_SIZE + serializedDataSize;
            }
        }

        public SerializedObjectPacket(ObjectID SerializedObjectID, byte[] SerializedData)
        {
            //Needed for C# versions < 11
            OBJECT_ID = null;
            SERIALIZED_DATA = null;
            SERIALIZED_DATA_SIZE = null;

            serializedDataSize = (ushort)SerializedData.Length;

            objectID = SerializedObjectID;
            serializedData = SerializedData;

        }

        public SerializedObjectPacket() { }

        public override byte[] Encode()
        {
            int ptr = 0;
            byte[] flatData = new byte[totalSize];

            //Adding ObjectID
            Buffer.BlockCopy(OBJECT_ID, 0, flatData, 0, SIZES.OBJECT_ID);
            ptr += SIZES.OBJECT_ID;

            //Adding data size
            Buffer.BlockCopy(SERIALIZED_DATA_SIZE, 0, flatData, ptr, SIZES.SERIALIZED_DATA_SIZE);
            ptr += SIZES.SERIALIZED_DATA_SIZE;

            //Adding serialized data
            Buffer.BlockCopy(SERIALIZED_DATA, 0, flatData, ptr, serializedDataSize);
            ptr += serializedDataSize;

            return flatData;
        }
    
        public override void Decode(byte[] data)
        {
            int ptr = 0;
            //Decoding object ID
            OBJECT_ID = new byte[SIZES.OBJECT_ID];
            OBJECT_ID[SIZES.OBJECT_ID - 2] = data[ptr++];
            OBJECT_ID[SIZES.OBJECT_ID - 1] = data[ptr++];

            //Decoding data size
            SERIALIZED_DATA_SIZE = new byte[SIZES.SERIALIZED_DATA_SIZE];
            SERIALIZED_DATA_SIZE[SIZES.SERIALIZED_DATA_SIZE - 2] = data[ptr++];
            SERIALIZED_DATA_SIZE[SIZES.SERIALIZED_DATA_SIZE - 1] = data[ptr++];

            SERIALIZED_DATA = new byte[serializedDataSize];
            Buffer.BlockCopy(data, ptr, SERIALIZED_DATA, 0, serializedDataSize);
        }
    
    }
}
