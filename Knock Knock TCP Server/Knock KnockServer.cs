using Knock_Knock_TCP_Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Knock_Knock_TCP_Server.States;

namespace Knock_Knock_TCP_Server
{
    internal class Knock_KnockServer : IDisposable
    {
        private TcpListener _tcpListener =
            new TcpListener(IPAddress.Any, 5001);

        private TcpClient? _client;
        private NetworkStream? _networkStream;
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;

        private IKnockKnockState _currentState = new WaitingForKnockKnockState ();

        public void Start()
        {
            StartListener();

            AcceptClient();

            SetupStreams();

            RunConversation();
        }

        private void RunConversation()
        {
            while (true)
            {
                _currentState.Handle (this);

                if (_currentState is FinishedState)
                {
                    _currentState.Handle (this);
                    break; // Exit the loop if the current state is FinishedState
                }
            }

        }

        public void SetState(IKnockKnockState state)
        {
            _currentState = state;
        }

        private void StartListener()
        {
            _tcpListener.Start();

            Console.WriteLine("Waiting for client...");
        }

        private void AcceptClient()
        {
            _client = _tcpListener.AcceptTcpClient();

            Console.WriteLine($"Client connected: {_client.Client.RemoteEndPoint}");
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

        //private void KnockKnock()
        //{
        //    ReadMessage();

        //    SendMessage("Who's there?");

        //    string? message = ReadMessage();

        //    SendMessage($"{message} who?");

        //    ReadMessage();

        //    SendMessage("Haha! That's a good one!");
        //}

        public void SendMessage(string message)
        {
            Console.WriteLine($"Server: {message}");

            _streamWriter!.WriteLine(message);
        }

        public string? ReadMessage()
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
