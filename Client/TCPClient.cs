using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class TCPClient
    {
        /// <summary>
        /// The socket used to connect to the server
        /// </summary>
        public Socket client { get; private set; }
        public IPEndPoint remoteEndPoint { get; private set; }

        private Exception lastException;
        bool lastExceptionNull = true;


        public TCPClient(string remoteIP, int remotePort)
        {
            //Setting up the TCP socket for the server connection
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        /// <summary>
        /// Tries to disconnect from the remote endpoint, if any exception occurs
        /// the task returns false and updates the lastException field
        /// </summary>
        /// <returns></returns>
        public Task<bool> ShutDownAndDisconnect()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(true);
                    return true;
                }
                catch(Exception ex)
                {
                     saveLastException(ex);
                     return false;
                }
            });
        }

        public void SendData(byte[] data, int nPackets)
        {
            for (int i = 0; i < nPackets ; i++)
            {
                client.Send(data);
            }
        }

        /// <summary>
        /// Tries to connect from the remote endpoint, if any exception occurs
        /// the task returns false and updates the lastException field
        /// </summary>
        /// <returns></returns>
        public Task<bool> Connect()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    //Synchronously tries to connect to the server
                    client.Connect(remoteEndPoint);
                    return true;
                }
                catch (Exception ex)
                {
                    saveLastException(ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Tries to get the last occurred exception on the socket
        /// </summary>
        /// <param name="ex">Output exception</param>
        /// <returns></returns>
        public bool TryGetLastException(out Exception ex)
        {
            if(!lastExceptionNull)
            {
                ex = lastException;
                return true;
            }

            ex = null;
            return false;
        }

        /// <summary>
        /// Saves an exception on the lastException variable
        /// </summary>
        /// <param name="ex"></param>
        private void saveLastException(Exception ex)
        {
            lastException = ex;
            lastExceptionNull = false;
        }
    }
}
