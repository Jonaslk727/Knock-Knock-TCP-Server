using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    /// <summary>
    /// Manages multiple concurrent test clients for stress and concurrency testing.
    /// </summary>
    public class ConcurrentTestClientManager
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger? _logger;
        private readonly List<KnockKnockTestClient> _clients = new();

        public int ActiveClientCount => _clients.Count(c => c.IsConnected);

        public ConcurrentTestClientManager(string host, int port, ILogger? logger = null)
        {
            _host = host;
            _port = port;
            _logger = logger;
        }

        /// <summary>
        /// Creates and connects multiple test clients concurrently.
        /// </summary>
        public async Task<List<KnockKnockTestClient>> CreateClientsAsync(int clientCount, TimeSpan? connectTimeout = null)
        {
            _logger?.LogInformation($"Creating {clientCount} concurrent clients...");
            var tasks = new List<Task>();

            for (int i = 0; i < clientCount; i++)
            {
                var client = new KnockKnockTestClient(_host, _port);
                _clients.Add(client);

                var task = client.ConnectAsync(connectTimeout ?? TimeSpan.FromSeconds(10))
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger?.LogError($"Client {i} connection failed: {t.Exception?.Message}");
                        }
                        else
                        {
                            _logger?.LogInformation($"Client {i} connected successfully");
                        }
                    });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            _logger?.LogInformation($"Active clients: {ActiveClientCount}/{clientCount}");
            return _clients.Where(c => c.IsConnected).ToList();
        }

        /// <summary>
        /// Runs conversations on all connected clients concurrently.
        /// </summary>
        public async Task<Dictionary<int, bool>> RunConversationsAsync()
        {
            _logger?.LogInformation($"Starting conversations on {ActiveClientCount} clients");
            var results = new Dictionary<int, bool>();
            var tasks = new List<Task>();

            int clientIndex = 0;
            foreach (var client in _clients.Where(c => c.IsConnected))
            {
                int index = clientIndex;
                var task = client.RunConversationAsync()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            results[index] = t.Result;
                        }
                        else
                        {
                            _logger?.LogError($"Client {index} conversation failed: {t.Exception?.Message}");
                            results[index] = false;
                        }
                    });
                tasks.Add(task);
                clientIndex++;
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// Disconnects all clients.
        /// </summary>
        public void DisconnectAll()
        {
            _logger?.LogInformation($"Disconnecting all {_clients.Count} clients");
            foreach (var client in _clients)
            {
                client.Disconnect();
            }
            _clients.Clear();
        }

        /// <summary>
        /// Rapidly connects and disconnects clients to stress test.
        /// </summary>
        public async Task<int> StressTestAsync(int connectionAttempts, TimeSpan delayBetweenAttempts)
        {
            _logger?.LogInformation($"Starting stress test: {connectionAttempts} connection attempts");
            int successCount = 0;

            for (int i = 0; i < connectionAttempts; i++)
            {
                try
                {
                    var client = new KnockKnockTestClient(_host, _port);
                    await client.ConnectAsync(TimeSpan.FromSeconds(5));

                    if (client.IsConnected)
                    {
                        successCount++;
                        client.Disconnect();
                    }

                    await Task.Delay(delayBetweenAttempts);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Attempt {i + 1} failed: {ex.Message}");
                }
            }

            _logger?.LogInformation($"Stress test completed: {successCount}/{connectionAttempts} successful");
            return successCount;
        }

        /// <summary>
        /// Disposes all resources.
        /// </summary>
        public void Dispose()
        {
            DisconnectAll();
        }
    }
}
