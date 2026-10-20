using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Knock_Kncok_TCP_Client.Configuration;
using Microsoft.Extensions.Options;

namespace Knock_Kncok_TCP_Client
{
    internal class Knock_Kncok_Client : IDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _networkStream;
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;

        private readonly string _serverAddress;
        private readonly int _port;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        public Knock_Kncok_Client(IOptions<ClientOptions> options)
        {
            var clientOptions = options.Value;
            _serverAddress = clientOptions.Connection.ServerAddress;
            _port = clientOptions.Connection.Port;
            _maxRetries = clientOptions.Connection.MaxRetries;
            _retryDelayMs = clientOptions.Connection.RetryDelayMs;
        }

        public void Start()
        {
            Connect();

            JokeLoop();

            Dispose();
        }

        private void Connect()
        {
            int currentRetry = 0;
            int currentDelayMs = _retryDelayMs;

            while (currentRetry < _maxRetries)
            {
                try
                {
                    Console.WriteLine($"Attempting to connect to {_serverAddress}:{_port} (Attempt {currentRetry + 1}/{_maxRetries})...");

                    _client = new TcpClient(_serverAddress, _port);

                    Console.WriteLine("Connected to server successfully!");

                    _networkStream = _client.GetStream();

                    _streamReader = new StreamReader(_networkStream);

                    _streamWriter = new StreamWriter(_networkStream)
                    {
                        AutoFlush = true
                    };

                    return; // Connection successful, exit the retry loop
                }
                catch (SocketException ex)
                {
                    currentRetry++;
                    if (currentRetry < _maxRetries)
                    {
                        Console.WriteLine($"Connection refused. Retrying in {currentDelayMs}ms... ({currentRetry}/{_maxRetries})");
                        Thread.Sleep(currentDelayMs);
                        currentDelayMs = Math.Min(currentDelayMs * 2, 2000); // Exponential backoff, max 2 seconds
                    }
                    else
                    {
                        Console.WriteLine($"Failed to connect after {_maxRetries} attempts.");
                        throw new InvalidOperationException($"Could not connect to server at {_serverAddress}:{_port} after {_maxRetries} retries.", ex);
                    }
                }
            }
        }

        private void JokeLoop()
        {
            bool wantMoreJokes = true;

            while (wantMoreJokes)
            {
                try
                {
                    // Request a joke
                    Console.WriteLine ("\n--- Requesting a joke from server ---");
                    SendMessage ("joke");

                   
                    ReadMessage ();

                    
                    SendMessage ("Who's there?");

                    // Receive 
                    ReadMessage ();

                    // Send a punchline attempt
                    Console.WriteLine ("Enter your answer to the punchline:");
                    string? userAnswer = Console.ReadLine ();
                    SendMessage (userAnswer ?? "I don't know");

                    // Receive server's punchline
                    ReadMessage ();

                    // Server asks if client wants another joke
                    string? response = ReadMessage ();

                    // Send yes or no
                    if (response?.Contains ("another joke") == true)
                    {
                        Console.WriteLine ("Do you want another joke? (yes/no)");
                        string? userChoice = Console.ReadLine ();
                        SendMessage (userChoice ?? "no");

                        if (userChoice?.ToLower () != "yes")
                        {
                            wantMoreJokes = false;
                        }
                    }
                    else
                    {
                        wantMoreJokes = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine ($"Error: {ex.Message}");
                    wantMoreJokes = false;
                }
            }

            Console.WriteLine ("\nThank you for playing!");
        }

        private void SendMessage(string message)
        {
            Console.WriteLine($"Client: {message}");

            _streamWriter?.WriteLine(message);
        }

        private string? ReadMessage()
        {
            string? message = _streamReader?.ReadLine();

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
