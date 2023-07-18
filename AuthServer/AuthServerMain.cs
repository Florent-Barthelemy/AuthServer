using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            ArgManager.RegisterArg("--pipe", 0, PipeMode, " Forwards sent data from a client to all other clients, UDP style", "-f");
            ArgManager.RegisterArg("--testBench", 1, RunTbCallback, " <name> Runs a preprogrammed test bench for development uses\n [DATAPIPE] tests the data forward functionality\n [CLIENT_<A/B>] Creates a client for the DATAPIPE", "-tb");

            //Parse command line arguments
            //Maybe tryc this and log the error in a file to avoid user frightening 
            ArgManager.ParseArgs(args);

            return 0;
        }


        public static void PipeMode(string[] args)
        {
            Console.WriteLine("[>] Server is in pipe mode");
            Pipe pipe = new Pipe(server);
            server.clientProcessor.OnClientDataReceivedEvent += pipe.PipeData;
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
                server = new TCPServer((int)serverPort, 100);
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

                //Start client C
                string commandClientC = "./AuthServer.exe -tb CLIENT_C";
                Process C = System.Diagnostics.Process.Start("powershell.exe", commandClientC);

                //Start client D
                string commandClientD = "./AuthServer.exe -tb CLIENT_D";
                Process D = System.Diagnostics.Process.Start("powershell.exe", commandClientD);



                Thread.CurrentThread.Join();
               
            }
            else if (args[0] == "CLIENT_A") { DoTbClient("A"); }
            else if (args[0] == "CLIENT_B") { DoTbClient("B"); }
            else if (args[0] == "CLIENT_C") { DoTbClient("C"); }
            else if (args[0] == "CLIENT_D") { DoTbClient("D"); }

        }

        static void DoTbClient(string name)
        {
            string remoteIP = Utils.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211);
            TCPClient client = new TCPClient(remoteIP, (int)serverPort);
            Console.WriteLine("Created client " + name + ", connecting on " + remoteIP + ":4444");
            client.ConnectSync();

            Thread.Sleep(1000);

            if (client.TryGetLastException(out var lastException)) { Console.WriteLine("Error occurred! : " + lastException.Message); Console.ReadLine(); }
            Console.WriteLine("Connected !");
            client.StartIOThreads();

            client.OnDataReceivedEvent += ClientA_DataReceived;

            for (int i = 0; i < 2; i++)
            {

                Task send = Task.Run(() => { client.SendData(Encoding.UTF8.GetBytes("Hello from " + name + " !")); });
                send.Wait();
                Thread.Sleep(500);
            }

            Thread.CurrentThread.Join();
        }

        private static void ClientA_DataReceived(object sender, byte[] e)
        {
            Console.WriteLine("Received " + e.Length + " bytes from server : " + Encoding.UTF8.GetString(e));
        }

        private static void TestBench_OnDataReceived(object sender, ClientDataReceivedEventArgs e)
        {
            Console.WriteLine("Received " + e.data.Length + " bytes from : " + e.autoRegSocket.UID.ToString() );
            foreach(AutoRegSocket s in server.GetClients())
            {
                //If the client is not the sender, forward the data to them.
                if(s.UID != e.autoRegSocket.UID)
                {
                    s.socket.Send(e.data);
                    Console.WriteLine(e.data.Length + " Bytes forwarded to --> " + s.UID.ToString());
                }
            }
        }

        private static void TestBench_OnNewClientConnected(object sender, NewClientConnectedEventArgs e)
        {
            Console.WriteLine("New client connected to the server ! UID : " + e.autoSock.UID.ToString());
        }
    }
}
