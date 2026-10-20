using Microsoft.Extensions.Options;

namespace Knock_Kncok_TCP_Client.Configuration.Validation
{
    /// <summary>
    /// Validator for ClientOptions configuration.
    /// Ensures connection settings are valid at startup.
    /// </summary>
    public class ClientOptionsValidator : IValidateOptions<ClientOptions>
    {
        /// <summary>
        /// Validates the ClientOptions configuration.
        /// </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The ClientOptions instance to validate.</param>
        /// <returns>A ValidateOptionsResult indicating success or failure with error message.</returns>
        public ValidateOptionsResult Validate(string? name, ClientOptions options)
        {
            if (options?.Connection == null)
            {
                return ValidateOptionsResult.Fail("Connection options must be configured.");
            }

            var connection = options.Connection;

            if (string.IsNullOrWhiteSpace(connection.ServerAddress))
            {
                return ValidateOptionsResult.Fail("ServerAddress cannot be null or empty.");
            }

            if (connection.Port <= 0 || connection.Port > 65535)
            {
                return ValidateOptionsResult.Fail("Port must be between 1 and 65535.");
            }

            if (connection.MaxRetries < 0)
            {
                return ValidateOptionsResult.Fail("MaxRetries cannot be negative.");
            }

            if (connection.RetryDelayMs < 0)
            {
                return ValidateOptionsResult.Fail("RetryDelayMs cannot be negative.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
