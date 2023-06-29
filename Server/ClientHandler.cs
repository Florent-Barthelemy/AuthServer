using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ClientHandler
    {
        public Thread handlingThread;
        
        private Socket[] clients;
        private int[] allocationMap;

        private bool stopHandlingThread = false;
        
        
        public ClientHandler(int maxClients)
        {
            clients = new Socket[maxClients];
        }

        public int GetClientAllocator()
        {



            return 0;
        }


        /// <summary>
        /// Starts client handling
        /// </summary>
        public void StartHandlingThread()
        {
            handlingThread = new Thread(new ThreadStart(() =>{
                _HandleClients();
            }));

            handlingThread.Start();
        }

        /// <summary>
        /// Stops the handling thread, await able method
        /// </summary>
        /// <returns></returns>
        public Task StopHandlingThread()
        {
            return Task.Run(() =>
            {
                stopHandlingThread = true;
                while (handlingThread.IsAlive) { }
            });
        }

        private void _HandleClients()
        {
            while(!stopHandlingThread)
            {
                //Mutex for variable access between events and threads
                lock(clients)
                {
                    foreach(Socket s in clients)
                    {
                        
                    }
                }
            }
        }


        /// <summary>
        /// Handles a client to the handle list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddClientToHandleList(object sender, NewClientConnectedEventArgs e)
        {
            
        }

    }
}
