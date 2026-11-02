using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text;

namespace Knock_Knock_TCP_Server
{
    /// <summary>
    /// Encapsulates a single client's complete conversation lifecycle.
    /// Each ClientHandler runs in its own thread and manages:
    /// - TCP communication (reading/writing)
    /// - Conversation state machine
    /// - Resource cleanup
    /// 
    /// This class implements IConversationContext to provide state objects
    /// with easy access to I/O and conversation metadata without exposing
    /// the full Knock_KnockServer instance.
    /// </summary>
    internal class ClientHandler : IConversationContext
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly StreamReader _streamReader;
        private readonly StreamWriter _streamWriter;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<JokesConfiguration> _jokesConfiguration;
        private readonly IJokeSelector _jokeSelector;
        private readonly string _clientIdentifier;

        private JokeModel? _currentJoke;
        private bool _isConnected = true;

        public bool IsConnected => _isConnected;

        public ClientHandler(
            TcpClient tcpClient,
            IServiceProvider serviceProvider,
            IOptions<JokesConfiguration> jokesConfiguration,
            IJokeSelector jokeSelector)
        {
            _tcpClient = tcpClient;
            _networkStream = tcpClient.GetStream();
            // Use UTF8 without BOM to avoid extra bytes at the start
            _streamReader = new StreamReader(_networkStream, new UTF8Encoding(false));
            _streamWriter = new StreamWriter(_networkStream, new UTF8Encoding(false)) { AutoFlush = true };
            _serviceProvider = serviceProvider;
            _jokesConfiguration = jokesConfiguration;
            _jokeSelector = jokeSelector;

            // Capture remote endpoint for logging
            _clientIdentifier = _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Main conversation loop for this client.
        /// Runs in a dedicated thread until the client disconnects or the conversation ends.
        /// </summary>
        public void Run()
        {
            try
            {
                Console.WriteLine($"[{_clientIdentifier}] Client connected.");

                // Start in the initial state
                IKnockKnockState? currentState = null;
                try
                {
                    currentState =
                        _serviceProvider.GetService(typeof(WaitingForJokeRequestState)) as IKnockKnockState;
                }
                catch (ObjectDisposedException)
                {
                    // Service provider was disposed, likely during server shutdown
                    Console.WriteLine($"[{_clientIdentifier}] Cannot start conversation: service provider was disposed");
                    _isConnected = false;
                    return;
                }

                // Run conversation state machine
                while (currentState != null && _isConnected)
                {
                    try
                    {
                        currentState = currentState.Handle(this, _jokesConfiguration, _jokeSelector);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"[{_clientIdentifier}] Client disconnected (I/O error): {ex.Message}");
                        _isConnected = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{_clientIdentifier}] Error in state '{currentState?.StateName}': {ex.Message}");
                        SendMessage("An error occurred. Please try again.");
                    }
                }

                Console.WriteLine($"[{_clientIdentifier}] Conversation ended.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_clientIdentifier}] Critical error: {ex.Message}");
            }
            finally
            {
                Cleanup();
            }
        }

        public void SendMessage(string message)
        {
            if (!_isConnected)
                return;

            try
            {
                Console.WriteLine($"[{_clientIdentifier}] Server: {message}");
                _streamWriter.WriteLine(message);
            }
            catch (IOException)
            {
                _isConnected = false;
            }
        }

        public string? ReadMessage()
        {
            if (!_isConnected)
                return null;

            try
            {
                var message = _streamReader.ReadLine();
                if (message != null)
                    Console.WriteLine($"[{_clientIdentifier}] Client: {message}");
                else
                    _isConnected = false;
                return message;
            }
            catch (IOException)
            {
                _isConnected = false;
                return null;
            }
        }

        public JokeModel? GetCurrentJoke()
        {
            return _currentJoke;
        }

        public void SetCurrentJoke(JokeModel joke)
        {
            _currentJoke = joke;
        }

        public string? GetClientIdentifier()
        {
            return _clientIdentifier;
        }

        private void Cleanup()
        {
            try
            {
                _streamReader?.Dispose();
                _streamWriter?.Dispose();
                _networkStream?.Dispose();
                _tcpClient?.Dispose();
                Console.WriteLine($"[{_clientIdentifier}] Resources cleaned up.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_clientIdentifier}] Error during cleanup: {ex.Message}");
            }
        }
    }
}
