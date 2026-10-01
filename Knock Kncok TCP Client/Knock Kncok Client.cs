using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Knock_Kncok_TCP_Client
{
    internal class Knock_Kncok_Client : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private int _port = 5001;

        public string ServerAddress { get; set; } = "Localhost";

        public void Start()
        {
            Connect();

            KnockKnock();

            Dispose();
        }

        private void Connect()
        {
            _client = new TcpClient(ServerAddress, _port);

            Console.WriteLine("Connected to server.");

            _networkStream = _client.GetStream();

            _streamReader = new StreamReader(_networkStream);

            _streamWriter = new StreamWriter(_networkStream)
            {
                AutoFlush = true
            };
        }

        private void KnockKnock()
        {
            SendMessage("Knock knock!");

            ReadMessage();

            SendMessage("Lettuce");

            ReadMessage();

            SendMessage("Lettuce in, it's cold out here!");

            ReadMessage();
        }

        private void SendMessage(string message)
        {
            Console.WriteLine($"Client: {message}");

            _streamWriter.WriteLine(message);
        }

        private string? ReadMessage()
        {
            string? message = _streamReader.ReadLine();

            Console.WriteLine($"Server: {message}");

            return message;
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _streamWriter?.Dispose();
            _networkStream?.Dispose();
            _client?.Dispose();
        }
    }
}
