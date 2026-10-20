using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.States
{
    /// <summary>
    /// After a joke completes, ask if the client wants another joke.
    /// "yes" -> returns to WaitingForJokeRequestState
    /// "no"  -> returns null to end the conversation
    /// other -> asks for valid input and stays in this state
    /// </summary>
    internal class LoopOrExitState : IKnockKnockState
    {
        public string StateName => "LoopOrExit";

        public IKnockKnockState? Handle(
            IConversationContext context,
            IOptions<JokesConfiguration> jokesConfiguration,
            IJokeSelector jokeSelector)
        {
            context.SendMessage("Want another joke? (yes/no)");

            string? response = context.ReadMessage();

            if (response?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new WaitingForJokeRequestState();
            }
            else if (response?.Equals("no", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.SendMessage("Thanks for chatting! Goodbye!");
                return null; // End conversation
            }
            else
            {
                context.SendMessage("Please respond with 'yes' or 'no'.");
                return this; // Stay in this state
            }
        }
    }
}
