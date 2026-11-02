using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    /// <summary>
    /// Simulates various client behaviors for testing error handling and edge cases.
    /// </summary>
    public class ClientBehaviorSimulator
    {
        private readonly KnockKnockTestClient _client;

        public ClientBehaviorSimulator(KnockKnockTestClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Simulates a client sending a malformed/invalid message.
        /// </summary>
        public async System.Threading.Tasks.Task SendMalformedMessageAsync()
        {
            await _client.SendMessageAsync("\x00\x01\x02Invalid");
        }

        /// <summary>
        /// Simulates a client sending the wrong answer to a prompt.
        /// </summary>
        public async System.Threading.Tasks.Task SendWrongAnswerAsync()
        {
            var initialMessage = await _client.ReceiveMessageAsync();
            // Send wrong response to "Who's there?"
            await _client.SendMessageAsync("Wrong answer!");

            // Try to continue after wrong answer
            await _client.SendMessageAsync("Whatever");
        }

        /// <summary>
        /// Simulates a client sending random noise.
        /// </summary>
        public async System.Threading.Tasks.Task SendRandomNoiseAsync(int messageCount = 5)
        {
            var random = new Random();
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

            for (int i = 0; i < messageCount; i++)
            {
                var noise = new string(new char[new Random().Next(5, 20)]
                    .Select(_ => characters[random.Next(characters.Length)]).ToArray());
                await _client.SendMessageAsync(noise);
            }
        }

        /// <summary>
        /// Simulates a client that disconnects abruptly after a delay.
        /// </summary>
        public async System.Threading.Tasks.Task DisconnectAbruptlyAfterDelayAsync(TimeSpan delay)
        {
            await System.Threading.Tasks.Task.Delay(delay);
            _client.Disconnect();
        }

        /// <summary>
        /// Simulates a client sending extremely long messages.
        /// </summary>
        public async System.Threading.Tasks.Task SendLongMessageAsync()
        {
            var longMessage = new string('A', 10000);
            await _client.SendMessageAsync(longMessage);
        }

        /// <summary>
        /// Simulates a client that sends messages very rapidly.
        /// </summary>
        public async System.Threading.Tasks.Task SendRapidMessagesAsync(int messageCount = 10)
        {
            for (int i = 0; i < messageCount; i++)
            {
                try
                {
                    await _client.SendMessageAsync($"Rapid message {i}");
                }
                catch
                {
                    // Ignore errors during rapid fire
                }
            }
        }

        /// <summary>
        /// Simulates a client that goes silent (doesn't respond).
        /// </summary>
        public async System.Threading.Tasks.Task GotoSilentAsync(TimeSpan silenceDuration)
        {
            await System.Threading.Tasks.Task.Delay(silenceDuration);
            // Connection stays open but no messages sent
        }
    }
}
