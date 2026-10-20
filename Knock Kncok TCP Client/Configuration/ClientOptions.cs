namespace Knock_Kncok_TCP_Client.Configuration
{
    /// <summary>
    /// Strongly typed options for client configuration.
    /// Bound from appsettings.json ["Connection"] section via IOptions pattern.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Connection configuration section.
        /// </summary>
        public ConnectionOptions Connection { get; set; } = new ConnectionOptions();

        /// <summary>
        /// Nested class for connection-specific settings.
        /// </summary>
        public class ConnectionOptions
        {
            /// <summary>
            /// The server IP address or hostname to connect to.
            /// </summary>
            public string ServerAddress { get; set; } = "Localhost";

            /// <summary>
            /// The TCP port the server listens on.
            /// </summary>
            public int Port { get; set; } = 5000;

            /// <summary>
            /// Maximum number of connection retry attempts.
            /// </summary>
            public int MaxRetries { get; set; } = 5;

            /// <summary>
            /// Initial delay in milliseconds between retry attempts.
            /// Increases exponentially with each retry.
            /// </summary>
            public int RetryDelayMs { get; set; } = 500;
        }
    }
}
