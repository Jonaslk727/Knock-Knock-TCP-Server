
using System;
using System.Collections.Generic;
using System.Text;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server
{
    /// <summary>
    /// Defines a state in the Knock Knock conversation state machine.
    /// Each state is responsible for:
    /// - Waiting for and reading a client message
    /// - Processing the message according to conversation protocol
    /// - Transitioning to the next state or ending the conversation
    /// 
    /// States now receive injected dependencies (IJokeSelector, IOptions) 
    /// rather than direct references to server or settings.
    /// </summary>
    internal interface IKnockKnockState
    {
        /// <summary>
        /// Process the current state of the conversation.
        /// This may read from client, send a response, and return the next state.
        /// </summary>
        /// <param name="context">The conversation context (I/O and state container)</param>
        /// <param name="jokesConfiguration">Injected jokes and strategy configuration</param>
        /// <param name="jokeSelector">Injected strategy for selecting jokes</param>
        /// <returns>The next state to transition to, or null if the conversation is finished</returns>
        IKnockKnockState? Handle(IConversationContext context, IOptions<JokesConfiguration> jokesConfiguration, IJokeSelector jokeSelector);

        /// <summary>
        /// Gets a descriptive name for this state (for logging and debugging).
        /// </summary>
        string StateName { get; }
    }
}

