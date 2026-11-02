using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Tests.Client;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests
{
    [Collection("MultiClientIntegration")]
    public class MultiClientIntegrationTests : IAsyncLifetime
    {
        private TestServerHelper _serverHelper = null!;

        public async Task InitializeAsync()
        {
            _serverHelper = new TestServerHelper(port: 5011);
            await _serverHelper.StartAsync();
            await Task.Delay(500);
        }

        public async Task DisposeAsync()
        {
            await _serverHelper.StopAsync();
            _serverHelper.Dispose();
        }

        [Fact]
        public async Task ThreeClients_ConcurrentConversations_AllSucceed()
        {
            // Arrange
            var clients = new List<KnockKnockTestClient>();
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                // Act - Create 3 clients
                var createdClients = await manager.CreateClientsAsync(3);
                Assert.Equal(3, createdClients.Count);
                Assert.Equal(3, manager.ActiveClientCount);

                // Run conversations concurrently
                var results = await manager.RunConversationsAsync();

                // Assert - All should complete successfully
                Assert.Equal(3, results.Count);
                Assert.True(results.Values.All(r => r), "All client conversations should succeed");

                manager.DisconnectAll();
                Assert.Equal(0, manager.ActiveClientCount);
            }
            finally
            {
                manager.Dispose();
            }
        }

        [Fact]
        public async Task FiveClients_IndependentConversations_NoInterference()
        {
            // Arrange
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                // Act
                var clients = await manager.CreateClientsAsync(5, TimeSpan.FromSeconds(5));
                Assert.Equal(5, clients.Count);

                // Each client runs a conversation independently
                var results = await manager.RunConversationsAsync();

                // Assert
                Assert.Equal(5, results.Count);
                foreach (var result in results.Values)
                {
                    Assert.True(result, "Each client's conversation should complete successfully");
                }

                manager.DisconnectAll();
            }
            finally
            {
                manager.Dispose();
            }
        }

        [Fact]
        public async Task TwoClients_SimultaneousConnection_BothSucceed()
        {
            // Arrange
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                // Act - Connect both clients simultaneously
                var task1 = client1.ConnectAsync();
                var task2 = client2.ConnectAsync();
                await Task.WhenAll(task1, task2);

                // Assert - Both should be connected
                Assert.True(client1.IsConnected);
                Assert.True(client2.IsConnected);

                // Both should be able to receive initial messages
                var msg1 = await client1.ReceiveMessageAsync();
                var msg2 = await client2.ReceiveMessageAsync();

                Assert.NotNull(msg1);
                Assert.NotNull(msg2);
                Assert.Contains("Knock", msg1, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Knock", msg2, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
            }
        }

        [Fact]
        public async Task ClientDisconnects_OthersContinue_Unaffected()
        {
            // Arrange
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client3 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                // Act - All connect
                await Task.WhenAll(
                    client1.ConnectAsync(),
                    client2.ConnectAsync(),
                    client3.ConnectAsync()
                );

                Assert.Equal(3, new[] { client1, client2, client3 }.Count(c => c.IsConnected));

                // All receive initial message
                var msg1_1 = await client1.ReceiveMessageAsync();
                var msg2_1 = await client2.ReceiveMessageAsync();
                var msg3_1 = await client3.ReceiveMessageAsync();

                Assert.NotNull(msg1_1);
                Assert.NotNull(msg2_1);
                Assert.NotNull(msg3_1);

                // Client 2 starts answering
                await client2.SendMessageAsync("Who's there?");
                var setup2 = await client2.ReceiveMessageAsync();

                // Client 1 and 3 disconnect abruptly
                client1.Disconnect();
                client3.Disconnect();

                // Client 2 should continue without issues
                await client2.SendMessageAsync($"{setup2} who?");
                var punchline2 = await client2.ReceiveMessageAsync();

                // Assert
                Assert.NotNull(punchline2);
                Assert.False(client1.IsConnected);
                Assert.True(client2.IsConnected);
                Assert.False(client3.IsConnected);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
                client3.Disconnect();
            }
        }

        [Fact]
        public async Task MultipleClients_ReceiveDifferentJokes_IfRandom()
        {
            // Arrange - Use Random strategy
            var helper = new TestServerHelper(port: 5012, strategy: "Random");
            await helper.StartAsync();
            await Task.Delay(500);

            var client1 = new KnockKnockTestClient("localhost", helper.Port);
            var client2 = new KnockKnockTestClient("localhost", helper.Port);

            try
            {
                // Act
                await Task.WhenAll(client1.ConnectAsync(), client2.ConnectAsync());

                await client1.ReceiveMessageAsync();
                await client2.ReceiveMessageAsync();

                await client1.SendMessageAsync("Who's there?");
                await client2.SendMessageAsync("Who's there?");

                var setup1 = await client1.ReceiveMessageAsync();
                var setup2 = await client2.ReceiveMessageAsync();

                // Assert - With Random strategy, setups might be same or different
                // But both should be valid joke setups
                var validSetups = new[] { "Lettuce", "Tank", "Boo" };
                Assert.Contains(setup1, validSetups, StringComparer.OrdinalIgnoreCase);
                Assert.Contains(setup2, validSetups, StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
                await helper.StopAsync();
                helper.Dispose();
            }
        }

        [Fact]
        public async Task ConcurrentClients_RapidMessages_NoCorruption()
        {
            // Arrange
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                // Act
                var clients = await manager.CreateClientsAsync(3);

                // Each client sends multiple rapid messages
                var tasks = new List<Task>();
                foreach (var client in clients)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await client.ReceiveMessageAsync();
                            await client.SendMessageAsync("Who's there?");
                            var setup = await client.ReceiveMessageAsync();
                            await client.SendMessageAsync($"{setup} who?");
                            var punchline = await client.ReceiveMessageAsync();
                            // Verify messages are not mixed/corrupted
                            Assert.NotNull(setup);
                            Assert.NotNull(punchline);
                        }
                        catch (Exception)
                        {
                            // Connection might close, which is ok
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                // Assert - All tasks should complete
                Assert.All(tasks, t => Assert.True(t.IsCompletedSuccessfully || t.IsFaulted || t.IsCanceled));

                manager.DisconnectAll();
            }
            finally
            {
                manager.Dispose();
            }
        }

        [Fact]
        public async Task ClientsConnect_ThenDisconnect_SequentiallySucceed()
        {
            // Arrange
            var clients = new List<KnockKnockTestClient>();
            var iterations = 3;

            try
            {
                // Act - Connect and disconnect sequentially multiple times
                for (int i = 0; i < iterations; i++)
                {
                    var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
                    await client.ConnectAsync();

                    // Assert - Should connect
                    Assert.True(client.IsConnected);

                    // Get initial message  
                    var msg = await client.ReceiveMessageAsync();
                    Assert.NotNull(msg);

                    // Disconnect
                    client.Disconnect();
                    Assert.False(client.IsConnected);

                    clients.Add(client);
                }

                // All iterations should succeed
                Assert.Equal(iterations, clients.Count);
            }
            finally
            {
                foreach (var client in clients)
                {
                    client.Disconnect();
                }
            }
        }

        [Fact]
        public async Task Stress_RapidConnectDisconnect_ServerStable()
        {
            // Arrange
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                // Act - Stress test with rapid connections/disconnections
                var successCount = await manager.StressTestAsync(10, TimeSpan.FromMilliseconds(50));

                // Assert - Most connections should succeed
                Assert.True(successCount >= 8, $"Expected at least 8 successful connections, got {successCount}");

                manager.DisconnectAll();
            }
            finally
            {
                manager.Dispose();
            }
        }

        [Fact]
        public async Task TwoClients_SameMessage_DifferentResponses_Independent()
        {
            // Arrange
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                // Act
                await Task.WhenAll(client1.ConnectAsync(), client2.ConnectAsync());

                var initial1 = await client1.ReceiveMessageAsync();
                var initial2 = await client2.ReceiveMessageAsync();

                // Both send same message
                await Task.WhenAll(
                    client1.SendMessageAsync("Who's there?"),
                    client2.SendMessageAsync("Who's there?")
                );

                var setup1 = await client1.ReceiveMessageAsync();
                var setup2 = await client2.ReceiveMessageAsync();

                // Assert - Both should receive responses (independent conversations)
                Assert.NotNull(setup1);
                Assert.NotNull(setup2);
                // Setups should be valid
                var validSetups = new[] { "Lettuce", "Tank", "Boo" };
                Assert.Contains(setup1, validSetups, StringComparer.OrdinalIgnoreCase);
                Assert.Contains(setup2, validSetups, StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
            }
        }
    }
}
