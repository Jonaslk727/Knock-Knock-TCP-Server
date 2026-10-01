
using System.Net;
using System.Net.Sockets;

namespace Knock_Knock_TCP_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using Knock_KnockServer server = new Knock_KnockServer();


            server.Start();

            server.Dispose();

            Console.WriteLine ("Press any key to exit...");
            Console.ReadKey ();
        }
    }
}
