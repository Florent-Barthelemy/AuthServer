using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
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

        /// <summary>
        /// Representation of the remote endpoint
        /// </summary>
        public IPEndPoint remoteEndPoint { get; private set; }

        /// <summary>
        /// Last exception thrown
        /// </summary>
        private Exception lastException;

        /// <summary>
        /// Either or not the last exception is null
        /// </summary>
        bool lastExceptionNull = true;

        /// <summary>
        /// Controls the I/O Thread life
        /// </summary>
        private bool doIOThread = false;

        /// <summary>
        /// The Long-Running Send Thread
        /// </summary>
        public Thread IOThread_send { get; private set; }

        /// <summary>
        /// The Long-Running Receive Thread
        /// </summary>
        public Thread IOThread_receive { get; private set; }

        /// <summary>
        /// The size of the RX buffer
        /// </summary>
        public int receiveBufferLength { get; private set; }

        /// <summary>
        /// The RX buffer
        /// </summary>
        byte[] receiveBuffer;

        /// <summary>
        /// TX FIFO
        /// </summary>
        public Queue<byte[]> dataToSendQueue { get; private set; }
        private ManualResetEvent dataEnqueuedEvent = new ManualResetEvent(false);

        /// <summary>
        /// When data have been received from the remote computer
        /// </summary>
        public event EventHandler<byte[]> OnDataReceivedEvent;

        /// <summary>
        /// The established network stream between remote and client
        /// </summary>
        public NetworkStream netStream { get; private set; }

        public TCPClient(string remoteIP, int remotePort, int receiveBufferLength = 1024)
        {
            //Creating the TX FIFO
            dataToSendQueue = new Queue<byte[]>();

            //Creating a receive buffer
            this.receiveBufferLength = receiveBufferLength;
            receiveBuffer = new byte[receiveBufferLength];

            //Setting up the TCP socket for the server connection
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        /// <summary>
        /// Tries to disconnect from the remote endpoint, if any exception occurs
        /// the task returns false and updates the lastException field
        /// </summary>
        /// <returns></returns>
        public Task<bool> CleanDisconnect()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(true);
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
        /// Sends data to the remote host
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns>A Task</returns>
        public Task SendData(byte[] data)
        {
            return Task.Factory.StartNew(async () =>
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(data);
                try
                {
                    _ = await client.SendAsync(buffer, SocketFlags.None);
                    return;
                }
                catch (Exception ex)
                {
                    saveLastException(ex);
                    return;
                }

            });
        }

        /// <summary>
        /// Enqueues data to send to the distant server
        /// </summary>
        /// <param name="data"></param>
        public void EnqueueDataToSend(byte[] data)
        {
            lock (dataToSendQueue)
            {
                dataToSendQueue.Enqueue(data);
            }

            dataEnqueuedEvent.Set();
        }

        /// <summary>
        /// Starts an I/O Thread, periodically send and receive data when the concerned buffers
        /// are available
        /// </summary>
        public void StartIOThreads()
        {
            doIOThread = true;

            IOThread_send = new Thread(new ThreadStart(() =>
            {
                while (doIOThread)
                {
                    dataEnqueuedEvent.WaitOne();

                    //If there is available data, dequeue it in a thread-safe manner
                    for (int i = 0; i < dataToSendQueue.Count; i++)
                    {
                        byte[] data;
                        lock (dataToSendQueue) { data = dataToSendQueue.Dequeue(); }
                        SendData(data);
                    }

                    dataEnqueuedEvent.Reset();
                }
            }));

            IOThread_receive = new Thread(new ThreadStart(async () =>
            {
               
                while(doIOThread)
                {
                    int bytesRead = client.Receive(receiveBuffer);
                    byte[] data = new byte[bytesRead];
                    Buffer.BlockCopy(receiveBuffer, 0, data, 0, bytesRead);
                    OnDataReceivedEvent?.Invoke(this, data);
                }
            }));

            IOThread_receive.Start();
            IOThread_send.Start();
        }

        /// <summary>
        /// Stops the IO Threads and wait for them to end
        /// </summary>
        /// <returns>await-able Task</returns>
        public Task StopIOThreads()
        {
            return Task.Factory.StartNew(() =>
            {
                doIOThread = false;

                //Block the tasks while I/O thread are alive
                while(IOThread_receive.IsAlive || IOThread_send.IsAlive) { }
                return;
            });
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

        public bool ConnectSync()
        {

            try
            {
                //Synchronously tries to connect to the server
                client.Connect(remoteEndPoint);
                //client.Listen(1);
                netStream = new NetworkStream(client);
                return true;
            }
            catch (Exception ex)
            {
                saveLastException(ex);
                return false;
            }

        }

        /// <summary>
        /// Tries to get the last occurred exception on the socket
        /// </summary>
        /// <param name="ex">Output exception</param>
        /// <returns></returns>
        public bool TryGetLastException(out Exception ex)
        {
            if (!lastExceptionNull)
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