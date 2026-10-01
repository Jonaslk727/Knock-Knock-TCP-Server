using System.Net.Sockets;

namespace Knock_Kncok_TCP_Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using Knock_Kncok_Client client = new Knock_Kncok_Client();

            client.Start();

            Console.WriteLine ("Press any key to exit...");
            Console.ReadKey ();
        }
    }
}
