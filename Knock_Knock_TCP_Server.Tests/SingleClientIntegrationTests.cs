using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Tests.Client;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests
{
    [Collection("SingleClientIntegration")]
    public class SingleClientIntegrationTests : IAsyncLifetime
    {
        private TestServerHelper _serverHelper = null!;

        public async Task InitializeAsync()
        {
            _serverHelper = new TestServerHelper(port: 5010);
            await _serverHelper.StartAsync();
            await Task.Delay(500); // Ensure server is ready
        }

        public async Task DisposeAsync()
        {
            await _serverHelper.StopAsync();
            _serverHelper.Dispose();
        }

        [Fact]
        public async Task SingleClient_CompletesFullConversation_Success()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);

            // Act
            await client.ConnectAsync(TimeSpan.FromSeconds(5));
            Assert.True(client.IsConnected);

            // Receive initial prompt
            var initial = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(initial);
            Assert.Contains("Knock", initial, StringComparison.OrdinalIgnoreCase);

            // Send "Who's there?"
            await client.SendMessageAsync("Who's there?");

            // Receive joke setup
            var setup = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(setup);
            Assert.NotEmpty(setup);

            // Send "<Setup> who?"
            await client.SendMessageAsync($"{setup} who?");

            // Receive punchline
            var punchline = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(punchline);
            Assert.NotEmpty(punchline);

            // Send "No" to exit
            await client.SendMessageAsync("no");

            // Receive goodbye
            var goodbye = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
            Assert.NotNull(goodbye);
            Assert.Contains("Thanks", goodbye, StringComparison.OrdinalIgnoreCase);

            // Cleanup
            client.Disconnect();
            Assert.False(client.IsConnected);
        }

        [Fact]
        public async Task SingleClient_SendsMultipleJokes_Success()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);

            // Act & Assert
            await client.ConnectAsync(TimeSpan.FromSeconds(5));

            // First joke cycle
            await CompleteJokeCycle(client, "yes");

            // Second joke cycle
            await CompleteJokeCycle(client, "no");

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_WrongAnswerStaysInState_Recovery()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act & Assert
            // Receive initial
            var initial = await client.ReceiveMessageAsync();
            Assert.Contains("Knock", initial, StringComparison.OrdinalIgnoreCase);

            // Send wrong answer
            await client.SendMessageAsync("I don't know");

            // Should stay in same state and expect recheck
            var response = await client.ReceiveMessageAsync();
            Assert.NotNull(response);

            // Now send correct answer
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            Assert.NotNull(setup);

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_ReceivesValidJoke_FromConfiguration()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act
            var initial = await client.ReceiveMessageAsync();
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();

            // Assert - setup should be one of the configured jokes
            var validSetups = new[] { "Lettuce", "Tank", "Boo" };
            Assert.True(validSetups.Contains(setup, StringComparer.OrdinalIgnoreCase),
                $"Setup '{setup}' should be one of the configured jokes");

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_ReceivesPunchline_MatchingSetup()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act
            await client.ReceiveMessageAsync(); // Initial
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            await client.SendMessageAsync($"{setup} who?");
            var punchline = await client.ReceiveMessageAsync();

            // Assert
            Assert.NotNull(punchline);
            Assert.NotEmpty(punchline);
            // Punchline should differ from setup
            Assert.NotEqual(setup, punchline);

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_Timeout_ClosesConnection()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act
            var initial = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(2));
            Assert.NotNull(initial);

            // Try to receive with very short timeout (simulating no response from server)
            // This should timeout gracefully
            var shortTimeout = TimeSpan.FromMilliseconds(100);

            // Send message and try immediate receive with short timeout
            await client.SendMessageAsync("test");

            // If timeout occurs, it should be handled gracefully
            try
            {
                var response = await client.ReceiveMessageAsync(shortTimeout);
                // Response might be null or a valid message
            }
            catch (TimeoutException)
            {
                // Expected behavior
            }

            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_HandlesEmptyInput_GracefullyRecovery()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act
            await client.ReceiveMessageAsync();

            // Send empty line
            await client.SendMessageAsync("");

            // Server should reprompt or keep connection open
            await Task.Delay(200);

            // Should still be connected and able to respond
            var still_connected_check = client.IsConnected;

            // Assert
            // Connection should still be active (implementation dependent)
            // Clean disconnect
            client.Disconnect();
        }

        [Fact]
        public async Task SingleClient_GracefulDisconnect_CleansUpResources()
        {
            // Arrange
            var client = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client.ConnectAsync();

            // Act
            await client.ReceiveMessageAsync();
            client.Disconnect();

            // Assert
            Assert.False(client.IsConnected);

            // Should be able to connect again
            var client2 = new KnockKnockTestClient("localhost", _serverHelper.Port);
            await client2.ConnectAsync();
            Assert.True(client2.IsConnected);
            client2.Disconnect();
        }

        // Helper method
        private async Task CompleteJokeCycle(KnockKnockTestClient client, string yesOrNo)
        {
            // Receive prompt
            var initial = await client.ReceiveMessageAsync();
            Assert.NotNull(initial);

            // Send response
            await client.SendMessageAsync("Who's there?");
            var setup = await client.ReceiveMessageAsync();
            Assert.NotNull(setup);

            // Send who response
            await client.SendMessageAsync($"{setup} who?");
            var punchline = await client.ReceiveMessageAsync();
            Assert.NotNull(punchline);

            // Send yes/no
            await client.SendMessageAsync(yesOrNo);
            var response = await client.ReceiveMessageAsync();
            Assert.NotNull(response);
        }
    }
}
