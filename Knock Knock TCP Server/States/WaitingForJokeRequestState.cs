using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.States
{
    /// <summary>
    /// Initial state: waiting for the client to request a joke.
    /// Expects the client to send "joke".
    /// Transitions to WaitingForWhoState if valid, stays in this state if invalid.
    /// </summary>
    public class WaitingForJokeRequestState : IKnockKnockState
    {
        public string StateName => "WaitingForJokeRequest";

        public IKnockKnockState? Handle(
            IConversationContext context,
            IOptions<JokesConfiguration> jokesConfiguration,
            IJokeSelector jokeSelector)
        {
            // Send initial prompt to client
            context.SendMessage("Knock knock?");

            string? message = context.ReadMessage();

            if (message?.Equals("joke", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Select a joke using the strategy
                var selectedJoke = jokeSelector.SelectJoke(jokesConfiguration.Value.Jokes);
                context.SetCurrentJoke(selectedJoke);

                context.SendMessage("Knock! Knock!");
                return new WaitingForWhoState();
            }
            else
            {
                context.SendMessage("Please send 'joke' to request a knock knock joke.");
                return this; // Stay in this state
            }
        }
    }
}
