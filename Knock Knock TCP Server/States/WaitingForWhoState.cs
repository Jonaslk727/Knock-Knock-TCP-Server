using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.States
{
    /// <summary>
    /// Waiting for the client to respond with "Who's there?"
    /// Transitions to SendingPunchlineState if correct, stays in this state if incorrect.
    /// </summary>
    internal class WaitingForWhoState : IKnockKnockState
    {
        public string StateName => "WaitingForWho";

        public IKnockKnockState? Handle(
            IConversationContext context,
            IOptions<JokesConfiguration> jokesConfiguration,
            IJokeSelector jokeSelector)
        {
            string? message = context.ReadMessage();

            if (message?.Equals("Who's there?", StringComparison.OrdinalIgnoreCase) == true)
            {
                var currentJoke = context.GetCurrentJoke();
                if (currentJoke != null)
                {
                    context.SendMessage(currentJoke.Who);
                    return new SendingPunchlineState();
                }
                else
                {
                    context.SendMessage("An error occurred. Please request a joke again.");
                    return new WaitingForJokeRequestState();
                }
            }
            else
            {
                context.SendMessage("Please respond with 'Who's there?'");
                return this; // Stay in this state
            }
        }
    }
}
