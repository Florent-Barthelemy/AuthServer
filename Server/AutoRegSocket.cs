using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    /// <summary>
    /// Defines a socket object that can bind to an autoReg array
    /// </summary>
    public class AutoRegSocket : AutoRegisterableElement<AutoRegSocket>
    {
        /// <summary>
        /// The base socket
        /// </summary>
        public Socket socket { get; private set; }
        public NetworkStream networkStream { get; private set; }

        public StreamBlock dataBlock { get; private set; }

        public AutoRegSocket(Socket baseSocket)
        {
            socket = baseSocket;
            networkStream = new NetworkStream(socket);
            dataBlock = new StreamBlock();
        }

        protected override OnRegisterEventArgs<AutoRegSocket> __BuildRegisterArgs()
        {
            return new OnRegisterEventArgs<AutoRegSocket>()
            {
                eventDate = DateTime.Now,
                objToRegister = this
            };
        }
    }
}
