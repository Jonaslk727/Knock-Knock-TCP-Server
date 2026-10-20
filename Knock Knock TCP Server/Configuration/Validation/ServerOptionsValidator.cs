using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.Configuration.Validation
{
    /// <summary>
    /// Validates ServerOptions for correct values at startup.
    /// </summary>
    internal class ServerOptionsValidator : IValidateOptions<ServerOptions>
    {
        public ValidateOptionsResult Validate(string? name, ServerOptions options)
        {
            if (options == null)
                return ValidateOptionsResult.Fail("ServerOptions is null");

            if (options.Port < 1024 || options.Port > 65535)
                return ValidateOptionsResult.Fail(
                    $"Server.Port must be between 1024 and 65535. Got: {options.Port}");

            if (options.MaxClients <= 0)
                return ValidateOptionsResult.Fail(
                    $"Server.MaxClients must be greater than 0. Got: {options.MaxClients}");

            return ValidateOptionsResult.Success;
        }
    }
}
