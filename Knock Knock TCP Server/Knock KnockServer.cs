using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Knock_Knock_TCP_Server
{
    /// <summary>
    /// Multithreaded Knock Knock TCP Server.
    /// The server listens for incoming client connections on a configured port.
    /// For each new connection, a ClientHandler is created and run in a dedicated thread.
    /// The main thread continues listening for new connections.
    /// </summary>
    internal class Knock_KnockServer
    {
        private TcpListener? _tcpListener;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ServerOptions> _serverOptions;
        private readonly IJokeSelector _jokeSelector;
        private List<Thread> _clientThreads = new List<Thread>();
        private bool _isRunning = false;

        public Knock_KnockServer(
            IServiceProvider serviceProvider,
            IOptions<ServerOptions> serverOptions,
            IJokeSelector jokeSelector)
        {
            _serviceProvider = serviceProvider;
            _serverOptions = serverOptions;
            _jokeSelector = jokeSelector;
        }

        /// <summary>
        /// Main server loop: listen for incoming connections and spawn client handler threads.
        /// </summary>
        public void Start()
        {
            _isRunning = true;

            try
            {
                StartListener();
                AcceptConnections();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        private void StartListener()
        {
            var port = _serverOptions.Value.Port;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();

            Console.WriteLine($"Server started on port {port}. Waiting for client connections...");
            Console.WriteLine($"Joke selection strategy: {_jokeSelector.StrategyName}");
        }

        private void AcceptConnections()
        {
            while (_isRunning)
            {
                try
                {
                    // This blocks until a client connects
                    TcpClient client = _tcpListener!.AcceptTcpClient();

                    // Create a handler for this client
                    var handler = new ClientHandler(
                        client,
                        _serviceProvider,
                        _serviceProvider.GetRequiredService<IOptions<JokesConfiguration>>(),
                        _jokeSelector);

                    // Spawn a new thread to handle this client
                    var clientThread = new Thread(() => handler.Run())
                    {
                        IsBackground = true,
                        Name = $"ClientHandler-{client.Client.RemoteEndPoint}"
                    };

                    lock (_clientThreads)
                    {
                        _clientThreads.Add(clientThread);
                    }

                    clientThread.Start();
                }
                catch (SocketException ex) when (!_isRunning)
                {
                    // Expected when listener is stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client connection: {ex.Message}");
                }
            }
        }

        private void Stop()
        {
            _isRunning = false;

            try
            {
                _tcpListener?.Stop();
                Console.WriteLine("Listener stopped.");

                // Optionally wait for client threads to finish (with timeout)
                Console.WriteLine("Waiting for client connections to close...");
                lock (_clientThreads)
                {
                    foreach (var thread in _clientThreads)
                    {
                        if (thread.IsAlive && !thread.Join(TimeSpan.FromSeconds(2)))
                        {
                            Console.WriteLine($"Timeout waiting for thread {thread.Name}");
                        }
                    }
                }

                Console.WriteLine("All client connections closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shutdown: {ex.Message}");
            }
        }
    }
}

