using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.States
{
    /// <summary>
    /// Waiting for the client to respond with "[Who] who?" to complete the setup.
    /// Sends the punchline and transitions to LoopOrExitState.
    /// </summary>
    public class SendingPunchlineState : IKnockKnockState
    {
        public string StateName => "SendingPunchline";

        public IKnockKnockState? Handle(
            IConversationContext context,
            IOptions<JokesConfiguration> jokesConfiguration,
            IJokeSelector jokeSelector)
        {
            string? message = context.ReadMessage();

            if (!string.IsNullOrWhiteSpace(message))
            {
                var currentJoke = context.GetCurrentJoke();
                if (currentJoke != null)
                {
                    context.SendMessage(currentJoke.Punchline);
                    return new LoopOrExitState();
                }
                else
                {
                    context.SendMessage("An error occurred. Please request a new joke.");
                    return new WaitingForJokeRequestState();
                }
            }

            return this; // Stay and wait for any non-empty message
        }
    }
}
