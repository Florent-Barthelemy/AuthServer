using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class TCPServer
    {
        /// <summary>
        /// The socket used to listen for connections
        /// </summary>
        public Socket listeningSocket { get; private set; }
        
        /// <summary>
        /// The server port
        /// </summary>
        public int serverPort { get; private set; }
        
        /// <summary>
        /// Local ip endpoint
        /// </summary>
        private IPEndPoint localEndpoint { get; set; }

        /// <summary>
        /// The server thread
        /// </summary>
        public Thread serverThread { get; private set; }

        /// <summary>
        /// Signals the server to stop
        /// </summary>
        private bool stopServer = false;

        /// <summary>
        /// Client timeout value in ms
        /// </summary>
        public int timeoutValue;

        public ClientProcessor clientProcessor { get; private set; }

        public TCPServer(int port, int maxConnections)
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            localEndpoint = new IPEndPoint(IPAddress.Any, port);
            listeningSocket.Bind(localEndpoint);

            clientProcessor = new ClientProcessor(maxConnections);

        }

        /// <summary>
        /// Starts the server in a new thread
        /// </summary>
        public void StartServerThread()
        {
            //Start by activating the processor thread
            clientProcessor.StartProcessingThread();

            serverThread = new Thread(new ThreadStart(() =>
            {
                _ServerLoop();
                
                //Code to handle server stop

                //Code to handle server stop
            }));

            serverThread.Start();
        }

        /// <summary>
        /// Stops the server loop, returns when Thread is not alive
        /// Method is await able
        /// </summary>
        public Task StopServerThread() 
        {
            return Task.Run(() =>
            {
                stopServer = true;
                clientProcessor.StopProcessingThread();
                while (serverThread.IsAlive) { }
            });
        }

        /// <summary>
        /// Triggered when a new client has connected to the server
        /// </summary>
        public event EventHandler<NewClientConnectedEventArgs> OnNewClientConnectedEvent;
        private async void _ServerLoop()
        {
            listeningSocket.Listen(100);
            
            while(!stopServer)
            {
                Socket newConnection = await listeningSocket.AcceptAsync();
                newConnection.ReceiveTimeout = timeoutValue;

                //Create an Auto Registerable socket, bind it and register
                AutoRegSocket aSocket = new AutoRegSocket(newConnection);

                //Bind the socket to the processor list
                aSocket.Bind(clientProcessor.autoRegSockets);

                //Register the socket
                aSocket.Register();

                OnNewClientConnectedEvent?.Invoke(this, new NewClientConnectedEventArgs()
                {
                    socket = newConnection,
                    eventDate = DateTime.Now
                });
            }
        }
     
    }


    public class NewClientConnectedEventArgs : EventArgs
    {
        public Socket socket {get; set;}
        public DateTime eventDate { get; set; }
    }

}
