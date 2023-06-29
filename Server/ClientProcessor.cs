using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Processes a AutoRegisterArray<Sockets> that represents an array of connections
    /// </summary>
    public class ClientProcessor
    {
        public AutoRegisterArray<AutoRegSocket> autoRegSockets { get; private set; }

        public Thread processingThread { get; private set; }


        /// <summary>
        /// Creates a new instance of the client processor
        /// </summary>
        /// <param name="maxClients">Maximum number of clients</param>
        public ClientProcessor(int maxClients)
        {
            autoRegSockets = new AutoRegisterArray<AutoRegSocket>(maxClients, AllocationPolicy.firstFreeElement);
        }


        public void StartProcessingThread()
        {
            processingThread = new Thread(new ThreadStart(() =>
            {
                _CycleClientsProc();
            }));

            processingThread.Start();
        }

        /// <summary>
        /// Cycles through all clients and reg/unreg accordingly
        /// </summary>
        private async void _CycleClientsProc()
        {
            while (true)
            {
                foreach (AutoRegSocket sock in autoRegSockets.array.AggregatedArray())
                {
                    var streamData = await ReadStream(sock, 1024);

                    if (streamData.Item1 == 0)
                        CloseAndUnregister(sock);
                        
                }

                autoRegSockets.array.DeleteAllMarked();
            }
        }

        /// <summary>
        /// Returns a variable-sized array of received bytes
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public Task<Tuple<int, byte[]>> ReadStream(AutoRegSocket socket, int bufferSize)
        {
            return Task<Tuple<int, byte[]>>.Factory.StartNew(() =>
            {
                //Reading the AutoRegSocket stream...
                byte[] buffer = new byte[bufferSize];
                int bytesRead = socket.networkStream.Read(buffer, 0, buffer.Length);

                socket.networkStream.Flush();

                //Return the retrieved data
                return Tuple.Create(bytesRead, buffer);
                
            });
        }

        private void CloseAndUnregister(AutoRegSocket socket)
        {
            //Sending FIN
            socket.socket.Shutdown(SocketShutdown.Both);
            
            //Release ressources
            socket.socket.Close();

            //Unregister from autoRegArray & further processing
            socket.PreUnregister();
        }

        [Obsolete("Method is obsolete", true)]
        public void StartSocketHandler(Socket socket)
        {
            Task.Run(() =>
            {
                NetworkStream ns = new NetworkStream(socket);
                byte[] buffer = new byte[4096];
                int readc = 1;
                while (readc != 0)
                {
                    readc = ns.Read(buffer, 0, buffer.Length);
                    Console.WriteLine("Received : " + readc);
                }

                Console.WriteLine("Client Sent disconnect");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            });
        }
    }
}
