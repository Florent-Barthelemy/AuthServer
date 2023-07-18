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
        bool stopProcessingThread = false;
        public AutoRegisterArray<AutoRegSocket> autoRegSockets { get; private set; }

        public Thread processingThread { get; private set; }

        public event EventHandler<ClientDataReceivedEventArgs> OnClientDataReceivedEvent;


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

                //Handle processing thread stop

                //Handle processing thread stop
            }));

            processingThread.Start();
        }

        public Task StopProcessingThread()
        {
            return Task.Run(() =>
            {
                stopProcessingThread = true;
                while (processingThread.IsAlive) { }
            });
        }

        /// <summary>
        /// Cycles through all clients and reg/unreg accordingly
        /// </summary>
        private async void _CycleClientsProc()
        {
            while (!stopProcessingThread)
            {
                foreach (AutoRegSocket sock in autoRegSockets.array.AggregatedArray())
                {
                    if (sock.networkStream.DataAvailable)
                    {
                        var streamData = await ReadStream(sock, 65495);


                        //if socket has sent a FIN
                        if (streamData.Length == 0)
                        {
                            //Sending FIN ACK
                            sock.socket.Shutdown(SocketShutdown.Both);

                            //Release ressources
                            sock.socket.Close();

                            //Unregister from autoRegArray & further processing
                            sock.PreUnregister();
                        }
                        else
                        {
                            //Data can be processed
                            OnClientDataReceivedEvent?.Invoke(this, new ClientDataReceivedEventArgs()
                            {
                                autoRegSocket = sock,
                                data = streamData,
                                socket = sock.socket,
                                eventDate = DateTime.Now
                            });

                        }
                    }

                }

                autoRegSockets.array.DeleteAllMarked();
            }
        }

        /// <summary>
        /// Returns a variable-sized array of received bytes
        /// </summary>
        /// <param name="socket">Socket to read from</param>
        /// <returns>Bytes read, data</returns>
        public Task<byte[]> ReadStream(AutoRegSocket socket, int bufferSize)
        {
            return Task<byte[]>.Factory.StartNew(() =>
            {
               
                //Reading the AutoRegSocket stream...
                byte[] buffer = new byte[bufferSize];
                int bytesRead = socket.networkStream.Read(buffer, 0, buffer.Length);

                byte[] readData = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, readData, 0, bytesRead);

                socket.networkStream.Flush();

                //Return the retrieved data
                return readData;
                
            });
        }
    }

    public class ClientDataReceivedEventArgs : EventArgs
    {
        public AutoRegSocket autoRegSocket { get; set; }
        public byte[] data { get; set; }
        public Socket socket { get; set; }
        public DateTime eventDate { get; set; }
    }
}
