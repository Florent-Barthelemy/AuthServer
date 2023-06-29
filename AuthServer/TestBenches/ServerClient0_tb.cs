using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Client;
using System.Threading;

namespace AuthServer.TestBenches
{
    public static class ServerClient0_tb
    {
        public static void Run()
        {
            Console.WriteLine("============= Client With server open Test bench ===================");

            int testPort = 4444;

            var localIP = Utils.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            Console.WriteLine("using machine Wireless80211 local IP : " + localIP);
            Console.WriteLine("Starting a server thread...");
            
            TCPServer server = new TCPServer(testPort, 10);
            
            server.StartServerThread();

            Console.WriteLine("Server started on port " + testPort);
            Console.WriteLine("Client is connecting...");

            TCPClient client = new TCPClient(localIP, testPort);
            TCPClient client2 = new TCPClient(localIP, testPort);


            if (client.Connect().Result == true && client2.Connect().Result == true)
            {
                //If connection is good, as a client we send data then disconnect
                Console.WriteLine("Connection successful!");
                Random rand = new Random();
                byte[] data = new byte[100];
                rand.NextBytes(data);
                client.SendData(data, 5);
                client2.SendData(data, 5);

                client.ShutDownAndDisconnect();
                client2.ShutDownAndDisconnect();

                Console.WriteLine("Disconnected!");
            }
                
            else
            {
                Console.WriteLine("Oops something went wrong while connecting to the server...");
                if(client.TryGetLastException(out var lastException))
                {
                    Console.WriteLine("Got exception : " + lastException.Message);
                }
            }
        }
    }

    public static class ManyClients
    {
        public static void Run()
        {
            Console.WriteLine("============= Synchronous clients on server TB  ===================");



            int testPort = 4444;
            int totalClients = 30;
            int serverCapacity = 2000;
            int numberOfPasses = 10;
            int payloadSize = 10;

            var localIP = Utils.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            Console.WriteLine("using machine Wireless80211 local IP : " + localIP);
            Console.WriteLine("Starting a server thread...");
            TCPServer server = new TCPServer(testPort, serverCapacity);
            server.StartServerThread();

            Random rand = new Random();
            byte[] dataBuf = new byte[payloadSize]; 

            for(int k = 0; k < numberOfPasses; k++)
            {
                int x = k + 1;
                Console.WriteLine("Pass No : " + x);
                //Creating 200 tcp clients
                TCPClient[] clients = new TCPClient[totalClients];

                //Instanciate & connect clients one by one
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i] = new TCPClient(localIP, testPort);
                    if (clients[i].Connect().Result == false)
                    {
                        Console.WriteLine("Oops, something went wrong during connection of client : " + i);
                        if (clients[i].TryGetLastException(out Exception ex))
                            Console.WriteLine(ex.Message);

                        Console.ReadLine();
                    }
                }

                Console.WriteLine("Done connecting....");

                Console.WriteLine("Sending data...");

                for (int i = 0; i < clients.Length; i++)
                
                    if (clients[i].Connect().Result == false)
                    {
                        rand.NextBytes(dataBuf);
                        clients[i].SendData(dataBuf, 1);
                    }
                
                Console.WriteLine("Done sending data");

                //Disconnect clients
                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i].ShutDownAndDisconnect().Result == false)
                    {
                        Console.WriteLine("Oops, something went wrong during disconnection of client : " + i);
                        if (clients[i].TryGetLastException(out Exception ex))
                            Console.WriteLine(ex.Message);
                        Console.ReadLine();
                    }

                }

                Console.WriteLine("Done");
      
                
            }

            



        }
    }



}
