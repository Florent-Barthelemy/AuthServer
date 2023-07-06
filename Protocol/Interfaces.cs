using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{

    /// <summary>
    /// Encapsulates methods for a packet
    /// </summary>
    public abstract class PacketBase
    {
        /// <summary>
        /// Encodes the data structure to a byte array
        /// </summary>
        /// <returns>Encoded byte array </returns>
        public abstract byte[] Encode();

        /// <summary>
        /// Decodes the encoded data to a new packet structure
        /// </summary>
        /// <param name="data">The encoded data</param>
        /// <returns>A packet struct</returns>
        public abstract void Decode(byte[] data);
    }
}
