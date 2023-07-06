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
using Protocol;

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
                client.SendData(data);
                client2.SendData(data);

                client.CleanDisconnect();
                client2.CleanDisconnect();

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
                        clients[i].SendData(dataBuf);
                    }
                
                Console.WriteLine("Done sending data");

                //Disconnect clients
                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i].CleanDisconnect().Result == false)
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

    public static class SendingBytes
    {
        public static void Run()
        {
            //10ko data payload
            int messageLen = 65_495;
            
            Random random = new Random();
            TCPServer server = new TCPServer(4444, 10);
            
            TCPClient client = new TCPClient(Utils.GetLocalIPv4(NetworkInterfaceType.Wireless80211), 4444);
            TCPClient client2 = new TCPClient(Utils.GetLocalIPv4(NetworkInterfaceType.Wireless80211), 4444);
              
            server.StartServerThread();
            server.clientProcessor.OnClientDataReceivedEvent += ProcessBytes;

            //Initial object to send
            byte[] encodedData = { 0xD0, 0xD0, 0xD0, 0xD0 };
            
            SerializedObjectPacket objData = new SerializedObjectPacket(ObjectID.Test_Object, encodedData);
            byte[] data = objData.Encode();


            SerializedObjectPacket cpk = new SerializedObjectPacket();
            cpk.Decode(data);

            cpk.Decode(cpk.Encode());



            DataHandlerPacket packet = new DataHandlerPacket(ProtocolVersion.V1_0, AppID.NeoPlasma, DataType.SerializedObject, data, (UInt32)0x01020304);

            var x = packet.rptMs;
            var y = packet.eof;

            RunClientTask(client, packet.Encode());





        }

        public static void ProcessBytes(object sender, ClientDataReceivedEventArgs e)
        {
            byte[] data = e.data;
        
            DataHandlerPacket received = new DataHandlerPacket();
            received.Decode(data);

            
            Console.WriteLine("Task sent");
        }

        public static Task RunClientTask(TCPClient c, byte[] dataToSend)
        {
            return Task.Run(async () => 
            {
                await c.Connect();
                await c.SendData(dataToSend);              
                await c.CleanDisconnect();

            }); 
        }
    }

    public static class ProtocolTester
    {
        public static void Run()
        {
            byte[] data = { 0xA0, 0xA0, 0xA0, 0xA0 };
            DataHandlerPacket packet = new DataHandlerPacket(ProtocolVersion.V1_0, AppID.NeoPlasma, DataType.SerializedObject, data);
            
            
            byte[] encoded = packet.Encode();


           



        }
    }
    

}
