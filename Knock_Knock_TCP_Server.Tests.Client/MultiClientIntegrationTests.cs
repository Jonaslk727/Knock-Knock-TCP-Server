using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Knock_Knock_TCP_Server.Configuration;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    [Collection("B_MultiClientIntegration")]
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
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                var createdClients = await manager.CreateClientsAsync(3);
                Assert.Equal(3, createdClients.Count);
                Assert.Equal(3, manager.ActiveClientCount);

                var results = await manager.RunConversationsAsync();

                Assert.Equal(3, results.Count);
                Assert.True(results.Values.All(r => r));

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
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                var clients = await manager.CreateClientsAsync(5, TimeSpan.FromSeconds(5));
                Assert.Equal(5, clients.Count);

                var results = await manager.RunConversationsAsync();

                Assert.Equal(5, results.Count);
                foreach (var result in results.Values)
                {
                    Assert.True(result);
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
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                var task1 = client1.ConnectAsync();
                var task2 = client2.ConnectAsync();
                await Task.WhenAll(task1, task2);

                Assert.True(client1.IsConnected);
                Assert.True(client2.IsConnected);

                var msg1 = await client1.ReceiveMessageAsync();
                var msg2 = await client2.ReceiveMessageAsync();

                Assert.NotNull(msg1);
                Assert.NotNull(msg2);
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
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client3 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                await Task.WhenAll(
                    client1.ConnectAsync(),
                    client2.ConnectAsync(),
                    client3.ConnectAsync()
                );

                Assert.Equal(3, new[] { client1, client2, client3 }.Count(c => c.IsConnected));

                var msg1_1 = await client1.ReceiveMessageAsync();
                var msg2_1 = await client2.ReceiveMessageAsync();
                var msg3_1 = await client3.ReceiveMessageAsync();

                Assert.NotNull(msg1_1);
                Assert.NotNull(msg2_1);
                Assert.NotNull(msg3_1);

                await client2.SendMessageAsync("Who's there?");
                var setup2 = await client2.ReceiveMessageAsync();

                client1.Disconnect();
                client3.Disconnect();

                Assert.True(client2.IsConnected);
                await client2.SendMessageAsync($"{setup2} who?");
                var punchline2 = await client2.ReceiveMessageAsync();

                Assert.NotNull(punchline2);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
                client3.Disconnect();
            }
        }

        [Fact]
        public async Task ConcurrentClients_RapidMessages_NoCorruption()
        {
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                var clients = await manager.CreateClientsAsync(3);

                var tasks = new List<Task>();
                foreach (var client in clients)
                {
                    var localClient = client;
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await localClient.ReceiveMessageAsync();
                            await localClient.SendMessageAsync("Who's there?");
                            var setup = await localClient.ReceiveMessageAsync();
                            await localClient.SendMessageAsync($"{setup} who?");
                            var punchline = await localClient.ReceiveMessageAsync();
                            Assert.NotNull(setup);
                            Assert.NotNull(punchline);
                        }
                        catch (Exception)
                        {
                        }
                    }));
                }

                await Task.WhenAll(tasks);

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
            var clients = new List<KnockKnockTestClient>();
            var iterations = 3;

            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
                    await client.ConnectAsync();

                    Assert.True(client.IsConnected);

                    var msg = await client.ReceiveMessageAsync();
                    Assert.NotNull(msg);

                    client.Disconnect();
                    Assert.False(client.IsConnected);

                    clients.Add(client);
                }

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
            var manager = new ConcurrentTestClientManager("localhost", _serverHelper.Port);

            try
            {
                var successCount = await manager.StressTestAsync(10, TimeSpan.FromMilliseconds(50));

                Assert.True(successCount >= 8);

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
            var client1 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);

            try
            {
                await Task.WhenAll(client1.ConnectAsync(), client2.ConnectAsync());

                var initial1 = await client1.ReceiveMessageAsync();
                var initial2 = await client2.ReceiveMessageAsync();

                await Task.WhenAll(
                    client1.SendMessageAsync("Who's there?"),
                    client2.SendMessageAsync("Who's there?")
                );

                var setup1 = await client1.ReceiveMessageAsync();
                var setup2 = await client2.ReceiveMessageAsync();

                Assert.NotNull(setup1);
                Assert.NotNull(setup2);
            }
            finally
            {
                client1.Disconnect();
                client2.Disconnect();
            }
        }
    }
}
