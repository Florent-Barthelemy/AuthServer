using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.TestBenches;
using AuthServer.TestBenches.AutoRegistration;
using Client;
using Protocol;
using Server;


namespace AuthServer
{
    class AuthServerMain
    {
        /// <summary>
        /// The server instance used to create clients
        /// </summary>
        static TCPServer server;

        static UInt32 serverPort = 4444;
        static UInt32 serverMaxClients = 10;
        

        static int Main(string[] args)
        {
            //If no args given exit the program
            if(args.Length < 1)
            {
                Console.WriteLine("No command line arguments given, exiting\nTry --help to get a list of commands with their necessary arguments");
                return 0;
            }

            //Registering CLI args
            ArgManager.RegisterArg("--help", 0, ShowHelpCallback, " Shows this help message", "-h");
            ArgManager.RegisterArg("--start", 0, StartServerCallback, " Runs the server", "-s");
            ArgManager.RegisterArg("--maxClients", 0, StartServerCallback, " Sets the maximum clients allowed, default is 10", "-m");
            ArgManager.RegisterArg("--port", 1, SetPortCallback, " <Port> : Sets the server port. if port is not specified, 4444 is used by default\n                            use -p before -s !", "-p");
            ArgManager.RegisterArg("--testBench", 1, RunTbCallback, " <name> Runs a preprogrammed test bench for development uses\n [DATAPIPE] tests the data forward functionality\n [CLIENT_<A/B>] Creates a client for the DATAPIPE", "-tb");
            
            
            //ArgManager.RegisterArg("--clientMode", 0, <CALLBACK>, " Runs as a client and tries to connect to a port", "client");

            //Parse command line arguments
            //Maybe tryc this and log the error in a file to avoid user frightening 
            ArgManager.ParseArgs(args);

            

            return 0;
        }


        /// <summary>
        /// Sets the server port variable
        /// </summary>
        /// <param name="args"></param>
        public static void SetPortCallback(string[] args)
        {
            serverPort = UInt32.Parse(args[0]);
        }

        /// <summary>
        /// Sets the maximum number of clients that can connect to the server
        /// </summary>
        /// <param name="args"></param>
        public static void SetMaxClientsCallback(string[] args)
        {
            serverMaxClients = UInt32.Parse(args[0]);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="args"></param>
        public static void StartServerCallback(string[] args)
        {
            Console.WriteLine("Starting server on port : {0}...", serverPort);

            //Creating a new server and start it
            server = new TCPServer((int)serverPort, (int)serverMaxClients);
            server.StartServerThread();
            

            while (!server.listeningSocket.IsBound) { }
            Console.WriteLine("Socket is bound, listening...");
            

            //server.StopServerThread();
            
        }

        /// <summary>
        /// Help menu handler
        /// </summary>
        /// <param name="args"></param>
        public static void ShowHelpCallback(string[] args)
        {
            Console.WriteLine(ArgManager.GetHelpMenu());
        }

        /// <summary>
        /// Runs test benches
        /// </summary>
        /// <param name="args"></param>
        public static void RunTbCallback(string[] args)
        {
            if (args[0] == "DATAPIPE")
            {
                Console.WriteLine("Running DATAPIPE tb...");
                
                //Start server
                server = new TCPServer((int)serverPort, 2);
                server.StartServerThread();

                server.OnNewClientConnectedEvent += TestBench_OnNewClientConnected;
                server.clientProcessor.OnClientDataReceivedEvent += TestBench_OnDataReceived;


                Console.WriteLine("Server started");

                //Start client A
                string commandClientA = "./AuthServer.exe -tb CLIENT_A";
                Process A = System.Diagnostics.Process.Start("powershell.exe", commandClientA);
                
                //Start client B
                string commandClientB = "./AuthServer.exe -tb CLIENT_B";
                Process B = System.Diagnostics.Process.Start("powershell.exe", commandClientB);

                Thread.CurrentThread.Join();
               
            }
            else if (args[0] == "CLIENT_A")
            {
                string remoteIP = Utils.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211);
                TCPClient client = new TCPClient(remoteIP, (int)serverPort);
                Console.WriteLine("Created client A, connecting on " + remoteIP + ":4444");
                client.Connect();
                byte[] data = { 0xA0, 0xB0, 0xC0, 0xD0 };
                for(int i = 0; i < 100; i++)
                {
                    Console.WriteLine("Sending 0xA0B0D0C0 and wait 1s...");

                    DataHandlerPacket packet = new DataHandlerPacket(ProtocolVersion.V1_0, AppID.NeoPlasma, DataType.ClientAuthentication, data);

                    Task send = Task.Run(() => { client.SendData(packet.Encode()); });
                    send.Wait();
                    
                    
                    Thread.Sleep(1000);
                }
            }
            else if (args[0] == "CLIENT_B")
            {
                string remoteIP = Utils.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211);
                TCPClient client = new TCPClient(remoteIP, (int)serverPort);
                Console.WriteLine("Created client B, connecting on " + remoteIP + ":4444");
                client.Connect();

                
                Thread.CurrentThread.Join();

            }

        }

        private static void TestBench_OnDataReceived(object sender, ClientDataReceivedEventArgs e)
        {
            Console.WriteLine("Received " + e.data.Length + "bytes of data@" + e.eventDate.ToShortTimeString() + ":" );//+ Encoding.UTF8.GetString(e.data));
        }

        private static void TestBench_OnNewClientConnected(object sender, NewClientConnectedEventArgs e)
        {
            Console.WriteLine("New client connected to the server !");
        }
    }
}
