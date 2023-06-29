using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.TestBenches;
using AuthServer.TestBenches.AutoRegistration;
using Server;


namespace AuthServer
{
    class AuthServerMain
    {
        static TCPServer server;
        

        static void Main(string[] args)
        {
            SenidngBytes.Run();


            Thread.CurrentThread.Join();
        }
    }
}
