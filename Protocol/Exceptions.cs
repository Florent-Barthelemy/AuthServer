using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    /// <summary>
    /// Exception when a bad EOF has been received
    /// </summary>
    public class BadEofException : Exception
    {
        public ushort ReceivedEOF;
        public BadEofException(ushort receivedEOF) : base() { ReceivedEOF = receivedEOF;  }
        public BadEofException(ushort receivedEOF, string message ) : base(message) { ReceivedEOF = receivedEOF; }
    }

    /// <summary>
    /// Exception when a bad SOF has been received
    /// </summary>
    public class BadSofException : Exception
    {
        public ushort ReceivedSOF;
        public BadSofException(ushort receivedSOF) : base() { ReceivedSOF = receivedSOF; }
        public BadSofException(ushort receivedSOF, string message) : base(message) { ReceivedSOF = receivedSOF; }
    }


}
