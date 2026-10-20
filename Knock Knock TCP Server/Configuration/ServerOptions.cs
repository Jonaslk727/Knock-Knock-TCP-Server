namespace Knock_Knock_TCP_Server.Configuration
{
    /// <summary>
    /// Strongly typed options for server configuration.
    /// Bound from appsettings.json ["Server"] section via IOptions pattern.
    /// </summary>
    public class ServerOptions
    {
        /// <summary>
        /// The TCP port the server listens on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Maximum number of simultaneous client connections allowed.
        /// </summary>
        public int MaxClients { get; set; }
    }
}
