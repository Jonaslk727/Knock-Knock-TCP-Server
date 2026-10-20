using Knock_Knock_TCP_Server.Configuration;

namespace Knock_Knock_TCP_Server.States
{
    /// <summary>
    /// Represents the context and state of a single conversation with a client.
    /// Each ClientHandler instance maintains one IConversationContext.
    /// This abstraction decouples state objects from the ThreadServer or TcpClient.
    /// </summary>
    internal interface IConversationContext
    {
        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        void SendMessage(string message);

        /// <summary>
        /// Reads a message from the client.
        /// Returns null or empty string if disconnected.
        /// </summary>
        string? ReadMessage();

        /// <summary>
        /// Gets the currently selected joke for this conversation.
        /// </summary>
        JokeModel? GetCurrentJoke();

        /// <summary>
        /// Sets the currently selected joke for this conversation.
        /// </summary>
        void SetCurrentJoke(JokeModel joke);

        /// <summary>
        /// Gets the remote endpoint identifier for logging/debugging purposes.
        /// </summary>
        string? GetClientIdentifier();

        /// <summary>
        /// Returns true if the client is still connected.
        /// </summary>
        bool IsConnected { get; }
    }
}
