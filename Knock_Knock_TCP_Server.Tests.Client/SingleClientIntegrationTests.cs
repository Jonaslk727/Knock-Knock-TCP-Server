using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Knock_Knock_TCP_Server.Configuration;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    [Collection("A_SingleClientIntegration")]
    public class SingleClientIntegrationTests : IAsyncLifetime
    {
        private TestServerHelper _serverHelper = null!;

        public async Task InitializeAsync()
        {
            _serverHelper = new TestServerHelper(port: 5010);
            await _serverHelper.StartAsync();
            await Task.Delay(500);
        }

        public async Task DisposeAsync()
        {
            await _serverHelper.StopAsync();
            _serverHelper.Dispose();
        }

        [Fact]
        public async Task SingleClient_CompletesFullConversation_Success()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);

            await client.ConnectAsync(TimeSpan.FromSeconds(5));
            Assert.True(client.IsConnected);

            var initial = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(initial);
            Assert.Contains("Knock", initial, StringComparison.OrdinalIgnoreCase);

            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(setup);

            await client.SendMessageAsync($"{setup} who?");
            var punchline = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(punchline);

            await client.SendMessageAsync("no");
            var goodbye = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(goodbye);

            client.Disconnect();
            Assert.False(client.IsConnected);
        }

        [Fact]
        public async Task SingleClient_SendsMultipleJokes_Success()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);

            await client.ConnectAsync(TimeSpan.FromSeconds(5));

            await CompleteJokeCycle(client, "yes");
            await CompleteJokeCycle(client, "no");

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_WrongAnswerStaysInState_Recovery()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            var initial = await client.ReceiveMessageAsync();
            Assert.Contains("Knock", initial, StringComparison.OrdinalIgnoreCase);

            await client.SendMessageAsync("I don't know");
            var response = await client.ReceiveMessageAsync();
            Assert.NotNull(response);

            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            Assert.NotNull(setup);

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_ReceivesValidJoke_FromConfiguration()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            var initial = await client.ReceiveMessageAsync();
            await client.SendMessageAsync("joke");
            var knockKnock = await client.ReceiveMessageAsync();
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();

            var validSetups = new[] { "Lettuce", "Tank", "Boo", "Interrupt", "Dozen" };
            Assert.True(validSetups.Contains(setup, StringComparer.OrdinalIgnoreCase),
                $"Setup '{setup}' should be one of the configured jokes");

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_ReceivesPunchline_MatchingSetup()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            await client.ReceiveMessageAsync();
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            await client.SendMessageAsync($"{setup} who?");
            var punchline = await client.ReceiveMessageAsync();

            Assert.NotNull(punchline);
            Assert.NotEmpty(punchline);
            Assert.NotEqual(setup, punchline);

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_GracefulDisconnect_CleansUpResources()
        {
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            await client.ReceiveMessageAsync();
            client.Disconnect();

            Assert.False(client.IsConnected);

            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client2.ConnectAsync();
            Assert.True(client2.IsConnected);
            client2.Disconnect();
        }

        private async Task CompleteJokeCycle(KnockKnockTestClient client, string yesOrNo)
        {
            var initial = await client.ReceiveMessageAsync();
            Assert.NotNull(initial);

            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            Assert.NotNull(setup);

            await client.SendMessageAsync($"{setup} who?");
            var punchline = await client.ReceiveMessageAsync();
            Assert.NotNull(punchline);

            await client.SendMessageAsync(yesOrNo);
            var response = await client.ReceiveMessageAsync();
            Assert.NotNull(response);
        }
    }
}
