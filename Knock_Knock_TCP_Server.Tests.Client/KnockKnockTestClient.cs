using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    /// <summary>
    /// A reusable TCP client for testing the Knock-Knock server.
    /// Handles connection, message sending/receiving, and communication flow.
    /// </summary>
    public class KnockKnockTestClient
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger<KnockKnockTestClient>? _logger;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public KnockKnockTestClient(string host, int port, ILogger<KnockKnockTestClient>? logger = null)
        {
            _host = host;
            _port = port;
            _logger = logger;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public async Task ConnectAsync(TimeSpan? timeout = null)
        {
            try
            {
                _tcpClient = new TcpClient();
                _logger?.LogInformation($"Connecting to {_host}:{_port}...");

                var connectTask = _tcpClient.ConnectAsync(_host, _port);
                if (timeout.HasValue)
                {
                    if (!connectTask.Wait(timeout.Value))
                    {
                        throw new TimeoutException($"Connection timeout after {timeout.Value.TotalSeconds}s");
                    }
                }
                else
                {
                    await connectTask;
                }

                _networkStream = _tcpClient.GetStream();
                _logger?.LogInformation("Connected successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Connection failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected || _networkStream == null)
                throw new InvalidOperationException("Not connected to server");

            try
            {
                var data = Encoding.UTF8.GetBytes(message + "\n");
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();
                _logger?.LogInformation($"Sent: {message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Send failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Receives a message from the server.
        /// </summary>
        public async Task<string?> ReceiveMessageAsync(TimeSpan? timeout = null)
        {
            if (!IsConnected || _networkStream == null)
                throw new InvalidOperationException("Not connected to server");

            try
            {
                var buffer = new byte[4096];
                var readTask = _networkStream.ReadAsync(buffer, 0, buffer.Length);

                int bytesRead;
                if (timeout.HasValue)
                {
                    if (!readTask.Wait(timeout.Value))
                    {
                        throw new TimeoutException($"Receive timeout after {timeout.Value.TotalSeconds}s");
                    }
                    bytesRead = await readTask;
                }
                else
                {
                    bytesRead = await readTask;
                }

                if (bytesRead == 0)
                {
                    _logger?.LogInformation("Server closed connection");
                    return null;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                _logger?.LogInformation($"Received: {message}");
                return message;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Receive failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _networkStream?.Close();
                _tcpClient?.Close();
                _logger?.LogInformation("Disconnected");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Disconnect error: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs a complete Knock-Knock conversation flow.
        /// </summary>
        public async Task<bool> RunConversationAsync()
        {
            try
            {
                // Receive initial prompt
                var initialMessage = await ReceiveMessageAsync(TimeSpan.FromSeconds(20));
                if (initialMessage == null || !initialMessage.Contains("Knock knock?", StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogWarning($"Unexpected initial message: {initialMessage}");
                    return false;
                }

                // Send "Who's there?"
                await SendMessageAsync("Who's there?");

                // Receive the setup
                var setup = await ReceiveMessageAsync(TimeSpan.FromSeconds(20));
                _logger?.LogInformation($"Setup: {setup}");

                // Send "<Setup> who?"
                await SendMessageAsync($"{setup} who?");

                // Receive the punchline
                var punchline = await ReceiveMessageAsync(TimeSpan.FromSeconds(20));
                _logger?.LogInformation($"Punchline: {punchline}");

                // Send "Play again?"
                await SendMessageAsync("Play again?");

                // Receive response (should be next joke or goodbye)
                var response = await ReceiveMessageAsync(TimeSpan.FromSeconds(20));
                _logger?.LogInformation($"Server response: {response}");

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Conversation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}
