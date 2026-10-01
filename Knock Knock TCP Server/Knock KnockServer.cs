using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Knock_Kncok_TCP_Client
{
    internal class Knock_KnockServer : IDisposable
    {
        private TcpListener _tcpListener =
            new TcpListener(IPAddress.Any, 5001);

        private TcpClient? _client;
        private NetworkStream? _networkStream;
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;

        public void Start()
        {
            StartListener();

            AcceptClient();

            SetupStreams();

            KnockKnock();
        }

        private void StartListener()
        {
            _tcpListener.Start();

            Console.WriteLine("Waiting for client...");
        }

        private void AcceptClient()
        {
            _client = _tcpListener.AcceptTcpClient();

            Console.WriteLine(
                $"Client connected: {_client.Client.RemoteEndPoint}");
        }

        private void SetupStreams()
        {
            _networkStream = _client!.GetStream();

            _streamReader = new StreamReader(_networkStream);

            _streamWriter = new StreamWriter(_networkStream)
            {
                AutoFlush = true
            };
        }

        private void KnockKnock()
        {
            ReadMessage();

            SendMessage("Who's there?");

            string? message = ReadMessage();

            SendMessage($"{message} who?");

            ReadMessage();

            SendMessage("Haha! That's a good one!");
        }

        private void SendMessage(string message)
        {
            Console.WriteLine($"Server: {message}");

            _streamWriter!.WriteLine(message);
        }

        private string? ReadMessage()
        {
            string? message = _streamReader!.ReadLine();

            Console.WriteLine($"Client: {message}");

            return message;
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _streamWriter?.Dispose();
            _networkStream?.Dispose();
            _client?.Dispose();
            _tcpListener.Stop();
        }
    }
}
