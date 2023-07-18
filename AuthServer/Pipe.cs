using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer
{

    /// <summary>
    /// Pipe is used to forward data sent from a client to the others
    /// UDP style
    /// </summary>
    internal class Pipe
    {
        TCPServer tcpServer;
        public Pipe(TCPServer server) 
        {
            tcpServer = server;
        }

        public void PipeData(object sender, ClientDataReceivedEventArgs e)
        {
            Console.WriteLine("Received " + e.data.Length + " bytes from : " + e.autoRegSocket.UID.ToString());
            foreach (AutoRegSocket s in tcpServer.GetClients())
            {
                //If the client is not the sender, forward the data to them.
                if (s.UID != e.autoRegSocket.UID)
                {
                    s.socket.Send(e.data);
                    Console.WriteLine(e.data.Length + " Bytes forwarded to --> " + s.UID.ToString());
                }
            }
        }
    }
}
